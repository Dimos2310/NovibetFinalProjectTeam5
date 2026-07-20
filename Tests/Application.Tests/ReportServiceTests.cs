using Application.DTOs;
using Application.Services;
using Xunit;

namespace Application.Tests;

// Το ReportService έχει δύο ευθύνες που αξίζει να ελεγχθούν: τι στέλνει ΠΡΟΣ ΤΑ ΚΑΤΩ στο
// repository (το κανονικοποιημένο φίλτρο) και τι επιστρέφει ΠΡΟΣ ΤΑ ΠΑΝΩ στον caller
// (τη σειρά). Το ίδιο το grouping/counting είναι SQL και καλύπτεται στο Infrastructure.Tests.
public class ReportServiceTests
{
    private static readonly DateTime Now = new(2022, 10, 12, 8, 41, 37, DateTimeKind.Utc);

    // ---------- τι φτάνει στο repository ----------

    [Fact]
    public async Task Null_codes_are_passed_down_as_null_meaning_all_countries()
    {
        var repo = new FakeIpRepository();
        var sut = new ReportService(repo);

        await sut.GetReportAsync(null);

        Assert.True(repo.WasCalled);
        Assert.Null(repo.ReceivedCodes);
    }

    [Fact]
    public async Task Empty_array_is_passed_down_as_null_meaning_all_countries()
    {
        // Αυτό είναι το σημαντικό: το model binding του ASP.NET μετατρέπει ένα query string
        // που λείπει σε κενό array, όχι σε null. Αν το service προωθούσε αυτό το κενό array
        // ως έχει, το σκέτο "GET /api/report" χωρίς φίλτρο θα μπορούσε να επιστρέψει κενό
        // αντί να απαριθμήσει όλες τις χώρες.
        var repo = new FakeIpRepository();
        var sut = new ReportService(repo);

        await sut.GetReportAsync([]);

        Assert.Null(repo.ReceivedCodes);
    }

    [Fact]
    public async Task Codes_are_uppercased_and_trimmed_before_hitting_the_database()
    {
        // Οι κωδικοί αποθηκεύονται όπως τους στέλνει το IP2C (κεφαλαία). Ένας caller που
        // πληκτρολογεί "gr" πρέπει και πάλι να πάρει την Ελλάδα - και αυτό δεν πρέπει να
        // εξαρτάται από το collation της βάσης.
        var repo = new FakeIpRepository();
        var sut = new ReportService(repo);

        await sut.GetReportAsync([" gr ", "it"]);

        Assert.NotNull(repo.ReceivedCodes);
        Assert.Equal(["GR", "IT"], repo.ReceivedCodes);
    }

    [Fact]
    public async Task Duplicate_codes_are_collapsed()
    {
        var repo = new FakeIpRepository();
        var sut = new ReportService(repo);

        await sut.GetReportAsync(["GR", "gr", " GR"]);

        Assert.NotNull(repo.ReceivedCodes);
        Assert.Equal(["GR"], repo.ReceivedCodes);
    }

    [Fact]
    public async Task Blank_entries_are_dropped_and_an_all_blank_filter_becomes_null()
    {
        var repo = new FakeIpRepository();
        var sut = new ReportService(repo);

        await sut.GetReportAsync(["", "   "]);

        // Δεν ζητήθηκε τίποτα αξιοποιήσιμο, οπότε επιστρέφουμε στο "όλες οι χώρες" αντί να
        // φιλτράρουμε πάνω σε κωδικό που δεν μπορεί ποτέ να ταιριάξει.
        Assert.Null(repo.ReceivedCodes);
    }

    [Fact]
    public async Task Blank_entries_are_dropped_but_real_codes_survive()
    {
        var repo = new FakeIpRepository();
        var sut = new ReportService(repo);

        await sut.GetReportAsync(["", "gr", "  "]);

        Assert.NotNull(repo.ReceivedCodes);
        Assert.Equal(["GR"], repo.ReceivedCodes);
    }

    // ---------- τι επιστρέφεται στον caller ----------

    [Fact]
    public async Task Rows_come_back_ordered_by_address_count_descending()
    {
        var repo = new FakeIpRepository(
            new CountryReportItem("Italy", 3, Now),
            new CountryReportItem("Greece", 4, Now),
            new CountryReportItem("Japan", 1, Now));

        var sut = new ReportService(repo);

        var result = await sut.GetReportAsync(null);

        Assert.Equal(["Greece", "Italy", "Japan"], result.Select(r => r.CountryName));
    }

    [Fact]
    public async Task Countries_with_the_same_count_are_ordered_alphabetically()
    {
        // Οι ισοβαθμίες πρέπει να είναι ντετερμινιστικές, αλλιώς το endpoint επιστρέφει τις
        // γραμμές με όποια σειρά έτυχε να τις παραγάγει η βάση.
        var repo = new FakeIpRepository(
            new CountryReportItem("Spain", 3, Now),
            new CountryReportItem("China", 3, Now),
            new CountryReportItem("Italy", 3, Now));

        var sut = new ReportService(repo);

        var result = await sut.GetReportAsync(null);

        Assert.Equal(["China", "Italy", "Spain"], result.Select(r => r.CountryName));
    }

    [Fact]
    public async Task Counts_and_timestamps_are_passed_through_untouched()
    {
        var lastUpdated = new DateTime(2022, 10, 12, 7, 4, 51, DateTimeKind.Utc);
        var repo = new FakeIpRepository(new CountryReportItem("Greece", 4, lastUpdated));

        var sut = new ReportService(repo);

        var result = await sut.GetReportAsync(["GR"]);

        var row = Assert.Single(result);
        Assert.Equal("Greece", row.CountryName);
        Assert.Equal(4, row.AddressesCount);
        Assert.Equal(lastUpdated, row.LastAddressUpdated);
    }

    [Fact]
    public async Task An_empty_database_produces_an_empty_report_not_an_error()
    {
        var repo = new FakeIpRepository();
        var sut = new ReportService(repo);

        var result = await sut.GetReportAsync(null);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Filtering_on_a_country_we_have_nothing_for_returns_an_empty_report()
    {
        // Το repository δεν βρίσκει γραμμές που να ταιριάζουν - το service πρέπει να το
        // επιστρέψει ως κενή λίστα και όχι να επινοήσει γραμμή με μηδενικό count.
        var repo = new FakeIpRepository();
        var sut = new ReportService(repo);

        var result = await sut.GetReportAsync(["ZZ"]);

        Assert.NotNull(repo.ReceivedCodes);
        Assert.Equal(["ZZ"], repo.ReceivedCodes);
        Assert.Empty(result);
    }
}
