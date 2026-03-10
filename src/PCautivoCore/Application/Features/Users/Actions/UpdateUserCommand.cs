using PCautivoCore.Domain.Enums;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Shared.Responses;
using FluentValidation;
using MediatR;

namespace PCautivoCore.Application.Features.Users.Actions;

public record UpdateUserCommand(int UserId) : IRequest<Result>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public UserType? UserType { get; set; }

    public sealed class Validator : AbstractValidator<UpdateUserCommand>
    {
        public Validator()
        {
            RuleFor(x => x.UserId).GreaterThan(0).WithMessage("Datos inválidos.");
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("El nombre es requerido.").MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().WithMessage("El apellido es requerido.").MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().WithMessage("El correo electrónico es requerido.").EmailAddress().WithMessage("El correo electrónico no es válido.");
            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("El número de teléfono es requerido.").MaximumLength(20);
            RuleFor(x => x.CountryCode).NotEmpty().WithMessage("El código de país es requerido.").MaximumLength(5);
        }
    }

    internal sealed class Handler(IUserRepository userRepository) : IRequestHandler<UpdateUserCommand, Result>
    {
        public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetUserById(request.UserId);
            var detail = await userRepository.GetUserDetailById(request.UserId);

            if (user == null || detail == null) return Errors.NotFound("Usuario no encontrado.");

            var allUsers = await userRepository.GetAllUsersWithDetails();

            var emailTaken = allUsers.Any(u => u.Id != request.UserId && u.Detail?.Email?.Trim().Equals(request.Email.Trim(), StringComparison.OrdinalIgnoreCase) == true);
            if (emailTaken) return Errors.BadRequest("El correo electrónico ya está en uso.");

            var phoneTaken = allUsers.Any(u => u.Id != request.UserId && u.Detail?.PhoneNumber?.Trim().Equals(request.PhoneNumber.Trim(), StringComparison.OrdinalIgnoreCase) == true);
            if (phoneTaken) return Errors.BadRequest("El número de teléfono ya está en uso.");

            detail.FirstName = request.FirstName;
            detail.LastName = request.LastName;
            detail.Email = request.Email;
            detail.PhoneNumber = request.PhoneNumber;
            detail.CountryCode = request.CountryCode;
            detail.UpdatedAt = DateTimeOffset.UtcNow;

            var detailResult = await userRepository.UpdateUserDetail(detail);
            if (!detailResult) return Errors.BadRequest("Error al actualizar el usuario.");

            if (request.UserType.HasValue)
            {
                user.UserType = request.UserType.Value;
                user.UpdatedAt = DateTimeOffset.UtcNow;

                var userResult = await userRepository.UpdateUser(user);
                if (!userResult) return Errors.BadRequest("Error al actualizar el usuario.");
            }

            return Results.NoContent();
        }
    }
}