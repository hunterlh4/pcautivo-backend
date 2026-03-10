using PCautivoCore.Shared.Responses;
using FluentValidation;
using MediatR;

namespace PCautivoCore.Shared.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var firstMessage = failures.First().ErrorMessage;
        var error = Errors.BadRequest(firstMessage);

        // Result (non-generic)
        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)(Result)error;

        // Result<T> (generic) — invoke the implicit operator via reflection
        var responseType = typeof(TResponse);
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var implicitOp = responseType.GetMethod("op_Implicit", [typeof(Error)]);
            if (implicitOp != null)
                return (TResponse)implicitOp.Invoke(null, [error])!;
        }

        throw new ValidationException(failures);
    }
}
