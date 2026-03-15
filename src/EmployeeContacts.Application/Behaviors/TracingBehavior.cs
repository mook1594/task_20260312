using System.Diagnostics;
using MediatR;

namespace EmployeeContacts.Application.Behaviors;

public sealed class TracingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ActivitySource activitySource;

    public TracingBehavior(ActivitySource activitySource)
    {
        this.activitySource = activitySource;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        using Activity? activity = activitySource.StartActivity(typeof(TRequest).Name);
        activity?.SetTag("request.type", typeof(TRequest).FullName);

        return await next().ConfigureAwait(false);
    }
}
