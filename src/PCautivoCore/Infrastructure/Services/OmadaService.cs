using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Infrastructure.Models.Omada;
using PCautivoCore.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PCautivoCore.Infrastructure.Services;

/// <summary>
/// Servicio para autorizar clientes en Omada OC-300.
/// extPortal/auth requiere sesión de admin activa (Csrf-Token).
/// El token se cachea en memoria y se renueva automáticamente al expirar.
/// </summary>
public class OmadaService(
    IOptions<OmadaSettings> options,
    ILogger<OmadaService> logger) : IOmadaService
{
    private readonly OmadaSettings _settings = options.Value;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // Cache del token de admin (válido ~2h en OC-300)
    private string? _adminToken;
    private string? _sessionCookie;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _loginLock = new(1, 1);

    // URL de login (puerto 443) separada de la URL de API (puerto 8043)
    private string LoginBaseUrl => !string.IsNullOrWhiteSpace(_settings.LoginBaseUrl)
        ? _settings.LoginBaseUrl
        : _settings.BaseUrl;

    // ──────────────────────────────────────────────────────────────────────────
    // Login de admin (con caché)
    // ──────────────────────────────────────────────────────────────────────────

    private HttpClientHandler BuildHandler() => new()
    {
        ServerCertificateCustomValidationCallback = _settings.IgnoreSslErrors
            ? (_, _, _, _) => true
            : HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };

    private static string NormMac(string mac) =>
        mac.Replace(":", "-").Replace(".", "-").ToUpperInvariant();

    private async Task<string?> GetAdminTokenAsync()
    {
        if (_adminToken != null && DateTime.UtcNow < _tokenExpiry)
            return _adminToken;

        await _loginLock.WaitAsync();
        try
        {
            if (_adminToken != null && DateTime.UtcNow < _tokenExpiry)
                return _adminToken;

            var cookies = new System.Net.CookieContainer();
            using var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = _settings.IgnoreSslErrors
                    ? (_, _, _, _) => true
                    : HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                CookieContainer  = cookies,
                UseCookies       = true
            };
            using var client = new HttpClient(handler) { BaseAddress = new Uri(LoginBaseUrl) };

            var loginBody = JsonSerializer.Serialize(new
            {
                username = _settings.AdminUsername,
                password = _settings.AdminPassword
            });

            var loginUrl = $"/{_settings.ControllerId}/api/v2/login";

            using var req = new HttpRequestMessage(HttpMethod.Post, loginUrl)
            {
                Content = new StringContent(loginBody, System.Text.Encoding.UTF8, "application/json")
            };

            var resp = await client.SendAsync(req);
            var raw  = await resp.Content.ReadAsStringAsync();

            var parsed = JsonSerializer.Deserialize<OmadaLoginResponse>(raw, _jsonOptions);
            if (parsed?.ErrorCode != 0 || parsed.Result?.Token is null)
            {
                logger.LogWarning("Omada admin login falló. ErrorCode: {Code}", parsed?.ErrorCode);
                return null;
            }

            _adminToken   = parsed.Result.Token;
            _sessionCookie = cookies.GetCookies(new Uri(LoginBaseUrl))["TPOMADA_SESSIONID"]?.Value;
            _tokenExpiry  = DateTime.UtcNow.AddMinutes(90);

            // Pequeña pausa para que el OC-300 propague la sesión de admin
            // antes de recibir la petición de autorización del cliente.
            // Sin este delay, la primera autorización tras un nuevo login de admin
            // llega antes de que Omada haya registrado la sesión, causando rechazo.
            await Task.Delay(600);

            return _adminToken;
        }
        finally
        {
            _loginLock.Release();
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Autorizar cliente via extPortal/auth
    // ──────────────────────────────────────────────────────────────────────────

    public async Task<bool> AuthorizeClientAsync(
        string target,
        int targetPort,
        string clientMac,
        string apMac,
        string ssidName,
        int radioId = 0,
        string? vid = null,
        string scheme = "https",
        string? site = null,
        long? t = null,
        string? redirectUrl = null,
        string? clientIp = null)
    {
        // Endpoint correcto confirmado desde el botón "Autorizar" del panel Omada:
        // POST /api/v2/sites/{siteId}/cmd/clients/{mac}/auth  → errorCode: 0
        // extPortal/auth siempre devolvía -1 (no es el endpoint de autorización correcto).
        
        // Resolver siteId si es necesario (si es nombre en lugar de ID técnico)
        var initialSiteId = !string.IsNullOrWhiteSpace(site) ? site : _settings.Site;
        var resolvedSiteId = initialSiteId;
        
        // Si parece ser un nombre (sin hífenes/dígitos), intentar resolver a ID técnico
        if (!string.IsNullOrEmpty(initialSiteId) && !initialSiteId.Any(c => char.IsDigit(c) || c == '-'))
        {
            try
            {
                using var handlerForResolve = BuildHandler();
                using var clientForResolve = new HttpClient(handlerForResolve) { BaseAddress = new Uri(LoginBaseUrl) };
                var adminTokenForResolve = await GetAdminTokenAsync();
                if (adminTokenForResolve != null)
                {
                    clientForResolve.DefaultRequestHeaders.Add("Csrf-Token", adminTokenForResolve);
                    resolvedSiteId = await ResolveHotspotSiteIdAsync(clientForResolve, initialSiteId, CancellationToken.None);
                }
            }
            catch
            {
                // Si la resolución falla, usar el valor original
                resolvedSiteId = initialSiteId;
            }
        }
        
        for (int attempt = 1; attempt <= 6; attempt++)
        {
            var adminToken = await GetAdminTokenAsync();
            if (adminToken is null) return false;

            try
            {
                using var handler = BuildHandler();
                using var client  = new HttpClient(handler) { BaseAddress = new Uri(LoginBaseUrl) };
                client.DefaultRequestHeaders.Add("Csrf-Token", adminToken);
                if (!string.IsNullOrWhiteSpace(_sessionCookie))
                    client.DefaultRequestHeaders.Add("Cookie", $"TPOMADA_SESSIONID={_sessionCookie}");

                var siteId   = resolvedSiteId;
                var mac      = NormMac(clientMac);
                var authPath = $"/{_settings.ControllerId}/api/v2/sites/{siteId}/cmd/clients/{mac}/auth";

                var payloadJson = JsonSerializer.Serialize(new { time = 28800 }, _jsonOptions);

                using var req = new HttpRequestMessage(HttpMethod.Post, authPath)
                {
                    Content = new StringContent(payloadJson, System.Text.Encoding.UTF8, "application/json")
                };

                var authResponse = await client.SendAsync(req);
                var authRaw      = await authResponse.Content.ReadAsStringAsync();

                // Si hay error HTTP de sesión expirada → renovar token y reintentar
                if (authResponse.StatusCode is System.Net.HttpStatusCode.Unauthorized
                                            or System.Net.HttpStatusCode.Forbidden)
                {
                    //logger.LogWarning("Omada token expirado (HTTP {Status}), renovando...", (int)authResponse.StatusCode);
                    _adminToken  = null;
                    _tokenExpiry = DateTime.MinValue;
                    await Task.Delay(500);
                    continue;
                }

                if (!authResponse.IsSuccessStatusCode)
                {
                    //logger.LogWarning("Omada auth HTTP error {Status}", (int)authResponse.StatusCode);
                    return false;
                }

                var result  = JsonSerializer.Deserialize<OmadaBaseResponse>(authRaw, _jsonOptions);
                bool success = result?.ErrorCode == 0;

                if (success) return true;

                // -41010: la sesión pendiente del portal ya no existe (el dispositivo fue
                // reemplazado por una sesión nueva o ya fue autorizado previamente).
                // Verificar si el cliente ya tiene internet activo.
                if (result?.ErrorCode == -41010)
                {
                    //logger.LogWarning("Omada -41010: sesión pendiente no existe para {Mac}. Verificando si ya está autorizado...", clientMac);
                    bool alreadyAuth = await IsClientAuthorizedAsync(NormMac(clientMac), resolvedSiteId, adminToken);
                    if (alreadyAuth)
                    {
                        //logger.LogInformation("Cliente {Mac} ya está autorizado en Omada. Tratando como éxito.", clientMac);
                        return true;
                    }
                }

                // Omada rechazó la petición (errorCode != 0).
                // NO hacemos nuevo login — un login extra invalidaría la sesión activa.
                // El OC-300 puede tomar hasta ~7s en registrar la sesión pendiente del cliente
                // después del redirect inicial. Esperamos y reintentamos con el mismo token.
                if (attempt < 6)
                {
                    //logger.LogWarning(
                    //    "Omada auth errorCode {Code} en intento {Attempt}/6. " +
                    //    "Esperando 1.5s y reintentando con mismo token...",
                    //    result?.ErrorCode, attempt);
                    await Task.Delay(1500);
                    continue;
                }

                //logger.LogWarning("Omada auth rechazo definitivo. ErrorCode: {Code}, Msg: {Msg}", result?.ErrorCode, result?.Message);
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al autorizar en Omada. ClientMac: {Mac}", clientMac);
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Consulta la API OpenAPI de Omada para verificar si un cliente ya tiene
    /// authStatus >= 1 (autorizado con acceso a internet activo).
    /// </summary>
    private async Task<bool> IsClientAuthorizedAsync(string normalizedMac, string siteId, string adminToken)
    {
        try
        {
            using var handler = BuildHandler();
            using var client  = new HttpClient(handler) { BaseAddress = new Uri(LoginBaseUrl) };
            client.DefaultRequestHeaders.Add("Csrf-Token", adminToken);
            if (!string.IsNullOrWhiteSpace(_sessionCookie))
                client.DefaultRequestHeaders.Add("Cookie", $"TPOMADA_SESSIONID={_sessionCookie}");

            // Usar la MAC en formato con guiones para filtrar
            var mac = Uri.EscapeDataString(normalizedMac);
            var path = $"/openapi/v2/{_settings.ControllerId}/sites/{siteId}/clients?mac={mac}";

            var resp = await client.GetAsync(path);
            if (!resp.IsSuccessStatusCode) return false;

            var raw    = await resp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<OmadaClientsResponse>(raw, _jsonOptions);

            var clientInfo = parsed?.Result?.Data?.FirstOrDefault(c =>
                string.Equals(c.Mac, normalizedMac, StringComparison.OrdinalIgnoreCase));

            // authStatus >= 1 significa que el cliente ya fue autorizado
            return clientInfo is { Active: true, AuthStatus: >= 1 };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al verificar estado del cliente {Mac} en Omada.", normalizedMac);
            return false;
        }
    }

    public async Task<string?> PortalAuthAsync(
        string username,
        string password,
        string clientMac,
        string apMac,
        string ssidName,
        int radioId = 0,
        string? originUrl = null)
    {
        try
        {
            using var handler = BuildHandler();
            using var client  = new HttpClient(handler) { BaseAddress = new Uri(_settings.BaseUrl) };

            var payload = new OmadaPortalAuthRequest
            {
                AuthType     = 2,
                LocalUser    = username,
                LocalUserPsw = password,
                ClientMac    = NormMac(clientMac),
                ApMac        = NormMac(apMac),
                SsidName     = ssidName,
                RadioId      = radioId,
                OriginUrl    = originUrl ?? string.Empty
            };

            var payloadJson = JsonSerializer.Serialize(payload, _jsonOptions);

            using var req = new HttpRequestMessage(HttpMethod.Post, "/portal/auth")
            {
                Content = new StringContent(payloadJson, System.Text.Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(req);
            var raw      = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Omada portal/auth HTTP error {Status}", (int)response.StatusCode);
                return null;
            }

            var result = JsonSerializer.Deserialize<OmadaPortalAuthResponse>(raw, _jsonOptions);
            if (result?.ErrorCode != 0)
            {
                logger.LogWarning("Omada portal/auth rechazo. ErrorCode: {Code}", result?.ErrorCode);
                return null;
            }

            return result.Result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error en Omada portal/auth. ClientMac: {Mac}", clientMac);
            return null;
        }
    }

    public async Task<IReadOnlyList<OmadaHotspotClientSession>> GetHotspotClientSessionsAsync(
        string siteId,
        long timeStartMs,
        long timeEndMs,
        int pageSize = 200,
        CancellationToken cancellationToken = default)
    {
        var sessions = new List<OmadaHotspotClientSession>();

        var adminToken = await GetAdminTokenAsync();
        if (adminToken is null)
        {
            logger.LogWarning("No se pudo obtener token admin para consultar sesiones de hotspot.");
            return sessions;
        }

        using var handler = BuildHandler();
        using var client = new HttpClient(handler) { BaseAddress = new Uri(LoginBaseUrl) };
        client.DefaultRequestHeaders.Add("Csrf-Token", adminToken);
        if (!string.IsNullOrWhiteSpace(_sessionCookie))
        {
            client.DefaultRequestHeaders.Add("Cookie", $"TPOMADA_SESSIONID={_sessionCookie}");
        }

        var resolvedSiteId = await ResolveHotspotSiteIdAsync(client, siteId, cancellationToken);
        var escapedSiteId = Uri.EscapeDataString(resolvedSiteId);

        var currentPage = 1;

        while (!cancellationToken.IsCancellationRequested)
        {
            var path =
                $"/{_settings.ControllerId}/api/v2/hotspot/sites/{escapedSiteId}/clients" +
                $"?currentPage={currentPage}" +
                $"&currentPageSize={pageSize}" +
                $"&filters.timeStart={timeStartMs}" +
                $"&filters.timeEnd={timeEndMs}";

            var response = await client.GetAsync(path, cancellationToken);
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
            {
                logger.LogWarning("Omada hotspot clients devolvió {Status}.", (int)response.StatusCode);
                break;
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Omada hotspot clients HTTP {Status}. Body: {Body}", (int)response.StatusCode, raw);
                break;
            }

            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            if (root.TryGetProperty("errorCode", out var errCode) && errCode.GetInt32() != 0)
            {
                logger.LogWarning("Omada hotspot clients errorCode={Code}. Body: {Body}", errCode.GetInt32(), raw);
                break;
            }

            if (!TryGetArray(root, out var items))
            {
                break;
            }

            var addedThisPage = 0;

            foreach (var item in items.EnumerateArray())
            {
                var mac =
                    GetString(item, "mac") ??
                    GetString(item, "clientMac") ??
                    GetString(item, "macAddress");

                if (string.IsNullOrWhiteSpace(mac))
                {
                    continue;
                }

                var startMs =
                    GetLong(item, "start") ??
                    GetLong(item, "timeStart") ??
                    GetLong(item, "startTime") ??
                    GetLong(item, "authTime");

                var endMs =
                    GetLong(item, "end") ??
                    GetLong(item, "timeEnd") ??
                    GetLong(item, "endTime");

                var durationSeconds =
                    GetInt(item, "duration") ??
                    GetInt(item, "durationSeconds") ??
                    ParseDurationToSeconds(GetString(item, "duration"));

                // Si Omada no manda duration explícito, lo calculamos con start/end.
                if (!durationSeconds.HasValue && startMs.HasValue && endMs.HasValue && endMs > startMs)
                {
                    durationSeconds = (int)((endMs.Value - startMs.Value) / 1000L);
                }

                // Si no vino start pero sí end + duration, reconstruimos start para registrar ENTRADA/SALIDA.
                if (!startMs.HasValue && endMs.HasValue && durationSeconds.HasValue)
                {
                    startMs = endMs.Value - (durationSeconds.Value * 1000L);
                }

                DateTime? startUtc = null;
                if (startMs.HasValue)
                {
                    startUtc = DateTimeOffset.FromUnixTimeMilliseconds(startMs.Value).UtcDateTime;
                }

                sessions.Add(new OmadaHotspotClientSession
                {
                    Id = GetString(item, "id")
                        ?? GetString(item, "sessionId")
                        ?? GetString(item, "sessionID"),
                    ClientMac = NormMac(mac),
                    ClientIp = GetString(item, "ip") ?? GetString(item, "clientIp"),
                    ClientName = GetString(item, "name") ?? GetString(item, "clientName") ?? GetString(item, "hostName"),
                    StartTimeUtc = startUtc,
                    DurationSeconds = Math.Max(durationSeconds ?? 0, 0),
                    RawJson = item.GetRawText()
                });

                addedThisPage++;
            }

            if (addedThisPage < pageSize)
            {
                break;
            }

            currentPage++;

            if (currentPage > 1000)
            {
                logger.LogWarning("Corte de seguridad en paginación de hotspot clients (page > 1000).");
                break;
            }
        }

        return sessions;
    }

    private async Task<string> ResolveHotspotSiteIdAsync(HttpClient client, string configuredSite, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(configuredSite))
        {
            return configuredSite;
        }

        var siteCandidate = configuredSite.Trim();

        // Si ya parece un id técnico, usamos directo.
        if (siteCandidate.Any(char.IsDigit) && siteCandidate.Any(ch => ch is '-' or '_'))
        {
            return siteCandidate;
        }

        var paths = new[]
        {
            $"/{_settings.ControllerId}/api/v2/sites?currentPage=1&currentPageSize=1000",
            $"/openapi/v2/{_settings.ControllerId}/sites?currentPage=1&currentPageSize=1000"
        };

        foreach (var path in paths)
        {
            try
            {
                var response = await client.GetAsync(path, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    continue;
                }

                var raw = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;

                if (!TryGetArray(root, out var items))
                {
                    continue;
                }

                foreach (var item in items.EnumerateArray())
                {
                    var name = GetString(item, "name") ?? GetString(item, "siteName");
                    if (!string.Equals(name, siteCandidate, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var id = GetString(item, "siteId") ?? GetString(item, "id") ?? GetString(item, "key");
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        logger.LogInformation("Omada hotspot: sitio '{ConfiguredSite}' resuelto a siteId '{ResolvedSiteId}'.", siteCandidate, id);
                        return id;
                    }
                }
            }
            catch
            {
                // Si un endpoint no está disponible, probamos el siguiente.
            }
        }

        logger.LogWarning("Omada hotspot: no se pudo resolver siteId para '{ConfiguredSite}', se usará el valor configurado.", siteCandidate);
        return siteCandidate;
    }

    private static bool TryGetArray(JsonElement root, out JsonElement items)
    {
        items = default;

        if (!root.TryGetProperty("result", out var result) || result.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (result.TryGetProperty("data", out items) && items.ValueKind == JsonValueKind.Array)
        {
            return true;
        }

        if (result.TryGetProperty("rows", out items) && items.ValueKind == JsonValueKind.Array)
        {
            return true;
        }

        return false;
    }

    private static string? GetString(JsonElement obj, string prop)
    {
        if (!obj.TryGetProperty(prop, out var val)) return null;
        return val.ValueKind switch
        {
            JsonValueKind.String => val.GetString(),
            JsonValueKind.Number => val.GetRawText(),
            _ => null
        };
    }

    private static long? GetLong(JsonElement obj, string prop)
    {
        if (!obj.TryGetProperty(prop, out var val)) return null;

        if (val.ValueKind == JsonValueKind.Number && val.TryGetInt64(out var n))
        {
            return n;
        }

        if (val.ValueKind == JsonValueKind.String && long.TryParse(val.GetString(), out var s))
        {
            return s;
        }

        return null;
    }

    private static int? GetInt(JsonElement obj, string prop)
    {
        if (!obj.TryGetProperty(prop, out var val)) return null;

        if (val.ValueKind == JsonValueKind.Number && val.TryGetInt32(out var n))
        {
            return n;
        }

        if (val.ValueKind == JsonValueKind.String && int.TryParse(val.GetString(), out var s))
        {
            return s;
        }

        return null;
    }

    private static int? ParseDurationToSeconds(string? durationText)
    {
        if (string.IsNullOrWhiteSpace(durationText)) return null;

        var parts = durationText.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var total = 0;

        foreach (var part in parts)
        {
            if (part.EndsWith("h", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(part[..^1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var h))
            {
                total += h * 3600;
                continue;
            }

            if (part.EndsWith("m", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(part[..^1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var m))
            {
                total += m * 60;
                continue;
            }

            if (part.EndsWith("s", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(part[..^1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var s))
            {
                total += s;
            }
        }

        return total > 0 ? total : null;
    }
}