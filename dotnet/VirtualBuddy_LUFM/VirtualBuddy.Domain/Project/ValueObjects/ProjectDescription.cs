using VirtualBuddy.Domain.Common;

namespace VirtualBuddy.Domain.Project.ValueObjects
{
    public class ProjectDescription : ValueObject
    {
        public string Value { get; }

        public ProjectDescription(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Project description is required.");

            if (value.Length < 10)
                throw new ArgumentException("Project description must be at least 10 characters long.");

            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator string(ProjectDescription description) => description.Value;
        public static implicit operator ProjectDescription(string value) => new ProjectDescription(value);
    }
}
