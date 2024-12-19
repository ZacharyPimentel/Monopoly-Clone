public class RequestLoggingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Capture the full request URL
        if(context.Request.Method == HttpMethods.Get){
            var requestUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
            context.Items["RequestURL"] = requestUrl;
        }
        context.Items.TryGetValue("RequestURL", out var url);
        Console.WriteLine(url);
        await next(context);
    }
}