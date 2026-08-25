namespace VirtualBuddy.Domain.Common.Exceptions
{
    public class TooManyRequestsException : DomainException
    {
        public TooManyRequestsException(string message) : base(message) { }
    }
}
