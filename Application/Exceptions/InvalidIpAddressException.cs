namespace Application.Exceptions;

// Thrown for IP2C status 0 (invalid input) - a 400, distinct from status 2 ("unknown
// country"), which IIpInfoService represents as null instead of an exception.
public class InvalidIpAddressException : Exception
{
    public InvalidIpAddressException(string ip)
        : base($"'{ip}' is not a valid IP address.")
    {
    }
}
