namespace PCautivoCore.Infrastructure.Settings;

public class NotificationSettings
{
    public IncidentNotificationSettings Incidents { get; set; } = new();
    public MaintenanceNotificationSettings Maintenance { get; set; } = new();
    public StockNotificationSettings Stock { get; set; } = new();
}

public class IncidentNotificationSettings
{
    public List<string> ImplicatedEmails { get; set; } = new();
    public List<PhoneContact> ImplicatedPhones { get; set; } = new();
}

public class MaintenanceNotificationSettings
{
    public List<string> ImplicatedEmails { get; set; } = new();
    public List<PhoneContact> ImplicatedPhones { get; set; } = new();
}

public class StockNotificationSettings
{
    public List<string> ImplicatedEmails { get; set; } = new();
    public List<PhoneContact> ImplicatedPhones { get; set; } = new();
}

public class PhoneContact
{
    public string CountryCode { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
