using PCautivoCore.Domain.Enums;

namespace PCautivoCore.Domain.Models;

public class PropertyItemMaintenance
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public int? RoomId { get; set; } // NULL = toda la propiedad
    public int? ProductId { get; set; }
    public int? ProductGroupId { get; set; }
    
    // Unidad de presentación (cómo se compra/presenta)
    public MeasurementUnit? PresentationUnit { get; set; }
    public decimal? PresentationSize { get; set; }
    
    // Unidad base de almacenamiento
    public MeasurementUnit? Unit { get; set; }
    public decimal? Size { get; set; } // Cantidad en unidad base
    
    public bool IsVariable { get; set; } // Indica si los valores son fijos o pueden cambiar
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public Property? Property { get; set; }
    public Room? Room { get; set; }
    public Product? Product { get; set; }
}
