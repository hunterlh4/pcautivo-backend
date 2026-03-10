using PCautivoCore.Domain.Enums;

namespace PCautivoCore.Domain.Models;

/// <summary>
/// InspectionItem V2 - Usa InspectionRoom
/// Incluye snapshot histórico del producto (solo presentación)
/// </summary>
public class InspectionItemV2
{
    public int Id { get; set; }
    public int InspectionId { get; set; }
    public int RoomId { get; set; }
    public int? ParentItemId { get; set; }  // ID del InspectionItem padre (NULL = item raíz)
    public string? Name { get; set; } // Nombre del producto
    public string? SerialNumber { get; set; }
    
    // Snapshot histórico - Unidad de Presentación
    public MeasurementUnit? PresentationUnit { get; set; }
    public decimal? PresentationSize { get; set; }
    
    // Estado de la inspección
    public bool IsExists { get; set; }
    public InspectionItemCondition Condition { get; set; }
    public string? Comments { get; set; }
    
    // Auditoría
    public int CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    
    // Relaciones
    public Inspection? Inspection { get; set; }
    public InspectionRoom? Room { get; set; }
}
