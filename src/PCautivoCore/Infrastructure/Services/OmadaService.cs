using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Infrastructure.Models.Omada;
using PCautivoCore.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

                var siteId   = !string.IsNullOrWhiteSpace(site) ? site : _settings.Site;
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
                    var siteChk = !string.IsNullOrWhiteSpace(site) ? site : _settings.Site;
                    bool alreadyAuth = await IsClientAuthorizedAsync(NormMac(clientMac), siteChk, adminToken);
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
}