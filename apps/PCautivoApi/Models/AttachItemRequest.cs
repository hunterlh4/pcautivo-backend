namespace PCautivoApi.Models;

public record AttachItemRequest
{
    public int ItemId { get; set; }
    public IFormFile? File { get; set; }
}
