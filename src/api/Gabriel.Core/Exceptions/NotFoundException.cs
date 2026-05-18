namespace Gabriel.Core.Exceptions;

// Thrown when a requested aggregate is missing. Translated to a 404.
public class NotFoundException : Exception
{
    public string Resource { get; }
    public object Key { get; }

    public NotFoundException(string resource, object key)
        : base($"{resource} with key '{key}' was not found.")
    {
        Resource = resource;
        Key = key;
    }
}
