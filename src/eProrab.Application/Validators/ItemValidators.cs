using eProrab.Application.DTOs.Items;
using FluentValidation;

namespace eProrab.Application.Validators;

public class CreateItemRequestValidator : AbstractValidator<CreateItemRequest>
{
    public CreateItemRequestValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU tələb olunur.")
            .MaximumLength(50).WithMessage("SKU maksimum 50 simvoldan çox ola bilməz.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Kateqoriya ID müsbət rəqəm olmalıdır (0-dan böyük).")
            .LessThanOrEqualTo(int.MaxValue).WithMessage("Kateqoriya ID düzgün deyil.");

        RuleFor(x => x.Unit)
            .IsInEnum().WithMessage("Ölçü vahidi düzgün deyil.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Qiymət sıfır və ya müsbət olmalıdır.")
            .LessThan(decimal.MaxValue).WithMessage("Qiymət çox böyükdür.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Ehtiyat miqdarı sıfır və ya müsbət olmalıdır.")
            .LessThanOrEqualTo(decimal.MaxValue).WithMessage("Ehtiyat miqdarı çox böyükdür.")
            .When(x => x.StockQuantity.HasValue);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(2048).WithMessage("Şəkil URL-i maksimum 2048 simvoldan çox ola bilməz.")
            .Must(BeValidUrl).WithMessage("Şəkil URL-i düzgün HTTP(S) URL olmalıdır.")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));

        RuleFor(x => x.Translations)
            .MustCoverAllLanguages(t => t.Language, t => t.Name);

        RuleFor(x => x.Dimensions)
            .MaximumLength(200).WithMessage("Ölçülər maksimum 200 simvoldan çox ola bilməz.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dimensions));
    }

    private bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

public class UpdateItemRequestValidator : AbstractValidator<UpdateItemRequest>
{
    public UpdateItemRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Kateqoriya ID müsbət rəqəm olmalıdır (0-dan böyük).")
            .LessThanOrEqualTo(int.MaxValue).WithMessage("Kateqoriya ID düzgün deyil.");

        RuleFor(x => x.Unit)
            .IsInEnum().WithMessage("Ölçü vahidi düzgün deyil.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Qiymət sıfır və ya müsbət olmalıdır.")
            .LessThan(decimal.MaxValue).WithMessage("Qiymət çox böyükdür.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Ehtiyat miqdarı sıfır və ya müsbət olmalıdır.")
            .LessThanOrEqualTo(decimal.MaxValue).WithMessage("Ehtiyat miqdarı çox böyükdür.")
            .When(x => x.StockQuantity.HasValue);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(2048).WithMessage("Şəkil URL-i maksimum 2048 simvoldan çox ola bilməz.")
            .Must(BeValidUrl).WithMessage("Şəkil URL-i düzgün HTTP(S) URL olmalıdır.")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));

        RuleFor(x => x.Translations)
            .MustCoverAllLanguages(t => t.Language, t => t.Name);

        RuleFor(x => x.Dimensions)
            .MaximumLength(200).WithMessage("Ölçülər maksimum 200 simvoldan çox ola bilməz.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dimensions));
    }

    private bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
