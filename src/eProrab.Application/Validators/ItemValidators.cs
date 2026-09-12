using eProrab.Application.DTOs.Items;
using FluentValidation;

namespace eProrab.Application.Validators;

public class CreateItemRequestValidator : AbstractValidator<CreateItemRequest>
{
    public CreateItemRequestValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required.")
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID must be a positive number (greater than 0).")
            .LessThanOrEqualTo(int.MaxValue).WithMessage("Category ID is invalid.");

        RuleFor(x => x.Unit)
            .IsInEnum().WithMessage("Unit of measure must be a valid enum value.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be zero or positive.")
            .LessThan(decimal.MaxValue).WithMessage("Price is too large.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be zero or positive.")
            .LessThanOrEqualTo(decimal.MaxValue).WithMessage("Stock quantity is too large.")
            .When(x => x.StockQuantity.HasValue);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(2048).WithMessage("Image URL must not exceed 2048 characters.")
            .Must(BeValidUrl).WithMessage("Image URL must be a valid HTTP(S) URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));

        RuleFor(x => x.Translations)
            .MustCoverAllLanguages(t => t.Language, t => t.Name);

        RuleFor(x => x.Dimensions)
            .MaximumLength(200).WithMessage("Dimensions must not exceed 200 characters.")
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
            .GreaterThan(0).WithMessage("Category ID must be a positive number (greater than 0).")
            .LessThanOrEqualTo(int.MaxValue).WithMessage("Category ID is invalid.");

        RuleFor(x => x.Unit)
            .IsInEnum().WithMessage("Unit of measure must be a valid enum value.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be zero or positive.")
            .LessThan(decimal.MaxValue).WithMessage("Price is too large.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be zero or positive.")
            .LessThanOrEqualTo(decimal.MaxValue).WithMessage("Stock quantity is too large.")
            .When(x => x.StockQuantity.HasValue);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(2048).WithMessage("Image URL must not exceed 2048 characters.")
            .Must(BeValidUrl).WithMessage("Image URL must be a valid HTTP(S) URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));

        RuleFor(x => x.Translations)
            .MustCoverAllLanguages(t => t.Language, t => t.Name);

        RuleFor(x => x.Dimensions)
            .MaximumLength(200).WithMessage("Dimensions must not exceed 200 characters.")
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
