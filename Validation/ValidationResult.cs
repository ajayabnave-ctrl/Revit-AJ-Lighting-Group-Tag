namespace AJ.LightingGroupTag.Validation
{
    /// <summary>
    /// Represents the outcome of fixture group validation.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; }
        public string ErrorMessage { get; }

        private ValidationResult(bool isValid, string errorMessage)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        public static ValidationResult Success() => new ValidationResult(true, string.Empty);
        public static ValidationResult Failure(string message) => new ValidationResult(false, message);
    }
}
