using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CountryReportItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CountryReportItem>>> GetReport(
        [FromQuery] string[]? countryCodes,
        CancellationToken cancellationToken)
    {
        var report = await _reportService.GetReportAsync(countryCodes, cancellationToken);
        return Ok(report);
    }
}
