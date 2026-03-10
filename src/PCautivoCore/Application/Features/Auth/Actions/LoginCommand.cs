using PCautivoCore.Application.Features.Auth.Dtos;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Shared.Responses;
using PCautivoCore.Shared.Utils;
using FluentValidation;
using MediatR;

namespace PCautivoCore.Application.Features.Auth.Actions;

public record LoginCommand : IRequest<Result<LoginDto>>
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;

    public sealed class Validator : AbstractValidator<LoginCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Username).NotEmpty().WithMessage("UsernameRequired");
            RuleFor(x => x.Password).NotEmpty().WithMessage("PasswordRequired");
        }
    }

    internal sealed class Handler(
        IUserRepository userRepository,
        IJwtUtil jwtUtil
        ) : IRequestHandler<LoginCommand, Result<LoginDto>>
    {
        public async Task<Result<LoginDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetUserByUsername(request.Username);

            if (user == null) return Errors.Unauthorized("InvalidUsernameOrPassword");
            
            bool compare = PasswordUtil.VerifyPassword(request.Password, user.PasswordHash);

            if (!compare) return Errors.Unauthorized("InvalidUsernameOrPassword");
            

            int expiresIn = jwtUtil.GetExpiresIn();
            
            var payload = new Dictionary<string, object>
            {
                ["sub"] = user.Id.ToString(),
                ["user"] = user.Username
            };

            string token = jwtUtil.GenerateToken(user.Id.ToString());

            var response = new LoginDto
            {
                TokenType = "Bearer",
                ExpiresIn = expiresIn,
                AccessToken = token
            };

            return response;
        }
    }
}