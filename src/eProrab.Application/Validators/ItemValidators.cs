using eProrab.Application.DTOs.Items;
using FluentValidation;

namespace eProrab.Application.Validators;

public class CreateItemRequestValidator : AbstractValidator<CreateItemRequest>
{
    public CreateItemRequestValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Unit).IsInEnum();
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0).When(x => x.StockQuantity.HasValue);
        RuleFor(x => x.ImageUrl).MaximumLength(2048);
        RuleFor(x => x.Translations)
            .MustCoverAllLanguages(t => t.Language, t => t.Name);
    }
}

public class UpdateItemRequestValidator : AbstractValidator<UpdateItemRequest>
{
    public UpdateItemRequestValidator()
    {
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Unit).IsInEnum();
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0).When(x => x.StockQuantity.HasValue);
        RuleFor(x => x.ImageUrl).MaximumLength(2048);
        RuleFor(x => x.Translations)
            .MustCoverAllLanguages(t => t.Language, t => t.Name);
    }
}
