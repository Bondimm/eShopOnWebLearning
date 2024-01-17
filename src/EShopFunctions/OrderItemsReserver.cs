using Polly;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using Microsoft.Azure.WebJobs.ServiceBus;
using Azure.Messaging.ServiceBus;
using EShopFunctions.Models;

namespace OrderItemsReserver
{
    public static class OrderItemsReserver
    {
        [FunctionName("OrderItemsReserver")]
        public static async Task Run(
            [ServiceBusTrigger(
                "reserveditems",
                Connection = "ServiceBusConnection",
                AutoCompleteMessages = false)]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions,
            ILogger log)
        {
            var maxRetries = 3;

            var retryPolicy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(retryCount: maxRetries, sleepDurationProvider: (attemptCount) => TimeSpan.FromSeconds(attemptCount * 15),
                onRetry: (exception, sleepDuration, attemptNumber, context) =>
                {
                    log.LogInformation($"Blob request error. Retrying in {sleepDuration}. {attemptNumber} / {maxRetries}");
                    messageActions.RenewMessageLockAsync(message);
                });

            var fallbackPolicy = Policy
                .Handle<Exception>()
                .FallbackAsync(fallbackAction: async (cancellationToken) =>
                {
                    await HandleError(messageActions, message);
                });

            var retryWithFallback = fallbackPolicy.WrapAsync(retryPolicy);

            log.LogInformation($"C# HTTP trigger started processing order information at {DateTime.UtcNow}.");

            await retryWithFallback.ExecuteAsync(async () =>
            {
                await HandleMessage(
                    messageActions,
                    message);
            });

            log.LogInformation($"C# HTTP trigger finished processing order information at {DateTime.UtcNow}.");
        }

        private static async Task HandleMessage(
            ServiceBusMessageActions messageActions,
            ServiceBusReceivedMessage message)
        {
            var body = Encoding.UTF8.GetString(message.Body);

            var blobServiceClient = new BlobServiceClient(Environment.GetEnvironmentVariable("EShopStorage"));

            var containerClient = blobServiceClient.GetBlobContainerClient("reserved-orders");

            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient($"order-{DateTime.UtcNow.ToFileTime()}.json");

            await blobClient.UploadAsync(BinaryData.FromString(body));

            await messageActions.CompleteMessageAsync(message);
        }

        private static async Task HandleError(
            ServiceBusMessageActions messageActions,
            ServiceBusReceivedMessage message)
        {
            try
            {
                await SendErrorEmail(message);
                await messageActions.CompleteMessageAsync(message);
            }
            catch
            {
                await messageActions.AbandonMessageAsync(message);
            }
        }

        private static async Task SendErrorEmail(ServiceBusReceivedMessage message)
        {
            var reservedItems = Encoding.UTF8.GetString(message.Body);
            var subject = "Order reservation failed! Manual order resolution must be done.";
            var body = CreateEmailBody(reservedItems);

            var client = new HttpClient();

            var jsonData = JsonSerializer.Serialize(new
            {
                subject,
                body
            });

            string logicAppUrl = Environment.GetEnvironmentVariable("LogicAppUrl");

            HttpResponseMessage result = await client.PostAsync(
                logicAppUrl,
                new StringContent(jsonData, Encoding.UTF8, "application/json"));

            if (!result.IsSuccessStatusCode)
            {
                throw new HttpRequestException();
            }
        }

        private static string CreateEmailBody(string reservedItems)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            var reservations = JsonSerializer.Deserialize<List<OrderReservation>>(reservedItems, options);

            var body = new StringBuilder();

            body.AppendLine("Order reservation save error. Please, resolve reserved itmes manually:");

            foreach (var reservation in reservations)
            {
                body.AppendLine($"ItemId: {reservation.ItemId}; Quantity: {reservation.Quantity}");
            }

            return body.ToString();
        }
    }
}
