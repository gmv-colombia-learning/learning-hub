using System;

namespace VirtualBuddy.Domain.Common.Exceptions
{
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message) { }
    }
}
