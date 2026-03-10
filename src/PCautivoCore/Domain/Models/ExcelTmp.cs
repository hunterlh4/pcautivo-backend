namespace PCautivoCore.Domain.Models;

public class ExcelTmp
{
    public int Id { get; set; }
    public int PropiedadId { get; set; }
    public int? AmbienteId { get; set; }
    public string Ambiente { get; set; } = string.Empty;
    public int? ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public int StoreId { get; set; }
}
