using FastEndpoints;
using HappyFactory.Features.Products.Create;
using HappyFactory.Features.Products.Get;
using HappyFactory.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add FastEndpoints (vertical-slice-friendly)
builder.Services.AddFastEndpoints();

// Swagger for discovery of endpoints
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Happy Factory API", Version = "v1" });
});

// Read model: EF Core InMemory for queries/projections
builder.Services.AddDbContext<ReadModelDbContext>(options =>
    options.UseInMemoryDatabase("ReadModel"));

// Event store (in-memory)
builder.Services.AddSingleton<EventStore>();

// Register application handlers (command / query handlers)
builder.Services.AddScoped<CreateProductHandler>();
builder.Services.AddScoped<GetProductHandler>();

// Projection service: listen to events and update read model.
builder.Services.AddHostedService<ProjectionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Happy Factory API v1");
        c.RoutePrefix = string.Empty; // serve UI at app root
    });
}

app.UseHttpsRedirection();

// FastEndpoints middleware (maps endpoints discovered in the app)
app.UseFastEndpoints();

app.Run();
