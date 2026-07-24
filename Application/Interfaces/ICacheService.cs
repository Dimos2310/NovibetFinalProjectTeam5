namespace Application.Interfaces;

/// <summary>
/// Αφαίρεση πάνω από το επίπεδο caching, που χρησιμοποιείται από την αναζήτηση
/// "cache first" (Task 1) και ακυρώνεται από το job ενημέρωσης (Task 2). Κρύβει το αν
/// από πίσω υπάρχει in-memory cache ή κατανεμημένη (π.χ. Redis).
/// </summary>
public interface ICacheService
{
    /// <summary>Επιστρέφει την cached τιμή για το κλειδί, ή default αν δεν υπάρχει.</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>Αποθηκεύει μια τιμή με το δοθέν κλειδί και προαιρετικό χρόνο ζωής.</summary>
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

    /// <summary>Αφαιρεί την τιμή του κλειδιού (για ακύρωση ξεπερασμένων εγγραφών).</summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
