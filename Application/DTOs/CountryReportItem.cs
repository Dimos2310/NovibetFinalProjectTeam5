namespace Application.DTOs;

/// <summary>
/// A single row of the report endpoint (Task 3): how many addresses we hold for a
/// country and when one of them was last updated.
/// </summary>
public record CountryReportItem(
    string CountryName,
    int AddressesCount,
    DateTime LastAddressUpdated);
