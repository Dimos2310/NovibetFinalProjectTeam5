namespace Application.DTOs;

public record CountryReportItem(
    string CountryName,
    int AddressesCount,
    DateTime LastAddressUpdated);
