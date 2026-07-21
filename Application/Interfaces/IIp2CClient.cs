using Application.DTOs;

namespace Application.Interfaces;

/// <summary>
/// Αφαίρεση πάνω από το εξωτερικό web service IP2C. Κρατά τη λογική της εφαρμογής
/// ανεξάρτητη από οτιδήποτε αφορά HTTP ή τον host ip2c.org, και επιτρέπει στα tests
/// να δώσουν ένα ψεύτικο (fake) client.
/// </summary>
public interface IIp2CClient
{
    /// <summary>
    /// Καλεί το IP2C για τη δοθείσα IP και επιστρέφει το parsed αποτέλεσμα
    /// (status + κωδικοί χώρας + όνομα).
    /// </summary>
    Task<Ip2CResult> GetCountryInfoAsync(string ip, CancellationToken cancellationToken = default);
}
