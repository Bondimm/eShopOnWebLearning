using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Text.Json;
using EShopFunctions.Models;

namespace AzureCosmosFunction
{
    public static class DeliveryOrderProcessor
    {
        [FunctionName("DeliveryOrderProcessor")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "EShopInWeb",
                containerName: "Orders",
                Connection = "CosmosDBConnection",
                CreateIfNotExists = true)] IAsyncCollector<Order> orders,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger started processing order information at {DateTime.UtcNow}.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            try
            {
                var order = JsonSerializer.Deserialize<Order>(requestBody, options);

                await orders.AddAsync(order);

                log.LogInformation($"C# HTTP trigger finished processing order information at {DateTime.UtcNow}.");

                return new OkObjectResult($"Order added!");
            }
            catch
            {
                log.LogInformation($"Something went wrong with order addition.");
                return new BadRequestResult();
            }
        }
    }
}
