using PCautivoCore.Domain.Enums;

namespace PCautivoCore.Domain.Models;

public class InventoryKardex
{
    public int Id { get; set; }
    public int InventoryId { get; set; }  // FK a Inventory
    
    public OperationType Operation { get; set; }  // 0=Purchase, 1=Sale, 2=Transfer, 3=Consumption, etc.
    public TransactionType TransactionType { get; set; }  // 0=In, 1=Out
    
    public decimal? BaseSize { get; set; }  // Cantidad del movimiento en unidad base
    public decimal? PresentationSize { get; set; }  // Cantidad en presentación
    
    // Para transferencias:
    // Si TransactionType = Out: RelatedStoreId = Store destino (a dónde va)
    // Si TransactionType = In: RelatedStoreId = Store origen (de dónde viene)
    public int? RelatedStoreId { get; set; }
    
    // Para consumos de mantenimiento - referencia al MaintenanceItem específico
    // Esto permite saber exactamente en qué habitación/área se usó el producto
    public int? RelatedMaintenanceItemId { get; set; }
    
    public string? Note { get; set; }  // Notas de la transacción
    
    public DateTimeOffset CreatedAt { get; set; }
    
    // Navigation properties
    public Inventory? Inventory { get; set; }
    public Store? RelatedStore { get; set; }
    public MaintenanceItem? RelatedMaintenanceItem { get; set; }
}
