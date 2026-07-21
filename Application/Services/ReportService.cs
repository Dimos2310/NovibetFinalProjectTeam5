using Application.Abstractions;
using Application.DTOs;
using Application.Interfaces;

namespace Application.Services;

// Task 3: "πόσες διευθύνσεις ανά χώρα, και πότε ενημερώθηκε τελευταία φορά κάποια από αυτές".
// Το ίδιο το grouping γίνεται σε SQL μέσα στο IIpRepository.GetReportAsync - αυτό το service
// αναλαμβάνει τα δύο πράγματα που δεν ανήκουν στο repository: τον καθαρισμό όσων έστειλε
// ο caller, και την απόφαση για τη σειρά με την οποία επιστρέφονται οι γραμμές.
public class ReportService : IReportService
{
    private readonly IIpRepository _ipRepository;

    public ReportService(IIpRepository ipRepository)
    {
        _ipRepository = ipRepository;
    }

    public async Task<IReadOnlyList<CountryReportItem>> GetReportAsync(
        string[]? twoLetterCodes,
        CancellationToken cancellationToken = default)
    {
        var filter = NormalizeCodes(twoLetterCodes);

        var rows = await _ipRepository.GetReportAsync(filter, cancellationToken);

        // Πρώτα οι χώρες με τις περισσότερες διευθύνσεις, μετά αλφαβητικά ώστε οι ισοβαθμίες
        // να μην επιστρέφονται σε τυχαία σειρά. Χωρίς αυτό, η σειρά είναι ό,τι τύχει να
        // βγάλει η βάση - πράγμα που κάνει το endpoint δύσκολο στο μάτι και αδύνατο να
        // ελεγχθεί σε test. Η ταξινόμηση γίνεται εδώ (όχι σε SQL) και είναι φθηνή:
        // μιλάμε για μία γραμμή ανά χώρα, όχι ανά IP.
        return rows
            .OrderByDescending(r => r.AddressesCount)
            .ThenBy(r => r.CountryName, StringComparer.Ordinal)
            .ToList();
    }

    // Το IP2C μας δίνει τους κωδικούς με κεφαλαία ("GR"), άρα έτσι είναι αποθηκευμένοι στη
    // βάση. Ένας caller που στέλνει "gr" ή " gr " πρέπει και πάλι να πάρει την Ελλάδα,
    // εξ ου και ο καθαρισμός. Επιπλέον έτσι το φιλτράρισμα συμπεριφέρεται το ίδιο σε
    // SQL Server (case-insensitive collation) και σε SQLite στα tests (case-sensitive) -
    // χωρίς αυτό, η ίδια ακριβώς κλήση θα επέστρεφε διαφορετικά αποτελέσματα ανά provider.
    private static string[]? NormalizeCodes(string[]? codes)
    {
        if (codes is null || codes.Length == 0)
        {
            return null; // null/κενό σημαίνει "όλες οι χώρες", σύμφωνα με την εκφώνηση
        }

        var cleaned = codes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim().ToUpperInvariant())
            .Distinct()
            .ToArray();

        // Δεν έμεινε τίποτα αξιοποιήσιμο (π.χ. ο caller έστειλε [""]) - το αντιμετωπίζουμε
        // σαν να μη ζητήθηκε καθόλου φίλτρο, αντί να επιστρέψουμε κενό report.
        return cleaned.Length == 0 ? null : cleaned;
    }
}
