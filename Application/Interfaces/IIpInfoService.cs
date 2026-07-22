using Application.DTOs;

namespace Application.Interfaces;

/// <summary>
/// Συμβόλαιο του use-case για το endpoint πληροφοριών IP (Task 1). Η υλοποίηση εκτελεί
/// την αλυσίδα cache → βάση → IP2C και γεμίζει αναδρομικά και τους δύο αποθηκευτικούς χώρους.
/// </summary>
public interface IIpInfoService
{
    /// <summary>
    /// Επιστρέφει τα στοιχεία χώρας για τη δοθείσα IP, ή null αν η IP είναι άγνωστη/άκυρη.
    /// </summary>
    Task<IpInfoResponse?> GetIpInfoAsync(string ip, CancellationToken cancellationToken = default);
}
