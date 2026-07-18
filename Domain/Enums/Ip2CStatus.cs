namespace Domain.Enums;

/// <summary>
/// Status flag returned as the first field of an IP2C response line
/// ("status;countryCode2;countryCode3;countryName").
/// </summary>
public enum Ip2CStatus
{
    /// <summary>0 - the input was not a valid IP address.</summary>
    Invalid = 0,

    /// <summary>1 - the IP was resolved successfully.</summary>
    Success = 1,

    /// <summary>2 - the IP is valid but unknown to the service.</summary>
    Unknown = 2
}
