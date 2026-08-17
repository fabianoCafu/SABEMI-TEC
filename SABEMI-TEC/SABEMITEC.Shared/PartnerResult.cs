namespace SABEMITEC.Shared
{
    public static class PartnerResult
    {
        public record Result<T>(
            bool IsSuccess,
            T? Object,
            string? Message,
            string? Error)
        {
            public bool IsFailure => !IsSuccess;
            public static Result<T> Success(T? obj, string? message = null) => new(true, obj, message, null);
            public static Result<T> Success(string? message = null) => new(true, default, message, null);
            public static Result<T> Failure(string error) => new(false, default, null, error);
        }
    }
}
