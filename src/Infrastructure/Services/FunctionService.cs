using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorShared;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.Extensions.Options;

namespace Microsoft.eShopWeb.Infrastructure.Services;

public class FunctionService : IFunctionService
{
    private readonly FunctionUrlConfiguration _baseFunctionUrlConfiguration;
    private readonly HttpClient _httpClient;

    public FunctionService(
        HttpClient httpClient,
        IOptions<FunctionUrlConfiguration> baseFunctionUrlConfiguration)
    {
        _baseFunctionUrlConfiguration = baseFunctionUrlConfiguration.Value;
        _httpClient = httpClient;
    }

    public async Task<bool> ReserveOrderItems(IEnumerable<OrderItem> orderItems)
    {
        var orderInfos = orderItems.Select(o => new 
        { 
            ItemId = o.ItemOrdered.CatalogItemId,
            Quantity = o.Units
        });

        var content = ToJson(orderInfos);

        var result = await _httpClient.PostAsync(
            $"{_baseFunctionUrlConfiguration.BaseUrl}{_baseFunctionUrlConfiguration.OrderItemsReserverFunction}?code={_baseFunctionUrlConfiguration.FunctionKey}", content);

        return result.IsSuccessStatusCode;
    }

    private static StringContent ToJson(object dataToSend)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        return new StringContent(JsonSerializer.Serialize(dataToSend, options), Encoding.UTF8, "application/json");
    }
}
