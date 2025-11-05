using api.Enumerable;

namespace api.Service.GuardService;
public class FriendlyException(ErrorType type, string message) : Exception(message)
{
    public ErrorType Type { get; } = type;
}