namespace api.DTO.Entity;

public class ErrorLogCreateParams
{
    public required string ErrorMessage { get; set; }
    public string? Source { get; set; }
    public string? StackTrace { get; set; }
    public Exception? InnerException { get; set; }
}