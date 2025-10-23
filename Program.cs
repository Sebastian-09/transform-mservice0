using Microsoft.OpenApi.Models;
using TransformService.Services;

using Microsoft.AspNetCore.Mvc;

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

app.MapPost("/api/transform/factorizar", ([FromBody] GrammarRequest request) =>
{
    var transformer = new GrammarTransformer();
    var result = transformer.Factorize(request.Grammar);
    return Results.Ok(result);
});

app.MapPost("/api/transform/eliminate-left-recursion", (Dictionary<string, List<string>> grammar, GrammarTransformer transformer) =>
{
    var result = transformer.EliminateLeftRecursion(grammar);
    return Results.Ok(result);
});

app.MapPost("/api/transform/step-by-step", (Dictionary<string, List<string>> grammar, GrammarTransformer transformer) =>
{
    var result = transformer.TransformStepByStep(grammar);
    return Results.Ok(result);
});

app.Run("http://localhost:5085");
