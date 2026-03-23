namespace PCautivoCore.Infrastructure.Settings;

public class OmadaSyncJobSettings
{
    public bool Enabled { get; set; } = false;
    /// <summary>Inicio del job en formato HH:mm o cron Quartz (por ejemplo: 16:27 o 0 27 16 * * ?).</summary>
    public string StartJob { get; set; } = "07:00";
    /// <summary>Hora de corte del rango en formato HH:mm (por ejemplo: 16:27).</summary>
    public string RangeTime { get; set; } = "08:00";
    public string TimeZoneId { get; set; } = "SA Pacific Standard Time";
}
