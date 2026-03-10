namespace PCautivoCore.Domain.Dtos.Queue;

public class WhatsAppQueueDto
{
    public int IdSistema { get; set; }
    public WhatsAppMessageDto Mensaje { get; set; } = new();
}

public class WhatsAppMessageDto
{
    public string CodigoPais { get; set; } = string.Empty;
    public string NumeroCelular { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
}
