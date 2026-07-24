namespace Domain.Enums;

// Maps to the first field of an IP2C response, e.g. "1;GR;GRC;Greece".
public enum Ip2CStatus
{
    Invalid = 0,
    Success = 1,
    Unknown = 2
}
