using PCautivoCore.Domain.Enums;

namespace PCautivoCore.Domain.Models;

public class PropertyItem
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public int RoomId { get; set; }
    public int? ProductId { get; set; }
    public int? ProductGroupId { get; set; }
    public int? InventoryId { get; set; }
    public ProductGroupStatus GroupStatus { get; set; }  // 0=Unassigned, 1=Parent, 2=Child
    public int StoreId { get; set; } // Almacén del cual se asigna el producto
    
    public bool IsPriority { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Property? Property { get; set; }
    public Room? Room { get; set; }
    public Product? Product { get; set; }
    public Store? Store { get; set; }
    public Inventory? Inventory { get; set; }
}
