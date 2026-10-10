using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

var contentRoot = Directory.GetCurrentDirectory();
var candidateRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
if (!Directory.Exists(Path.Combine(contentRoot, "Pages")) && Directory.Exists(Path.Combine(candidateRoot, "Pages")))
{
    contentRoot = candidateRoot;
}

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = contentRoot
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<QMSSystem.Web.Services.ApiCookieForwardingHandler>();

Action<HttpClient> configureClient = client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5070/";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
};

builder.Services.AddHttpClient("QMSApi", configureClient)
    .AddHttpMessageHandler<QMSSystem.Web.Services.ApiCookieForwardingHandler>();

builder.Services.AddHttpClient("ApiClient", configureClient)
    .AddHttpMessageHandler<QMSSystem.Web.Services.ApiCookieForwardingHandler>();

builder.Services.AddHttpClient(string.Empty, configureClient)
    .AddHttpMessageHandler<QMSSystem.Web.Services.ApiCookieForwardingHandler>();

var dataProtectionKeysPath = builder.Configuration["DataProtection:KeysPath"]
    ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "QMSSystem",
        "DataProtectionKeys");
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services.AddDataProtection()
    .SetApplicationName("QMSSystem")
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "QMS.Auth";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = false;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("OperatorOnly", policy => policy.RequireRole("Operator"));
    options.AddPolicy("SupervisorOnly", policy => policy.RequireRole("Supervisor"));
    options.AddPolicy("DocumentReviewer", policy => policy.RequireRole("Admin", "Supervisor"));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/AccessDenied");
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Operator", "OperatorOnly");
    options.Conventions.AuthorizeFolder("/Supervisor", "SupervisorOnly");
    options.Conventions.AuthorizePage("/Documents/DocumentApproval", "DocumentReviewer");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.Run();
