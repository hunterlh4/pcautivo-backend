namespace PCautivoCore.Infrastructure.Settings;

/// <summary>
/// Configuración de conexión al controlador Omada OC-300.
/// </summary>
public class OmadaSettings
{
    /// <summary>URL base del controlador Omada. Ej: https://192.168.1.1:8043</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL base para el login de admin (puerto 443, sin :8043).
    /// Si no se especifica se usa BaseUrl.
    /// Ej: https://192.168.0.2
    /// </summary>
    public string? LoginBaseUrl { get; set; }

    /// <summary>Omada Controller ID (visible en la URL del panel de administración).</summary>
    public string ControllerId { get; set; } = string.Empty;

    /// <summary>Nombre del sitio en el controlador Omada. Por defecto "Default".</summary>
    public string Site { get; set; } = "Default";

    /// <summary>Usuario administrador del controlador Omada.</summary>
    public string AdminUsername { get; set; } = string.Empty;

    /// <summary>Contraseña del administrador del controlador Omada.</summary>
    public string AdminPassword { get; set; } = string.Empty;

    /// <summary>Ignorar validación de certificado SSL (útil en desarrollo con cert autofirmado).</summary>
    public bool IgnoreSslErrors { get; set; } = false;
}
