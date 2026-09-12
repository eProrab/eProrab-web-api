using eProrab.Application.DTOs.Markets;
using FluentValidation;

namespace eProrab.Application.Validators;

public class UpsertMarketProfileRequestValidator : AbstractValidator<UpsertMarketProfileRequest>
{
    public UpsertMarketProfileRequestValidator()
    {
        RuleFor(x => x.StoreName)
            .NotEmpty().WithMessage("Mağaza adı tələb olunur.")
            .MaximumLength(200).WithMessage("Mağaza adı maksimum 200 simvoldan çox ola bilməz.");

        RuleFor(x => x.Voen)
            .MaximumLength(50).WithMessage("VOEN maksimum 50 simvoldan çox ola bilməz.")
            .Matches(@"^\d+$").WithMessage("VOEN yalnız rəqəmlərdən ibarət olmalıdır.")
            .When(x => !string.IsNullOrWhiteSpace(x.Voen));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Təsvir maksimum 2000 simvoldan çox ola bilməz.");

        RuleFor(x => x.ContactPhone)
            .MaximumLength(30).WithMessage("Telefon nömrəsi maksimum 30 simvoldan çox ola bilməz.")
            .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("Telefon nömrəsi formatı düzgün deyil.")
            .When(x => !string.IsNullOrWhiteSpace(x.ContactPhone));

        RuleFor(x => x.ContactEmail)
            .EmailAddress().WithMessage("Kontakt e-poçtu düzgün email ünvanı olmalıdır.")
            .MaximumLength(256).WithMessage("E-poçt maksimum 256 simvoldan çox ola bilməz.")
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Ünvan maksimum 500 simvoldan çox ola bilməz.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("Şəhər maksimum 100 simvoldan çox ola bilməz.");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(2048).WithMessage("Logo URL maksimum 2048 simvoldan çox ola bilməz.")
            .Must(BeValidUrl).WithMessage("Logo URL düzgün URL olmalıdır.")
            .When(x => !string.IsNullOrWhiteSpace(x.LogoUrl));

        RuleFor(x => x.BannerUrl)
            .MaximumLength(2048).WithMessage("Banner URL maksimum 2048 simvoldan çox ola bilməz.")
            .Must(BeValidUrl).WithMessage("Banner URL düzgün URL olmalıdır.")
            .When(x => !string.IsNullOrWhiteSpace(x.BannerUrl));

        RuleFor(x => x.WorkingHours)
            .MaximumLength(200).WithMessage("Çalışma saatları maksimum 200 simvoldan çox ola bilməz.");
    }

    private bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

public class CreateMarketItemRequestValidator : AbstractValidator<CreateMarketItemRequest>
{
    public CreateMarketItemRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Kateqoriya ID müsbət rəqəm olmalıdır.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Məhsul adı tələb olunur.")
            .MaximumLength(200).WithMessage("Məhsul adı maksimum 200 simvoldan çox ola bilməz.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Təsvir maksimum 2000 simvoldan çox ola bilməz.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Qiymət sıfır və ya müsbət olmalıdır.")
            .LessThan(decimal.MaxValue).WithMessage("Qiymət çox böyükdür.");

        RuleFor(x => x.Unit)
            .IsInEnum().WithMessage("Ölçü vahidi düzgün deyil.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Ehtiyat miqdarı sıfır və ya müsbət olmalıdır.")
            .When(x => x.StockQuantity.HasValue);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(2048).WithMessage("Şəkil URL maksimum 2048 simvoldan çox ola bilməz.")
            .Must(BeValidUrl).WithMessage("Şəkil URL düzgün HTTP(S) URL olmalıdır.")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));

        RuleFor(x => x.Dimensions)
            .MaximumLength(200).WithMessage("Ölçülər maksimum 200 simvoldan çox ola bilməz.");

        RuleFor(x => x.SurfaceType)
            .IsInEnum().WithMessage("Səth tipi düzgün deyil.");

        RuleFor(x => x.Sku)
            .MaximumLength(50).WithMessage("SKU maksimum 50 simvoldan çox ola bilməz.")
            .When(x => !string.IsNullOrWhiteSpace(x.Sku));
    }

    private bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

public class UpdateMarketItemRequestValidator : AbstractValidator<UpdateMarketItemRequest>
{
    public UpdateMarketItemRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Kateqoriya ID müsbət rəqəm olmalıdır.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Məhsul adı tələb olunur.")
            .MaximumLength(200).WithMessage("Məhsul adı maksimum 200 simvoldan çox ola bilməz.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Təsvir maksimum 2000 simvoldan çox ola bilməz.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Qiymət sıfır və ya müsbət olmalıdır.")
            .LessThan(decimal.MaxValue).WithMessage("Qiymət çox böyükdür.");

        RuleFor(x => x.Unit)
            .IsInEnum().WithMessage("Ölçü vahidi düzgün deyil.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Ehtiyat miqdarı sıfır və ya müsbət olmalıdır.")
            .When(x => x.StockQuantity.HasValue);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(2048).WithMessage("Şəkil URL maksimum 2048 simvoldan çox ola bilməz.")
            .Must(BeValidUrl).WithMessage("Şəkil URL düzgün HTTP(S) URL olmalıdır.")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));

        RuleFor(x => x.Dimensions)
            .MaximumLength(200).WithMessage("Ölçülər maksimum 200 simvoldan çox ola bilməz.");

        RuleFor(x => x.SurfaceType)
            .IsInEnum().WithMessage("Səth tipi düzgün deyil.");
    }

    private bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
