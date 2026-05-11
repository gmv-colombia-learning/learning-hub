using VirtualBuddy.Domain.Common;
using VirtualBuddy.Domain.Common.Exceptions;

namespace VirtualBuddy.Domain.Project.ValueObjects
{
    public class ProjectName : ValueObject
    {
        public string Value { get; }

        public ProjectName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ValidationException("Project name is required.");

            if (value.Length > 100)
                throw new ValidationException("Project name cannot exceed 100 characters.");

            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator string(ProjectName projectName) => projectName.Value;
        public static implicit operator ProjectName(string value) => new ProjectName(value);
    }
}
