using Application;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Shared;
using InternetBankingApi.Extensions;
using InternetBankingApi.Handlers;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(opt =>
{
    opt.Filters.Add(new ProducesAttribute("application/json"));
}).ConfigureApiBehaviorOptions(opt =>
{
    opt.SuppressInferBindingSourcesForParameters = true;
    opt.SuppressMapClientErrors = true;
}).AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.PersistenceLayerIoc(builder.Configuration);
builder.Services.ApplicationLayerIoc();
builder.Services.SharedLayerIoc(builder.Configuration);
builder.Services.AddIdentityLayerIocForWebApi(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AppiVersioningExtension();
builder.Services.SwaggerExtension();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();       

var app = builder.Build();
await app.Services.RunIdentitySeedAsync();

// Configure the HTTP request pipeline.
app.UseSwaggerExtension(app);
app.MapOpenApi();

app.UseHttpsRedirection();
app.UseExceptionHandler();

app.UseAuthorization();
app.UseHealthChecks("/health");

app.MapControllers();

await app.RunAsync();