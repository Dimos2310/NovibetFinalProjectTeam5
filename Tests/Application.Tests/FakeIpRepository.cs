using Application.Abstractions;
using Application.DTOs;
using Domain.Entities;

namespace Application.Tests;

// Χειρόγραφο υποκατάστατο του IIpRepository, ώστε το ReportService να ελέγχεται χωρίς
// βάση δεδομένων. Δύο δουλειές: επιστρέφει όποιες γραμμές ορίσει το κάθε test, και
// καταγράφει το φίλτρο με το οποίο κλήθηκε, ώστε ένα test να μπορεί να επιβεβαιώσει
// τι ακριβώς πέρασε προς τα κάτω το service.
internal sealed class FakeIpRepository : IIpRepository
{
    private readonly IReadOnlyList<CountryReportItem> _rows;

    public FakeIpRepository(params CountryReportItem[] rows) => _rows = rows;

    // Τι παρέλαβε η GetReportAsync. Το null εδώ έχει νόημα (= "όλες οι χώρες"),
    // γι' αυτό το WasCalled παρακολουθεί χωριστά αν έγινε καν η κλήση.
    public string[]? ReceivedCodes { get; private set; }
    public bool WasCalled { get; private set; }

    public Task<IReadOnlyList<CountryReportItem>> GetReportAsync(
        string[]? countryCodes,
        CancellationToken ct = default)
    {
        ReceivedCodes = countryCodes;
        WasCalled = true;
        return Task.FromResult(_rows);
    }

    // Δεν χρησιμοποιούνται από το report - υπάρχουν μόνο για να ικανοποιηθεί το interface.
    public Task<Ip?> GetByAddressAsync(string address, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<Ip> AddAsync(Ip ip, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task SaveChangesAsync(CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<Ip>> GetBatchAsync(int skip, int take, CancellationToken ct = default)
        => throw new NotSupportedException();
}
