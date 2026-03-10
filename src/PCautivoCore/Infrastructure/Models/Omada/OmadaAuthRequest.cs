using System.Text.Json.Serialization;

namespace PCautivoCore.Infrastructure.Models.Omada;

internal record OmadaAuthRequest
{
    [JsonPropertyName("t")]           public long? T { get; init; }         // T del redirect URL como número
    [JsonPropertyName("clientMac")]   public required string ClientMac { get; init; }
    [JsonPropertyName("apMac")]       public required string ApMac { get; init; }
    [JsonPropertyName("ssidName")]    public required string SsidName { get; init; }
    [JsonPropertyName("radioId")]     public int RadioId { get; init; }
    [JsonPropertyName("time")]        public int Time { get; init; } = 28800;
    [JsonPropertyName("clientIp")]    public string? ClientIp { get; init; }
    [JsonPropertyName("redirectUrl")] public string? RedirectUrl { get; init; }
}

internal record OmadaBaseResponse
{
    [JsonPropertyName("errorCode")] public int ErrorCode { get; init; }
    [JsonPropertyName("msg")]       public string? Message { get; init; }
}

internal record OmadaLoginResponse
{
    [JsonPropertyName("errorCode")] public int ErrorCode { get; init; }
    [JsonPropertyName("result")]    public OmadaLoginResult? Result { get; init; }
}

internal record OmadaLoginResult
{
    [JsonPropertyName("token")]    public string? Token { get; init; }
    [JsonPropertyName("omadacId")] public string? OmadacId { get; init; }
}

// ── Portal auth (POST /portal/auth) ────────────────────────────────────────
// authType: 2 = Local User | 5 = External RADIUS (verificar en panel Omada)
internal record OmadaPortalAuthRequest
{
    [JsonPropertyName("authType")]    public int AuthType { get; init; } = 2;
    [JsonPropertyName("localuser")]   public required string LocalUser { get; init; }
    [JsonPropertyName("localuserPsw")] public required string LocalUserPsw { get; init; }
    [JsonPropertyName("clientMac")]   public required string ClientMac { get; init; }
    [JsonPropertyName("apMac")]       public required string ApMac { get; init; }
    [JsonPropertyName("ssidName")]    public required string SsidName { get; init; }
    [JsonPropertyName("radioId")]     public int RadioId { get; init; }
    [JsonPropertyName("originUrl")]   public string OriginUrl { get; init; } = string.Empty;
}

internal record OmadaPortalAuthResponse
{
    [JsonPropertyName("errorCode")] public int ErrorCode { get; init; }
    /// <summary>URL de landing devuelta por Omada tras autorizar al cliente.</summary>
    [JsonPropertyName("result")]    public string? Result { get; init; }
}

// ── Clients list (GET /openapi/v2/{controllerId}/sites/{siteId}/clients) ────
internal record OmadaClientsResponse
{
    [JsonPropertyName("errorCode")] public int ErrorCode { get; init; }
    [JsonPropertyName("result")]    public OmadaClientsResult? Result { get; init; }
}

internal record OmadaClientsResult
{
    [JsonPropertyName("data")] public List<OmadaClientInfo>? Data { get; init; }
}

internal record OmadaClientInfo
{
    [JsonPropertyName("mac")]        public string? Mac { get; init; }
    /// <summary>0 = pendiente, 1 = autorizado, 2 = autorizado (con sesión activa).</summary>
    [JsonPropertyName("authStatus")] public int AuthStatus { get; init; }
    [JsonPropertyName("active")]     public bool Active { get; init; }
}
