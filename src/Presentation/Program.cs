using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Clerk.Net.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddClerkApiClient(config =>
{
    config.SecretKey = builder.Configuration["Clerk:Secret"]!;
});
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "Clerk";
    })
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    })
    .AddOAuth(
        "Clerk",
        options =>
        {
            var clerkDomain = builder.Configuration["Clerk:Domain"];
            options.AuthorizationEndpoint = $"{clerkDomain}/oauth/authorize";
            options.Scope.Add("profile");
            options.Scope.Add("email");
            options.CallbackPath = new PathString("/oauth/callback");
            options.ClientId = builder.Configuration["Clerk:ClientId"]!;
            options.ClientSecret = builder.Configuration["Clerk:ClientSecret"]!;
            options.TokenEndpoint = $"{clerkDomain}/oauth/token";
            options.UserInformationEndpoint = $"{clerkDomain}/oauth/userinfo";
            options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "user_id");
            options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
            options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
            options.Events = new OAuthEvents
            {
                OnCreatingTicket = async context =>
                {
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        context.Options.UserInformationEndpoint
                    );
                    request.Headers.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json")
                    );
                    request.Headers.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        context.AccessToken
                    );
                    var response = await context.Backchannel.SendAsync(
                        request,
                        HttpCompletionOption.ResponseHeadersRead,
                        context.HttpContext.RequestAborted
                    );
                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadAsStringAsync();
                    context.RunClaimActions(JsonDocument.Parse(json).RootElement);
                },
            };
        }
    );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
