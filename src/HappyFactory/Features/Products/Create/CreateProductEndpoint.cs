using FastEndpoints;

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
        var id = await _handler.HandleAsync(req, ct);
        var response = new CreateProductResponse(id);

        // Return 201 Created with the created resource id in the body.
        await Send.CreatedAtAsync(nameof(CreateProductEndpoint), routeValues: EndpointUrl, responseBody: response, cancellation: ct);
    }
}