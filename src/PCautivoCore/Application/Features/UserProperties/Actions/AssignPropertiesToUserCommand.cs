using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Shared.Responses;
using MediatR;

namespace PCautivoCore.Application.Features.UserProperties.Actions;

public record AssignPropertiesToUserCommand : IRequest<Result>
{
    public int UserId { get; init; }
    public List<int> PropertyIdsToAdd { get; init; } = new();
    public List<int> PropertyIdsToRemove { get; init; } = new();
}

public class AssignPropertiesToUserCommandHandler(
    IUserPropertyRepository userPropertyRepository
) : IRequestHandler<AssignPropertiesToUserCommand, Result>
{
    public async Task<Result> Handle(AssignPropertiesToUserCommand request, CancellationToken cancellationToken)
    {
        // Eliminar propiedades en batch
        if (request.PropertyIdsToRemove != null && request.PropertyIdsToRemove.Any())
        {
            await userPropertyRepository.RemoveUserFromProperties(request.UserId, request.PropertyIdsToRemove);
        }

        // Agregar nuevas propiedades en batch
        if (request.PropertyIdsToAdd != null && request.PropertyIdsToAdd.Any())
        {
            await userPropertyRepository.AssignUserToProperties(request.UserId, request.PropertyIdsToAdd);
        }

        return Result.Success();
    }
}
