using System;
using System.Collections.Generic;
using System.Linq;

namespace FfAdmin.Common
{
    public record ValidationMessage(string Key, string Message)
    {
        public ValidationMessage Prefix(string prefix)
        => this with {Key = $"{prefix}.{Key}"};
    }

    public class ValidationException : Exception
    {
        public ValidationException(IEnumerable<ValidationMessage> messages) : base("Validation errors occurred")
        {
            Messages = messages.ToArray();
        }

        public IReadOnlyList<ValidationMessage> Messages { get; }
    }
}
