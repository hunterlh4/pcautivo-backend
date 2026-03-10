using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Shared.Requests;
using PCautivoCore.Shared.Responses;
using FluentValidation;
using MediatR;

namespace PCautivoCore.Application.Features.Auth.Actions;

public class UpdateProfileCommand : IRequest<Result>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? CountryCode { get; set; }

    public sealed class Validator : AbstractValidator<UpdateProfileCommand>
    {
        public Validator()
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("El nombre es requerido.").MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().WithMessage("El apellido es requerido.").MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().WithMessage("El correo electrónico es requerido.").EmailAddress().WithMessage("El correo electrónico no es válido.");
            RuleFor(x => x.CountryCode).MaximumLength(5).When(x => x.CountryCode != null);
        }
    }

    internal sealed class Handler(
        IUserRepository userRepository,
        IAuthContext authContext) : IRequestHandler<UpdateProfileCommand, Result>
    {
        public async Task<Result> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            int userId = authContext.UserId;

            var userDetail = await userRepository.GetUserDetailById(userId);

            if (userDetail == null)
                return Errors.NotFound("Usuario no encontrado.");

            userDetail.FirstName = request.FirstName;
            userDetail.LastName = request.LastName;
            userDetail.Email = request.Email;
            userDetail.PhoneNumber = request.PhoneNumber;
            userDetail.CountryCode = request.CountryCode;
            userDetail.UpdatedAt = DateTimeOffset.UtcNow;

            await userRepository.UpdateUserDetail(userDetail);

            return Results.NoContent();
        }
    }
}
