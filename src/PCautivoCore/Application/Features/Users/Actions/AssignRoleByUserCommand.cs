using PCautivoCore.Application.Features.Roles.Dtos;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Domain.Models;
using PCautivoCore.Shared.Responses;
using FluentValidation;
using MediatR;

namespace PCautivoCore.Application.Features.Users.Actions;

public class AssignRoleByUserCommand : IRequest<Result<RoleByUserDto>>
{
    public int UserId { get; set; }
    public int RoleId { get; set; }

    public sealed class Validator : AbstractValidator<AssignRoleByUserCommand>
    {
        public Validator()
        {
            RuleFor(x => x.UserId).GreaterThan(0).WithMessage("Datos inválidos.");
            RuleFor(x => x.RoleId).GreaterThan(0).WithMessage("Datos inválidos.");
        }
    }

    internal sealed class Handler(IUserRepository userRepository, IRoleRepository roleRepository) : IRequestHandler<AssignRoleByUserCommand, Result<RoleByUserDto>>
    {
        public async Task<Result<RoleByUserDto>> Handle(AssignRoleByUserCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetUserById(request.UserId);

            if (user == null)
            {
                return Errors.NotFound("Usuario no encontrado.");
            }

            var role = await roleRepository.GetRoleById(request.RoleId);

            if (role == null)
            {
                return Errors.NotFound("Rol no encontrado.");
            }

            var userRole = await userRepository.GetUserRoleByIds(user.Id, role.Id);

            if (userRole == null)
            {
                await userRepository.CreateUserRole(new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
            else
            {
                await userRepository.DeleteUserRole(userRole.UserId, userRole.RoleId);
            }

            userRole = await userRepository.GetUserRoleByIds(user.Id, role.Id);

            var response = new RoleByUserDto
            {
                Id = role.Id,
                Name = role.Name,
                Status = userRole != null
            };

            return response;
        }
    }
}