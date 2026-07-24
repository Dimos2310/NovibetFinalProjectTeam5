namespace Domain.Entities;

public class Ip
{
    public int Id { get; set; }

    public required string Address { get; set; }

    public required string CountryTwoLetterCode { get; set; }

    public DateTime LastUpdated { get; set; }
}
