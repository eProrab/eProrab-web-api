using eProrab.Application.DTOs.Auth;
using FluentValidation;

namespace eProrab.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
        RuleFor(x => x.PhoneNumber).MaximumLength(30);
        RuleFor(x => x.PreferredLanguage).IsInEnum();
        RuleFor(x => x.Role).Must(r => string.IsNullOrEmpty(r) ||
                                       r == eProrab.Domain.Constants.Roles.Client ||
                                       r == eProrab.Domain.Constants.Roles.Worker ||
                                       r == eProrab.Domain.Constants.Roles.Architect ||
                                       r == eProrab.Domain.Constants.Roles.Market)
            .WithMessage("Only 'Client', 'Worker', 'Architect' or 'Market' roles are allowed during registration.");
    }
}


public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
    }
}

public class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public class SelectRoleRequestValidator : AbstractValidator<SelectRoleRequest>
{
    public SelectRoleRequestValidator()
    {
        RuleFor(x => x.Role).NotEmpty().Must(r =>
            string.Equals(r, eProrab.Domain.Constants.Roles.Client, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r, eProrab.Domain.Constants.Roles.Worker, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r, eProrab.Domain.Constants.Roles.Architect, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r, eProrab.Domain.Constants.Roles.Market, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Yalnız 'Client', 'Worker', 'Architect' və ya 'Market' rolları seçilə bilər.");
    }
}
