using System;
using System.Threading.Tasks;

namespace BlazorShared.Interfaces;

public interface IApiService
{
    Task<string> GetApiUrlAsync();
}
