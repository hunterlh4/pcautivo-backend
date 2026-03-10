namespace PCautivoCore.Infrastructure.Queue;

public class AzureQueueSettings
{
    public AzureQueues Queues { get; set; } = new();
}

public class AzureQueues
{
    public AzureQueue WhatsApp { get; set; } = new();
    public AzureQueue Correo { get; set; } = new();
}

public class AzureQueue
{
    public string Name { get; set; } = string.Empty;
}
