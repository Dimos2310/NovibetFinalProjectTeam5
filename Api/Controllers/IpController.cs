using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

// Το endpoint του Task 1. "Λεπτό" όπως και το ReportController: παίρνει τη διεύθυνση από
// το route, τη δίνει στο IIpInfoService, και μεταφράζει το αποτέλεσμα σε HTTP απάντηση.
// Το InvalidIpAddressException (status 0 από το IP2C) ΔΕΝ πιάνεται εδώ επίτηδες - το
// τραβάει η ExceptionHandlingMiddleware πιο πάνω στο pipeline και το μετατρέπει σε 400.
[ApiController]
[Route("api/[controller]")]
public class IpController : ControllerBase
{
    private readonly IIpInfoService _ipInfoService;

    public IpController(IIpInfoService ipInfoService)
    {
        _ipInfoService = ipInfoService;
    }

    /// <summary>
    /// Επιστρέφει τα στοιχεία χώρας (CountryName, TwoLetterCode, ThreeLetterCode) για τη
    /// δοθείσα IP διεύθυνση. Ελέγχει πρώτα την cache, μετά τη βάση, και τέλος καλεί το IP2C.
    /// </summary>
    [HttpGet("{address}")]
    [ProducesResponseType(typeof(IpInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IpInfoResponse>> GetIpInfo(
        string address,
        CancellationToken cancellationToken)
    {
        var info = await _ipInfoService.GetIpInfoAsync(address, cancellationToken);

        // null σημαίνει "έγκυρη IP, αλλά το IP2C δεν ξέρει τη χώρα της" (status 2) -
        // αυτό είναι σωστά ένα 404, όχι σφάλμα του server.
        return info is null ? NotFound() : Ok(info);
    }
}
