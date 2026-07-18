namespace Domain.Enums;

// Maps directly to the first field of an IP2C response, e.g. "1;GR;GRC;Greece".
// Using an enum here means we never have to compare raw "0"/"1"/"2" strings/numbers
// anywhere else in the code.
public enum Ip2CStatus
{
    Invalid = 0,  // input wasn't a valid IP at all
    Success = 1,  // resolved fine, the country fields are populated
    Unknown = 2   // valid IP, but IP2C doesn't know which country it belongs to
}
