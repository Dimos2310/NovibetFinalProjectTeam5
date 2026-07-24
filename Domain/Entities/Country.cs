namespace Domain.Entities;

public class Country
{
    public required string TwoLetterCode { get; set; }

    public required string ThreeLetterCode { get; set; }

    public required string CountryName { get; set; }
}
