using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

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
});

builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5070");
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
}).AddHttpMessageHandler<QMSSystem.Web.Services.BearerTokenForwardingHandler>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<QMSSystem.Web.Services.BearerTokenForwardingHandler>();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/AccessDenied");
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Operator", "OperatorOnly");
    options.Conventions.AuthorizeFolder("/Supervisor", "SupervisorOnly");
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
