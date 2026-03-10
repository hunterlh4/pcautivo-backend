using PCautivoCore.Domain.Enums;

namespace PCautivoCore.Domain.Models;

public class InventoryConsumptionDetail
{
    public int Id { get; set; }
    
    // Referencias
    public int InventoryId { get; set; }
    public int? KardexOutId { get; set; }
    public int MaintenanceItemId { get; set; }
    public int MaintenanceId { get; set; }
    public int UserId { get; set; }
    
    // Cantidades consumidas
    public decimal BaseSize { get; set; }
    public decimal? PresentationSize { get; set; }
    
    // Unidades de medida
    public MeasurementUnit? UnitBase { get; set; }
    public MeasurementUnit? UnitPresentation { get; set; }
    
    // Estado de consolidación
    public bool IsConsolidated { get; set; }
    
    // Información adicional
    public string? Note { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    
    // Navegación
    public Inventory? Inventory { get; set; }
    public InventoryKardex? KardexOut { get; set; }
    public MaintenanceItem? MaintenanceItem { get; set; }
    public Maintenance? Maintenance { get; set; }
    public User? User { get; set; }
}
