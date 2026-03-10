using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Shared.Requests;
using PCautivoCore.Shared.Responses;
using PCautivoCore.Shared.Utils;
using FluentValidation;
using MediatR;

namespace PCautivoCore.Application.Features.Auth.Actions;

public class ChangePasswordCommand : IRequest<Result>
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;

    public sealed class Validator : AbstractValidator<ChangePasswordCommand>
    {
        public Validator()
        {
            RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("CurrentPasswordRequired");
            RuleFor(x => x.NewPassword).NotEmpty().WithMessage("NewPasswordRequired").MinimumLength(6).WithMessage("PasswordMinLength").MaximumLength(30);
            RuleFor(x => x.ConfirmNewPassword).NotEmpty().WithMessage("ConfirmPasswordRequired");
            RuleFor(x => x.ConfirmNewPassword).Equal(x => x.NewPassword).WithMessage("PasswordsDoNotMatch").When(x => !string.IsNullOrWhiteSpace(x.NewPassword));
        }
    }

    internal sealed class Handler(
        IUserRepository userRepository,
        IAuthContext authContext
       ) : IRequestHandler<ChangePasswordCommand, Result>
    {
        public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            int userId = authContext.UserId;

            var user = await userRepository.GetUserById(userId);

            if (user == null)
                return Errors.NotFound("UserNotFound");

            bool isValid = PasswordUtil.VerifyPassword(request.CurrentPassword!, user.PasswordHash);

            if (!isValid)
                return Errors.BadRequest("CurrentPasswordIncorrect");

            string newPasswordHash = PasswordUtil.HashPassword(request.NewPassword!);

            user.PasswordHash = newPasswordHash;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            bool updated = await userRepository.UpdatePassword(user);

            if (!updated)
                return Errors.UnprocessableContent("PasswordUpdateFailed");

            return Results.NoContent();
        }
    }
}
