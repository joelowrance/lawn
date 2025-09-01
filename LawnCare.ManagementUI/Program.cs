using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Auth0.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();

// Configure Auth0
builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"]!;
    options.ClientId = builder.Configuration["Auth0:ClientId"]!;
    options.ClientSecret = builder.Configuration["Auth0:ClientSecret"]!;
    //options.Audience = builder.Configuration["Auth0:Audience"];
});

// Add authorization
builder.Services.AddAuthorization();

// Add HTTP context accessor for authentication state
builder.Services.AddHttpContextAccessor();

// Add custom authentication state provider
builder.Services.AddScoped<LawnCare.ManagementUI.Shared.CustomAuthenticationStateProvider>();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<LawnCare.ManagementUI.Shared.CustomAuthenticationStateProvider>());

// Add application services
builder.Services.AddScoped<LawnCare.ManagementUI.Services.ISchedulingService, LawnCare.ManagementUI.Services.SchedulingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapControllers();
app.MapFallbackToPage("/_Host");

app.Run();
