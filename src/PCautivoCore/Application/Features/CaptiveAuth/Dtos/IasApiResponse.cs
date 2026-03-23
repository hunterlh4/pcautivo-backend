namespace PCautivoCore.Application.Features.CaptiveAuth.Dtos;

public class IasApiResponse
{
    public string? Mensaje { get; set; }
    public bool Respuesta { get; set; }
    public List<IasUserData>? Data { get; set; }
}

public class IasUserData
{
    public int Id { get; set; }
    public string? NroDoc { get; set; }
}
