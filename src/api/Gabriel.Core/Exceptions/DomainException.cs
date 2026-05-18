namespace Gabriel.Core.Exceptions;

// Base class for domain rule violations. Caught by the global exception handler
// and translated to a 400 Bad Request.
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
