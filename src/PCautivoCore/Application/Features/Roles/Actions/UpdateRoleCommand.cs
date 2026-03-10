using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Shared.Responses;
using MediatR;

namespace PCautivoCore.Application.Features.Roles.Actions;

public record UpdateRoleCommand(int RoleId) : IRequest<Result>
{
    public string? Name { get; set; }

    internal sealed class Handler(IRoleRepository roleRepository) : IRequestHandler<UpdateRoleCommand, Result>
    {
        public async Task<Result> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            if (request.RoleId <= 0)
            {
                return Errors.BadRequest("Datos inválidos.");
            }

            var item = await roleRepository.GetRoleById(request.RoleId);

            if (item == null)
            {
                return Errors.NotFound("Rol no encontrado.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Errors.BadRequest("El nombre es requerido.");
            }

            item.Name = request.Name;
            item.UpdatedAt = DateTimeOffset.UtcNow;

            var result = await roleRepository.UpdateRole(item);

            if (!result)
            {
                return Errors.BadRequest("Error al actualizar el rol.");
            }

            return Results.NoContent();
        }
    }
}