
using Microsoft.OpenApi.Models;
using TransformService.Services;
using TransformService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Transform Service API", Version = "v1" });
});
builder.Services.AddSingleton<GrammarTransformer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/api/transform/factorize", (Grammar grammar, GrammarTransformer transformer) =>
{
    var result = transformer.Factorize(grammar);
    return Results.Ok(result);
});

app.MapPost("/api/transform/eliminate-recursion", (Grammar grammar, GrammarTransformer transformer) =>
{
    var result = transformer.EliminateLeftRecursion(grammar);
    return Results.Ok(result);
});

app.Run();
