using FluentValidation;

namespace HappyFactory.Features.Products.Get;

public sealed record GetProductRequest(Guid Id);

public class GetProductRequestValidator : FluentValidation.AbstractValidator<GetProductRequest>
{
    public GetProductRequestValidator()
    {
        RuleFor(req => req.Id).NotEmpty().WithMessage("Product Id is required");
    }
}