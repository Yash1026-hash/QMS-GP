using System.IO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;
using QMSSystem.Api.Services;

var builder = WebApplication.CreateBuilder(args);


// =====================================================
// CONTROLLERS
// =====================================================

builder.Services.AddControllers();


// =====================================================
// SWAGGER
// =====================================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// =====================================================
// DATA PROTECTION
// =====================================================

var dataProtectionKeysPath =
    builder.Configuration["DataProtection:KeysPath"]
    ?? Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData),
        "QMSSystem",
        "DataProtectionKeys");

Directory.CreateDirectory(dataProtectionKeysPath);

builder.Services
    .AddDataProtection()
    .SetApplicationName("QMSSystem")
    .PersistKeysToFileSystem(
        new DirectoryInfo(dataProtectionKeysPath));


// =====================================================
// AUTHENTICATION
// =====================================================

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "QMS.Auth";

        options.Cookie.HttpOnly = true;

        options.Cookie.SameSite = SameSiteMode.Lax;

        options.Cookie.SecurePolicy =
            CookieSecurePolicy.SameAsRequest;

        options.ExpireTimeSpan =
            TimeSpan.FromHours(8);


        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            return Task.CompletedTask;
        };


        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode =
                StatusCodes.Status403Forbidden;

            return Task.CompletedTask;
        };
    });


// =====================================================
// AUTHORIZATION
// =====================================================

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "AdminOnly",
        policy =>
            policy.RequireRole("Admin"));

    options.AddPolicy(
        "OperatorOnly",
        policy =>
            policy.RequireRole("Operator"));

    options.AddPolicy(
        "SupervisorOnly",
        policy =>
            policy.RequireRole("Supervisor"));


    options.FallbackPolicy =
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
});


// =====================================================
// DATABASE
// =====================================================

var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection")
    ?? throw new InvalidOperationException(
        "Configure ConnectionStrings:DefaultConnection through user secrets or an environment variable.");

builder.Services.AddDbContext<UserDbContext>(
    options =>
        options.UseSqlServer(connectionString));


// =====================================================
// SERVICES
// =====================================================

builder.Services.AddScoped<UserStore>();

builder.Services.AddScoped<
    IPasswordHasher<QMSSystem.Shared.Models.UserAccount>,
    PasswordHasher<QMSSystem.Shared.Models.UserAccount>
>();


// =====================================================
// CORS
// =====================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "FrontendPolicy",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:5048")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


// =====================================================
// BUILD
// =====================================================

var app = builder.Build();


// =====================================================
// SWAGGER
// =====================================================

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "QMS System API V1");

    c.RoutePrefix = "swagger";
});


// =====================================================
// CORS
// =====================================================

app.UseCors("FrontendPolicy");


// =====================================================
// EXCEPTION HANDLING
// =====================================================

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception =
            context.Features
                .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()
                ?.Error;

        context.Response.StatusCode =
            StatusCodes.Status500InternalServerError;

        context.Response.ContentType =
            "application/json";

        await context.Response.WriteAsJsonAsync(
            new
            {
                message =
                    "An unexpected server error occurred.",

                details =
                    exception?.Message
            });
    });
});


// =====================================================
// AUTHENTICATION
// =====================================================

app.UseAuthentication();


// =====================================================
// AUTHORIZATION
// =====================================================

app.UseAuthorization();


// =====================================================
// CONTROLLERS
// =====================================================

app.MapControllers();


// =====================================================
// RUN
// =====================================================

app.Run();