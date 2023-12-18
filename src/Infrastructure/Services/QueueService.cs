using Azure.Messaging.ServiceBus;
using System.Threading.Tasks;
using BlazorShared;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

namespace Microsoft.eShopWeb.Infrastructure.Services;

public class QueueService : IQueueService
{
    private readonly QueueConfiguration _baseQueueConfiguration;

    public QueueService(
        IOptions<QueueConfiguration> baseQueueConfiguration)
    {
        _baseQueueConfiguration = baseQueueConfiguration.Value;
    }

    public async Task ReserveOrderItems(IEnumerable<OrderItem> orderItems)
    {
        var orderInfos = orderItems.Select(o => new
        {
            ItemId = o.ItemOrdered.CatalogItemId,
            Quantity = o.Units
        });

        var content = ToJson(orderInfos);

        await using var client = new ServiceBusClient(_baseQueueConfiguration.ConnectionString);

        await using ServiceBusSender sender = client.CreateSender(_baseQueueConfiguration.ReservedItemsQueue);

        try
        {
            var message = new ServiceBusMessage(content)
            {
                ContentType = "application/json",
            };

            await sender.SendMessageAsync(message);
        }
        finally
        {
            await sender.DisposeAsync();
            await client.DisposeAsync();
        }
    }

    private static string ToJson(object dataToSend)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        return JsonSerializer.Serialize(dataToSend, options);
    }
}
