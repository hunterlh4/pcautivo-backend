using Azure.Storage;
using Azure.Storage.Queues;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace PCautivoCore.Infrastructure.Queue;

public class AzureQueueClient
{
    private readonly AzureQueueSettings _settings;
    private readonly AzureQueueCredentials _credentials;
    private readonly Dictionary<string, QueueClient> _queues;

    public AzureQueueClient(IOptions<AzureQueueSettings> settings, IOptions<AzureQueueCredentials> credentials)
    {
        _settings = settings.Value;
        _credentials = credentials.Value;
        _queues = new Dictionary<string, QueueClient>();
        
        StorageSharedKeyCredential credential = new(_credentials.AccountName, _credentials.ApiKey);

        // Queue WhatsApp
        string whatsappQueueName = _settings.Queues.WhatsApp.Name;
        QueueClient whatsappQueue = new(
            new Uri($"{_credentials.UrlBase}/{whatsappQueueName}"),
            credential,
            new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            }
        );
        whatsappQueue.CreateIfNotExists();
        _queues.TryAdd(whatsappQueueName, whatsappQueue);

        // Queue Correo
        string correoQueueName = _settings.Queues.Correo.Name;
        QueueClient correoQueue = new(
            new Uri($"{_credentials.UrlBase}/{correoQueueName}"),
            credential,
            new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            }
        );
        correoQueue.CreateIfNotExists();
        _queues.TryAdd(correoQueueName, correoQueue);
    }

    public async Task SendMessageAsync(string queueName, object message)
    {
        if (_queues.TryGetValue(queueName, out QueueClient? queueClient))
        {
            string messageJson = JsonSerializer.Serialize(message);
            await queueClient.SendMessageAsync(messageJson);
        }
        else
        {
            throw new ArgumentException($"Queue '{queueName}' not found.");
        }
    }
}
