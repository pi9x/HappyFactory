using FastEndpoints;
using FluentValidation.Results;

namespace HappyFactory.Features.Products.Create;

/// <summary>
/// FastEndpoints endpoint for creating products.
/// POST /products
/// </summary>
public class CreateProductEndpoint(CreateProductHandler handler) : Endpoint<CreateProductRequest, CreateProductResponse>
{
    private const string EndpointUrl = "/products";
    private readonly CreateProductHandler _handler = handler ?? throw new ArgumentNullException(nameof(handler));

    public override void Configure()
    {
        Verbs(Http.POST);
        Routes(EndpointUrl);
        AllowAnonymous(); // adjust auth in real apps
        Summary(s => s.Summary = "Create a new product");
    }

    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        // Basic request validation. The handler also validates its inputs, but it's useful to short-circuit here.
        if (string.IsNullOrWhiteSpace(req.Name))
        {
            ValidationFailures.Add(new ValidationFailure(nameof(req.Name), "Name is required"));
        }
        
        if (string.IsNullOrWhiteSpace(req.Sku))
        {
            ValidationFailures.Add(new ValidationFailure(nameof(req.Sku), "SKU is required"));
        }

        if (ValidationFailures.Count != 0)
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        try
        {
            var id = await _handler.HandleAsync(req, ct);
            var response = new CreateProductResponse(id);

            // Return 201 Created with the created resource id in the body.
            await Send.CreatedAtAsync(nameof(CreateProductEndpoint), routeValues: EndpointUrl, responseBody: response, cancellation: ct);
        }
        catch (Exception ex)
        {
            ValidationFailures.Add(new ValidationFailure(nameof(req.Name), ex.Message));
            await Send.ErrorsAsync(statusCode: 500, cancellation: ct);
        }
    }
}