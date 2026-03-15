using EmployeeContacts.Api.OpenApi;
using EmployeeContacts.Api.ProblemDetails;
using EmployeeContacts.Api.Services;
using EmployeeContacts.Application.DependencyInjection;
using EmployeeContacts.Infrastructure.DependencyInjection;
using EmployeeContacts.Infrastructure.Persistence;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<EmployeeService>();
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            ValidationProblemDetails validationProblemDetails = new(context.ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
                Type = "https://httpstatuses.com/400"
            };

            validationProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            return new BadRequestObjectResult(validationProblemDetails);
        };
    });

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields =
        HttpLoggingFields.RequestMethod |
        HttpLoggingFields.RequestPath |
        HttpLoggingFields.ResponseStatusCode |
        HttpLoggingFields.Duration;
});
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("EmployeeContacts.Api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Employee Contacts API",
        Version = "v1",
        Description = "직원 비상 연락망 조회 및 일괄 등록 API"
    });

    string apiXmlPath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(apiXmlPath))
    {
        options.IncludeXmlComments(apiXmlPath, includeControllerXmlComments: true);
    }

    string applicationXmlPath = Path.Combine(AppContext.BaseDirectory, "EmployeeContacts.Application.xml");
    if (File.Exists(applicationXmlPath))
    {
        options.IncludeXmlComments(applicationXmlPath);
    }

    options.OperationFilter<EmployeeImportOperationFilter>();
});

WebApplication app = builder.Build();

// 데이터베이스 마이그레이션 및 테스트 데이터 자동 생성
using (IServiceScope scope = app.Services.CreateScope())
{
    AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.UseExceptionHandler();
app.UseHttpLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

string programVersion = builder.Configuration["ProgramVersion"] ?? "unknown";
app.MapGet("/", () => Results.Ok(new { version = programVersion, status = "ok" }))
    .WithName("Health")
    .WithOpenApi()
    .Produces<object>(StatusCodes.Status200OK);

app.MapControllers();
app.Run();

public partial class Program
{
}
