using FastEndpoints;
using FluentValidation.Results;

namespace HappyFactory.Features.Products.Get;

/// <summary>
/// FastEndpoints endpoint for GET /products/{id}
/// </summary>
public class GetProductEndpoint(GetProductHandler handler) : Endpoint<GetProductRequest, GetProductResponse>
{
    private const string EndpointUrl = "/products/{id:guid}";
    private readonly GetProductHandler _handler = handler ?? throw new ArgumentNullException(nameof(handler));

    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(EndpointUrl);
        AllowAnonymous();
        Summary(s => s.Summary = "Get a product by id");
    }

    public override async Task HandleAsync(GetProductRequest req, CancellationToken ct)
    {
        if (req.Id == Guid.Empty)
        {
            ValidationFailures.Add(new ValidationFailure(nameof(req.Id), "Id must be a valid GUID"));
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var result = await _handler.HandleAsync(req, ct);

        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(result, cancellation: ct);
    }
}