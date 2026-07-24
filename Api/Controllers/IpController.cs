using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IpController : ControllerBase
{
    private readonly IIpInfoService _ipInfoService;

    public IpController(IIpInfoService ipInfoService)
    {
        _ipInfoService = ipInfoService;
    }

    [HttpGet("{address}")]
    [ProducesResponseType(typeof(IpInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IpInfoResponse>> GetIpInfo(
        string address,
        CancellationToken cancellationToken)
    {
        var info = await _ipInfoService.GetIpInfoAsync(address, cancellationToken);
        return info is null ? NotFound() : Ok(info);
    }
}
