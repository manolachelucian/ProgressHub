

namespace ProgressHub.Core.Exceptions
{

    public class InvalidEmailFormatException : Exception { 
        public InvalidEmailFormatException(string? email) : base($"The email '{email}' is not in a valid format.") { }
    }

    public class DuplicateEmailException : Exception
    {
        public DuplicateEmailException(string email)
            : base($"A client with email '{email}' already exists.") { }
    }
}
