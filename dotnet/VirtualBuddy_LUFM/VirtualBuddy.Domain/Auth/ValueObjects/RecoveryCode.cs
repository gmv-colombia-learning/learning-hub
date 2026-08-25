using VirtualBuddy.Domain.Common;
using VirtualBuddy.Domain.Common.Exceptions;

namespace VirtualBuddy.Domain.Auth.ValueObjects
{
    public sealed class RecoveryCode : ValueObject
    {
        public const int RequiredLength = 6;
        public string Value { get; }

        public RecoveryCode(string value)
        {
            var normalizedValue = value?.Trim().ToUpperInvariant() ?? string.Empty;
            if (normalizedValue.Length != RequiredLength ||
                !normalizedValue.All(character =>
                    character is >= 'A' and <= 'Z' or >= '0' and <= '9'))
            {
                throw new ValidationException("El codigo debe contener exactamente seis caracteres alfanumericos.");
            }

            Value = normalizedValue;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
