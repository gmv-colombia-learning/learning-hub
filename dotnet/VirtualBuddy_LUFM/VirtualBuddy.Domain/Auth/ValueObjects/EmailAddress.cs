using System.Net.Mail;
using VirtualBuddy.Domain.Common;
using VirtualBuddy.Domain.Common.Exceptions;

namespace VirtualBuddy.Domain.Auth.ValueObjects
{
    public sealed class EmailAddress : ValueObject
    {
        public string Value { get; }
        public string NormalizedValue => Value.ToUpperInvariant();

        public EmailAddress(string value)
        {
            var trimmedValue = value?.Trim() ?? string.Empty;
            if (!MailAddress.TryCreate(trimmedValue, out var address) ||
                !string.Equals(address.Address, trimmedValue, StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidationException("El email no tiene un formato valido.");
            }

            Value = address.Address;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return NormalizedValue;
        }

        public override string ToString() => Value;
    }
}
