namespace Application.Exceptions;

// Thrown when the caller passes something that isn't a valid IP address at all -
// this is IP2C's "status = 0" case. Kept as an exception (rather than just returning
// null like the "unknown IP" case) because it's a genuinely different situation:
// the caller made a bad request (400), versus a valid IP we just don't have data for.
//
// Whoever implements IIpInfoService should throw this for Ip2CStatus.Invalid, and
// return null from GetIpInfoAsync for Ip2CStatus.Unknown - the controller/middleware
// then turn those into 400 and 404 respectively.
public class InvalidIpAddressException : Exception
{
    public InvalidIpAddressException(string ip)
        : base($"'{ip}' is not a valid IP address.")
    {
    }
}
