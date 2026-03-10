using PCautivoCore.Application.Features.Users.Dtos;
using PCautivoCore.Domain.Enums;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Domain.Models;
using PCautivoCore.Shared.Responses;
using PCautivoCore.Shared.Utils;
using FluentValidation;
using MediatR;

namespace PCautivoCore.Application.Features.Users.Actions;

public class CreateUserCommand : IRequest<Result<CreateUserDto>>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserType UserType { get; set; } = UserType.Admin;

    public sealed class Validator : AbstractValidator<CreateUserCommand>
    {
        public Validator(IUserRepository userRepository)
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("El nombre es requerido.").MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().WithMessage("El apellido es requerido.").MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().WithMessage("El correo electrónico es requerido.").EmailAddress().WithMessage("El correo electrónico no es válido.");
            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("El número de teléfono es requerido.").MaximumLength(20);
            RuleFor(x => x.CountryCode).NotEmpty().WithMessage("El código de país es requerido.").MaximumLength(5);
            RuleFor(x => x.Username).NotEmpty().WithMessage("El nombre de usuario es requerido.").MinimumLength(3).MaximumLength(50);
            RuleFor(x => x.Password).NotEmpty().WithMessage("La contraseña es requerida.").MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");

            RuleFor(x => x.Username)
                .MustAsync(async (username, ct) => await userRepository.GetUserByUsername(username) == null)
                .WithMessage("El nombre de usuario ya está en uso.")
                .When(x => !string.IsNullOrWhiteSpace(x.Username));

            RuleFor(x => x.Email)
                .MustAsync(async (email, ct) =>
                {
                    var users = await userRepository.GetAllUsersWithDetails();
                    return !users.Any(u => u.Detail?.Email?.Equals(email, StringComparison.OrdinalIgnoreCase) == true);
                })
                .WithMessage("El correo electrónico ya está en uso.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.PhoneNumber)
                .MustAsync(async (phone, ct) =>
                {
                    var users = await userRepository.GetAllUsersWithDetails();
                    return !users.Any(u => u.Detail?.PhoneNumber?.Equals(phone) == true);
                })
                .WithMessage("El número de teléfono ya está en uso.")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
        }
    }

    internal sealed class Handler(IUserRepository userRepository) : IRequestHandler<CreateUserCommand, Result<CreateUserDto>>
    {
        public async Task<Result<CreateUserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = PasswordUtil.HashPassword(request.Password),
                UserType = request.UserType,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var newDetail = new UserDetail
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                CountryCode = request.CountryCode,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var userId = await userRepository.CreateUserWithDetail(newUser, newDetail);

            return Results.Created(new CreateUserDto
            {
                Id = userId
            });
        }
    }
}