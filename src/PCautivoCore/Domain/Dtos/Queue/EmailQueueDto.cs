namespace PCautivoCore.Domain.Dtos.Queue;

public class EmailQueueDto
{
    public int IdSistema { get; set; }
    public EmailQueueContentDto Mensaje { get; set; } = new();
}

public class EmailQueueContentDto
{
    public string Asunto { get; set; } = string.Empty;
    public string Cuerpo { get; set; } = string.Empty;
    public bool EsHtml { get; set; }
    public List<string> Destinatarios { get; set; } = new();
    public List<string> DestinatariosConCopia { get; set; } = new();
    public List<string> DestinatariosConCopiaOculta { get; set; } = new();
    public List<EmailQueueAttachmentDto> Adjuntos { get; set; } = new();
}

public class EmailQueueAttachmentDto
{
    public MemoryStream Archivo { get; set; } = new();
    public string Nombre { get; set; } = string.Empty;
}
