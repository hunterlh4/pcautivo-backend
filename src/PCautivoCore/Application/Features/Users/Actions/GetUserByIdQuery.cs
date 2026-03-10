using PCautivoCore.Application.Features.Users.Dtos;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Shared.Responses;
using MediatR;

namespace PCautivoCore.Application.Features.Users.Actions;

public record GetUserByIdQuery(int UserId) : IRequest<Result<UserDto>>
{
    internal sealed class Handler(IUserRepository userRepository, IRoleRepository roleRepository) : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
    {
        public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            if (request.UserId <= 0)
            {
                return Errors.BadRequest("Datos inválidos.");
            }

            var user = await userRepository.GetUserById(request.UserId);

            if (user == null)
            {
                return Errors.NotFound("Usuario no encontrado.");
            }

            var roles = await roleRepository.GetRolesByUserId(user.Id);

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                UserType = user.UserType,
                Roles = roles.Select(x => new UserRoleDto
                {
                    Id = x.Id,
                    Name = x.Name
                })
            };
        }
    }
}