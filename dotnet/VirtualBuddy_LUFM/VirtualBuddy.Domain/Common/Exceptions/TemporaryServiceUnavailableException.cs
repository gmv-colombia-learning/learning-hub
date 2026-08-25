namespace VirtualBuddy.Domain.Common.Exceptions
{
    public class TemporaryServiceUnavailableException : DomainException
    {
        public TemporaryServiceUnavailableException(string message) : base(message) { }
    }
}
