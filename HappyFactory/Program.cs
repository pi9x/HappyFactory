using FastEndpoints;
using FastEndpoints.Swagger;
using HappyFactory.Features.Products.Create;
using HappyFactory.Features.Products.Get;
using HappyFactory.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add FastEndpoints (vertical-slice-friendly)
builder.Services.AddFastEndpoints().SwaggerDocument();

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
    app.UseSwaggerGen();
}

app.UseHttpsRedirection();

// FastEndpoints middleware (maps endpoints discovered in the app)
app.UseFastEndpoints();

app.Run();
