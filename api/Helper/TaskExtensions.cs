namespace api.Helper;
public static class TaskExtensions
{
    public static async Task<TResult> Then<TSource, TResult>(
        this Task<TSource> task,
        Func<TSource, Task<TResult>> next)
    {
        var result = await task;
        return await next(result);
    }
}