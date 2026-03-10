using PCautivoCore.Application.Features.Auth.Dtos;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Shared.Requests;
using PCautivoCore.Shared.Responses;
using MediatR;

namespace PCautivoCore.Application.Features.Auth.Actions;

public class GetProfileQuery : IRequest<Result<ProfileDto>>
{
    internal sealed class Handler(
        IUserRepository userRepository,
        IAuthContext authContext) : IRequestHandler<GetProfileQuery, Result<ProfileDto>>
    {
        public async Task<Result<ProfileDto>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
        {
            var userId = authContext.UserId;

            var user = await userRepository.GetUserById(userId);

            if (user == null)
            {
                return Errors.NotFound("UserNotFound");
            }

            var detail = await userRepository.GetUserDetailById(user.Id);

            return new ProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = detail?.FirstName,
                LastName = detail?.LastName,
                Email = detail?.Email,
                PhoneNumber = detail?.PhoneNumber,
                CountryCode = detail?.CountryCode
            };
        }
    }
}
