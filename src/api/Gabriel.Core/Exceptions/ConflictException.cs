namespace Gabriel.Core.Exceptions;

// The request is valid but conflicts with the resource's current state - e.g.
// approving a task that is already running, or chatting in a conversation an
// autonomous task is driving. Maps to 409.
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
