using eProrab.Application.DTOs.Users;
using eProrab.Domain.Constants;
using FluentValidation;

namespace eProrab.Application.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
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
            .MinimumLength(8).WithMessage("Şifrə ən az 8 simvoldan ibarət olmalıdır.");
        RuleFor(x => x.PhoneNumber)
            .MaximumLength(30).WithMessage("Telefon nömrəsi maksimum 30 simvoldan çox ola bilməz.");
        RuleFor(x => x.PreferredLanguage)
            .IsInEnum().WithMessage("Dil seçimi düzgün deyil.");
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Rol tələb olunur.")
            .Must(r => Roles.All.Contains(r))
            .WithMessage($"Rol aşağıdakılardan biri olmalıdır: {string.Join(", ", Roles.All)}");
    }
}

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad tələb olunur.")
            .MaximumLength(150).WithMessage("Ad maksimum 150 simvoldan çox ola bilməz.");
        RuleFor(x => x.PhoneNumber)
            .MaximumLength(30).WithMessage("Telefon nömrəsi maksimum 30 simvoldan çox ola bilməz.");
        RuleFor(x => x.PreferredLanguage)
            .IsInEnum().WithMessage("Dil seçimi düzgün deyil.");
    }
}

public class ChangeUserRoleRequestValidator : AbstractValidator<ChangeUserRoleRequest>
{
    public ChangeUserRoleRequestValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Rol tələb olunur.")
            .Must(r => Roles.All.Contains(r))
            .WithMessage($"Rol aşağıdakılardan biri olmalıdır: {string.Join(", ", Roles.All)}");
    }
}

public class AdminResetPasswordRequestValidator : AbstractValidator<AdminResetPasswordRequest>
{
    public AdminResetPasswordRequestValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni şifrə tələb olunur.")
            .MinimumLength(8).WithMessage("Yeni şifrə ən az 8 simvoldan ibarət olmalıdır.");
    }
}
