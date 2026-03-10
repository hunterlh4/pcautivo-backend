using PCautivoCore.Application.Features.Roles.Dtos;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Domain.Models;
using PCautivoCore.Shared.Responses;
using MediatR;

namespace PCautivoCore.Application.Features.Roles.Actions;

public class CreateRoleCommand : IRequest<Result<CreateRoleDto>>
{
    public string? Name { get; set; }

    internal sealed class Handler(IRoleRepository roleRepository) : IRequestHandler<CreateRoleCommand, Result<CreateRoleDto>>
    {
        public async Task<Result<CreateRoleDto>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Errors.BadRequest("El nombre es requerido.");
            }

            var newItem = new Role
            {
                Name = request.Name,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var itemId = await roleRepository.CreateRole(newItem);

            return Results.Created(new CreateRoleDto
            {
                Id = itemId
            });
        }
    }
}