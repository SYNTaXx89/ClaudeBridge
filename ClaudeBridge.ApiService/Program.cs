using Microsoft.OpenApi.Models;
using System.Reflection;
using ClaudeBridge.ApiService.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Claude OpenAI Bridge API",
        Version = "v1",
        Description = "A bridge API that converts OpenAI API requests to Claude API format"
    });

    // Set the comments path for the Swagger JSON and UI
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddHttpClient<ClaudeApiService>();
builder.Services.AddScoped<TransformationService>();
builder.Services.AddScoped<IOpenAIEndpointsHandler, OpenAIEndpointsHandler>();
builder.Services.AddSingleton<IServiceUrlProvider, AspireServiceUrlProvider>();
builder.Services.AddLogging();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Add request/response logging middleware
app.UseRequestResponseLogging();

app.MapPost("/v1/chat/completions", OpenAIEndpoints.ChatCompletions)
    .WithName("ChatCompletions");

app.Run();