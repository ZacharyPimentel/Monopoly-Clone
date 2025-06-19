namespace api.Entity;

public class ErrorLog
{
    public int Id { get; set; }
    public required string ErrorMessage { get; set; }
    public string? Source { get; set; }
    public string? StackTrace { get; set; }
    public Exception? InnerException { get; set; }
    public DateTime CreatedAt { get; set; }
}