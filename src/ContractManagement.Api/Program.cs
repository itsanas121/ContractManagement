using ContractManagement.Api.ExceptionHandling;
using ContractManagement.Core.Domain.Exceptions;
using ContractManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(); //unused to handle UserExceptionHandler() error


builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // /swagger
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/test-error", () =>
{
    throw new Exception("This is a test exception.");
});

app.MapGet("/test-error-domain-error", () =>
{
    throw new DomainValidationException(new[]
    {
        "Error A: Invalid contract number.",
        "Error B: Contract date cannot be in the future."
    });
});

app.Run();
