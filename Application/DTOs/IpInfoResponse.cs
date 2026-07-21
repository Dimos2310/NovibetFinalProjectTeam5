namespace Application.DTOs;

/// <summary>
/// Συμβόλαιο απάντησης του endpoint "πληροφορίες IP" (Task 1).
/// Κρατιέται ξεχωριστό από την οντότητα <c>Country</c>, ώστε το δημόσιο σχήμα του API
/// και το σχήμα της βάσης να μπορούν να εξελίσσονται ανεξάρτητα.
/// </summary>
public record IpInfoResponse(
    string CountryName,
    string TwoLetterCode,
    string ThreeLetterCode);
