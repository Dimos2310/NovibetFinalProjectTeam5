namespace Application.Abstractions
{
    public record ReportRow(string CountryName, int AddressesCount, DateTime LastAddressUpdated)
    {
        //dto from the usecase task 3, it is a query result, so it is a record type
    }
}
