using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;
using QMSSystem.Api.Services;
using QMSSystem.Api.Services.Workflow;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Configure ConnectionStrings:DefaultConnection through user secrets or an environment variable.");
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped<UserStore>();
builder.Services.AddScoped<UserStore>();
builder.Services.AddScoped<IPasswordHasher<QMSSystem.Shared.Models.UserAccount>, PasswordHasher<QMSSystem.Shared.Models.UserAccount>>();
builder.Services.AddQmsWorkflow(connectionString);

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5048")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Local databases only: fill empty QMS tables with one record per workflow step.
// Turn on with user secrets: Qms:SeedDemoData = true and Qms:DemoUsers:* = existing user ids.
if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("Qms:SeedDemoData"))
{
    var demoUsers = app.Configuration.GetSection("Qms:DemoUsers").Get<DemoUserIds>()
        ?? throw new InvalidOperationException("Set Qms:DemoUsers (Operator, SecondOperator, Supervisor, SecondSupervisor).");
    using var scope = app.Services.CreateScope();
    await DemoDataSeeder.SeedAsync(
        scope.ServiceProvider.GetRequiredService<QmsDbContext>(),
        demoUsers,
        DateTime.UtcNow);
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Recall Operations API V1");
    c.RoutePrefix = "swagger";
});

app.UseCors("FrontendPolicy");
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

        // A broken workflow rule is the user's mistake, not a server error:
        // return 409 with the rule's message so the page can show it.
        if (exception is WorkflowException workflowException)
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { message = workflowException.Message });
            return;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            message = "An unexpected server error occurred.",
            details = exception?.Message
        });
    });
});

app.MapControllers();

app.Run();