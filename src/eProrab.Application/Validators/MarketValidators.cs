using eProrab.Application.DTOs.Markets;
using FluentValidation;

namespace eProrab.Application.Validators;

public class UpsertMarketProfileRequestValidator : AbstractValidator<UpsertMarketProfileRequest>
{
    public UpsertMarketProfileRequestValidator()
    {
        RuleFor(x => x.StoreName)
            .NotEmpty().WithMessage("Store name is required.")
            .MaximumLength(200).WithMessage("Store name must not exceed 200 characters.");

        RuleFor(x => x.Voen)
            .MaximumLength(50).WithMessage("VOEN must not exceed 50 characters.")
            .Matches(@"^\d+$").WithMessage("VOEN must contain only digits.")
            .When(x => !string.IsNullOrWhiteSpace(x.Voen));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.ContactPhone)
            .MaximumLength(30).WithMessage("Phone number must not exceed 30 characters.")
            .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("Phone number format is invalid.")
            .When(x => !string.IsNullOrWhiteSpace(x.ContactPhone));

        RuleFor(x => x.ContactEmail)
            .EmailAddress().WithMessage("Contact email must be a valid email address.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(2048).WithMessage("Logo URL must not exceed 2048 characters.")
            .Must(BeValidUrl).WithMessage("Logo URL must be a valid URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.LogoUrl));

        RuleFor(x => x.BannerUrl)
            .MaximumLength(2048).WithMessage("Banner URL must not exceed 2048 characters.")
            .Must(BeValidUrl).WithMessage("Banner URL must be a valid URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.BannerUrl));

        RuleFor(x => x.WorkingHours)
            .MaximumLength(200).WithMessage("Working hours must not exceed 200 characters.");
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
            .GreaterThan(0).WithMessage("Category ID must be a positive number.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Item name is required.")
            .MaximumLength(200).WithMessage("Item name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be zero or positive.")
            .LessThan(decimal.MaxValue).WithMessage("Price is too large.");

        RuleFor(x => x.Unit)
            .IsInEnum().WithMessage("Unit of measure must be a valid enum value.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be zero or positive.")
            .When(x => x.StockQuantity.HasValue);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(2048).WithMessage("Image URL must not exceed 2048 characters.")
            .Must(BeValidUrl).WithMessage("Image URL must be a valid URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));

        RuleFor(x => x.Dimensions)
            .MaximumLength(200).WithMessage("Dimensions must not exceed 200 characters.");

        RuleFor(x => x.SurfaceType)
            .IsInEnum().WithMessage("Surface type must be a valid enum value.");

        RuleFor(x => x.Sku)
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters.")
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
            .GreaterThan(0).WithMessage("Category ID must be a positive number.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Item name is required.")
            .MaximumLength(200).WithMessage("Item name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be zero or positive.")
            .LessThan(decimal.MaxValue).WithMessage("Price is too large.");

        RuleFor(x => x.Unit)
            .IsInEnum().WithMessage("Unit of measure must be a valid enum value.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be zero or positive.")
            .When(x => x.StockQuantity.HasValue);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(2048).WithMessage("Image URL must not exceed 2048 characters.")
            .Must(BeValidUrl).WithMessage("Image URL must be a valid URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));

        RuleFor(x => x.Dimensions)
            .MaximumLength(200).WithMessage("Dimensions must not exceed 200 characters.");

        RuleFor(x => x.SurfaceType)
            .IsInEnum().WithMessage("Surface type must be a valid enum value.");
    }

    private bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
