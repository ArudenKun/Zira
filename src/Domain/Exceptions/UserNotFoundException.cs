namespace Domain.Exceptions;

public class UserNotFoundException : DomainException
{
    public UserNotFoundException(string message)
        : base(message) { }
}
