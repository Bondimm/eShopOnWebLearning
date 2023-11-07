using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BlazorShared;
using Microsoft.Extensions.Options;

namespace Microsoft.eShopWeb.Web.Controllers;

[Route("[controller]")]
[ApiController]
public class ServicingController : ControllerBase
{
    private readonly BaseUrlConfiguration _baseUrlConfiguration;

    public ServicingController(IOptions<BaseUrlConfiguration> baseUrlConfiguration)
    {
        _baseUrlConfiguration = baseUrlConfiguration.Value;
    }

    [Route("Api-Url")]
    [HttpGet]
    [Authorize]
    [AllowAnonymous]
    public IActionResult GetApiUrl() =>
        Ok(_baseUrlConfiguration.ApiBase);
}
