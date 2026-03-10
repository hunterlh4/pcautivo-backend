namespace PCautivoCore.Domain.Models;

public class IncidentAttachment
{
    public int Id { get; set; }
    public IncidentAttachmentType ItemType { get; set; }
    public int ItemId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public int CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public enum IncidentAttachmentType
{
    Inmueble = 1,
    NoInmueble = 2
}
