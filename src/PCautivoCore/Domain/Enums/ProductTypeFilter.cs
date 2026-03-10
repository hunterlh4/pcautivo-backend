namespace PCautivoCore.Domain.Enums;

/// <summary>
/// Filtro para consultar productos por tipo
/// Coincide con los valores de ProductType en la base de datos
/// Si es null, trae todos los tipos
/// </summary>
public enum ProductTypeFilter
{
    /// <summary>
    /// Solo muebles e inmuebles
    /// </summary>
    Furniture = 0,
    
    /// <summary>
    /// Solo productos de limpieza
    /// </summary>
    Cleaning = 1,
    
    /// <summary>
    /// Solo productos consumibles
    /// </summary>
    Consumable = 2
}
