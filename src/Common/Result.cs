namespace ProjectManagementApplication.Common
{
    public enum ResultStatus { Success, NotFound, ValidationFailed }

    public sealed class Result
    {
        public ResultStatus Status { get; }
        public string? ErrorMessage { get; }

        private Result(ResultStatus status, string? errorMessage = null) { Status = status; ErrorMessage = errorMessage; }
        public static Result Ok() => new(ResultStatus.Success);
        public static Result NotFound(string? msg = null) => new(ResultStatus.NotFound, msg);
        public static Result ValidationFailed(string? msg) => new(ResultStatus.ValidationFailed, msg);
    }

}
