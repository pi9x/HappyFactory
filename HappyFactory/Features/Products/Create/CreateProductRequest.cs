using FluentValidation;

namespace HappyFactory.Features.Products.Create;

public sealed record CreateProductRequest(string Name, string Sku);

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(req => req.Name).NotEmpty().WithMessage("Product Name is required");
        RuleFor(req => req.Sku).NotEmpty().WithMessage("Product SKU is required");
    }
}