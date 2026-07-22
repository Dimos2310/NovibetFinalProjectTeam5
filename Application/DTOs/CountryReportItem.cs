namespace Application.DTOs;

/// <summary>
/// Μία γραμμή του report endpoint (Task 3): πόσες διευθύνσεις κρατάμε για μια χώρα και
/// πότε ενημερώθηκε τελευταία φορά κάποια από αυτές.
/// </summary>
public record CountryReportItem(
    string CountryName,
    int AddressesCount,
    DateTime LastAddressUpdated);
