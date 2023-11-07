using System.Net.Http;
using System.Threading.Tasks;
using BlazorShared.Interfaces;

namespace BlazorAdmin.Services;

public class ApiService : IApiService
{
    private string _apiUrl;
    private string _baseUrl;

    public ApiService(string baseUrl)
    {
        _baseUrl = baseUrl;
    }

    public async Task<string> GetApiUrlAsync()
    {
        if (_apiUrl == null)
        {
            var httpClient = new HttpClient();
            var request = await httpClient.GetAsync($"{_baseUrl}Servicing/Api-Url");
            _apiUrl = await request.Content.ReadAsStringAsync();
        }
        
        return _apiUrl;
    }
}
