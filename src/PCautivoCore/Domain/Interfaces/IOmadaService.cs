namespace PCautivoCore.Domain.Interfaces;

/// <summary>
/// Contrato para la comunicación con el controlador Omada OC-300.
/// </summary>
public interface IOmadaService
{
    /// <summary>
    /// Autoriza un cliente en el controlador Omada (portal cautivo externo).
    /// </summary>
    /// <param name="clientMac">Dirección MAC del cliente a autorizar (ej: aa-bb-cc-dd-ee-ff).</param>
    /// <param name="apMac">Dirección MAC del access point al que está conectado.</param>
    /// <param name="ssidName">Nombre del SSID.</param>
    /// <param name="radioId">ID de radio del AP (0=2.4GHz, 1=5GHz).</param>
    /// <param name="site">Nombre del sitio en el controlador Omada.</param>
    /// <returns>True si la autorización fue exitosa.</returns>
    Task<bool> AuthorizeClientAsync(
        string target,//
        int targetPort,//
        string clientMac, //
        string apMac,//
        string ssidName,//
        int radioId = 0,//
        string? vid = null,
        string scheme = "http",
        string? site = null,//
        long? t = null,//
        string? redirectUrl = null,//
        string? clientIp = null);//

    /// <summary>
    /// Autentica al usuario directamente en el portal cautivo de Omada (authType=5, local user).
    /// No requiere login de admin. Devuelve la URL de landing o null si falló.
    /// </summary>
    Task<string?> PortalAuthAsync(
        string username,
        string password,
        string clientMac,
        string apMac,
        string ssidName,
        int radioId = 0,
        string? originUrl = null);
}
