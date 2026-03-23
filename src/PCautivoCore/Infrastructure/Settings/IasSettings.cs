namespace PCautivoCore.Infrastructure.Settings;

/// <summary>
/// Configuración de conexión a la API del IAS (Identity and Access Service).
/// </summary>
public class IasSettings
{
    /// <summary>URL base del servicio IAS. Ej: http://40.122.134.6/ias/</summary>
    public string Connection { get; set; } = string.Empty;
}
