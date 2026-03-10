using PCautivoCore.Domain.Enums;

namespace PCautivoCore.Domain.Models;

public class Inspection
{
    public int Id { get; set; }
    public InspectionType CheckType { get; set; }
    public int PropertyId { get; set; }
    public int? GuestId { get; set; }
    public DateTimeOffset CheckDate { get; set; }
    public InspectionState CurrentState { get; set; }
    public int CreatedBy { get; set; }
    public int UpdatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Property? Property { get; set; }
    public Guest? Guest { get; set; }
}

public class InspectionRoom
{
    public int Id { get; set; }
    public int InspectionId { get; set; }
    public string? Name { get; set; }
    public int CreatedBy { get; set; }
    public int UpdatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class InspectionItem
{
    public int Id { get; set; }
    public int InspectionId { get; set; }
    public int RoomId { get; set; }
    public int? ParentItemId { get; set; } // Referencia al padre dentro de InspectionItems
    
    // Snapshot histórico
    public string? Name { get; set; } // Nombre del producto
    public string? SerialNumber { get; set; } // Número de serie
    
    // Unidad de Presentación (cómo se compra) - Snapshot histórico
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
    
    // Navegación
    public Property? Property { get; set; }
    public Inspection? Inspection { get; set; }
    public InspectionRoom? Room { get; set; }
}

public enum InspectionType
{
    Inventory = 1,
    Inspection = 4
}

public enum InspectionState
{
    Pending = 1,
    Active = 2,
    Complete = 3,
    Closed = 4
}

public enum InspectionItemCondition
{
    Pending = 0,
    New = 1,
    Good = 2,
    Fair = 3,
    Poor = 4
}