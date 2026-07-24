using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

// Το endpoint του Task 3. Σκόπιμα "λεπτό": κάνει bind το query string, το δίνει στο
// service και επιστρέφει το αποτέλεσμα. Όλες οι ουσιαστικές αποφάσεις ζουν στο ReportService.
[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// Διευθύνσεις ανά χώρα, μαζί με την τελευταία φορά που ενημερώθηκε κάποια από αυτές.
    /// <param name="countryCodes">
    /// Προαιρετικοί διψήφιοι κωδικοί χωρών για φιλτράρισμα, π.χ.
    /// <c>?countryCodes=GR&amp;countryCodes=IT</c>. Παράλειψέ το για να πάρεις όλες τις χώρες.
    /// </param>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CountryReportItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CountryReportItem>>> GetReport(
        [FromQuery] string[]? countryCodes,
        CancellationToken cancellationToken)
    {
        // Προσοχή: όταν λείπει το query string, το model binding μας δίνει κενό array και
        // όχι null. Το ReportService χειρίζεται και τα δύο με τον ίδιο τρόπο ("όλες οι
        // χώρες"), οπότε δεν χρειάζεται ειδική μεταχείριση εδώ.
        var report = await _reportService.GetReportAsync(countryCodes, cancellationToken);
        return Ok(report);
    }
}
