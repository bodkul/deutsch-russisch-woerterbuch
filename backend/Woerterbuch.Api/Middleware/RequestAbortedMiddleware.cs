namespace Woerterbuch.Api.Middleware;

/// <summary>
/// Swallows cancellations caused by the client aborting the request
/// (aborted fetch, page reload, hot reload). Nobody is waiting for a response then.
/// </summary>
public sealed class RequestAbortedMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
        }
    }
}
