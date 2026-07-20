using Application.DTOs;

namespace Application.Interfaces;

/// <summary>
/// Συμβόλαιο του use-case για το endpoint του report (Task 3).
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Επιστρέφει πλήθος διευθύνσεων ανά χώρα και τη χρονοσήμανση τελευταίας ενημέρωσης.
    /// Δώσε ένα σύνολο διψήφιων κωδικών για φιλτράρισμα, ή null για όλες τις χώρες.
    /// </summary>
    Task<IReadOnlyList<CountryReportItem>> GetReportAsync(
        string[]? twoLetterCodes,
        CancellationToken cancellationToken = default);
}
