namespace PCautivoCore.Domain.Models;

public class ProductGroupRelation
{
    public int ParentId { get; set; }
    public int ChildId { get; set; }
    
    // Navigation properties
    public Product? Parent { get; set; }
    public Product? Child { get; set; }
}
