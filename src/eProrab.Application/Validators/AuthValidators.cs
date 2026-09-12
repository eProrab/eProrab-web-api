using eProrab.Application.DTOs.Auth;
using FluentValidation;

namespace eProrab.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad tələb olunur.")
            .MaximumLength(150).WithMessage("Ad maksimum 150 simvoldan çox ola bilməz.");
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email tələb olunur.")
            .EmailAddress().WithMessage("Email formatı düzgün deyil.")
            .MaximumLength(256).WithMessage("Email maksimum 256 simvoldan çox ola bilməz.");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifrə tələb olunur.")
            .MinimumLength(8).WithMessage("Şifrə ən az 8 simvoldan ibarət olmalıdır.")
            .Matches("[A-Z]").WithMessage("Şifrə ən az bir böyük hərf ehtiva etməlidir.")
            .Matches("[0-9]").WithMessage("Şifrə ən az bir rəqəm ehtiva etməlidir.");
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
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email tələb olunur.")
                .EmailAddress().WithMessage("Email formatı düzgün deyil.");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifrə tələb olunur.");
        }
    }

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Cari şifrə tələb olunur.");
            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Yeni şifrə tələb olunur.")
                .MinimumLength(8).WithMessage("Yeni şifrə ən az 8 simvoldan ibarət olmalıdır.")
                .Matches("[A-Z]").WithMessage("Yeni şifrə ən az bir böyük hərf ehtiva etməlidir.")
                .Matches("[0-9]").WithMessage("Yeni şifrə ən az bir rəqəm ehtiva etməlidir.");
        }
    }

public class RefreshRequestValidator : AbstractValidator<RefreshRequest>
    {
        public RefreshRequestValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Yeniləmə tokeni tələb olunur.");
        }
    }

public class SelectRoleRequestValidator : AbstractValidator<SelectRoleRequest>
{
    public SelectRoleRequestValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Rol tələb olunur.")
            .Must(r =>
                string.Equals(r, eProrab.Domain.Constants.Roles.Client, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(r, eProrab.Domain.Constants.Roles.Worker, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(r, eProrab.Domain.Constants.Roles.Architect, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(r, eProrab.Domain.Constants.Roles.Market, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Yalnız 'Müştəri', 'İşçi', 'Mimar' və ya 'Market' rolları seçilə bilər.");
    }
}
