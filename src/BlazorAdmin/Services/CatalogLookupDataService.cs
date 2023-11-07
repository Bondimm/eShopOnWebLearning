using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;
using BlazorShared;
using BlazorShared.Attributes;
using BlazorShared.Interfaces;
using BlazorShared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlazorAdmin.Services;

public class CatalogLookupDataService<TLookupData, TReponse>
    : ICatalogLookupDataService<TLookupData>
    where TLookupData : LookupData
    where TReponse : ILookupDataResponse<TLookupData>
{

    private readonly HttpClient _httpClient;
    private readonly ILogger<CatalogLookupDataService<TLookupData, TReponse>> _logger;
    private readonly IApiService _apiService;

    public CatalogLookupDataService(HttpClient httpClient,
        IApiService apiService,
        ILogger<CatalogLookupDataService<TLookupData, TReponse>> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiService = apiService;
    }

    public async Task<List<TLookupData>> List()
    {
        var endpointName = typeof(TLookupData).GetCustomAttribute<EndpointAttribute>().Name;
        _logger.LogInformation($"Fetching {typeof(TLookupData).Name} from API. Enpoint : {endpointName}");

        var response = await _httpClient.GetFromJsonAsync<TReponse>($"{await _apiService.GetApiUrlAsync()}{endpointName}");
        return response.List;
    }
}
