using Microsoft.AspNetCore.HttpOverrides;
using Shunty.LabelNesting.Core.Extensions;
using Shunty.LabelNesting.Web.Components;
using Shunty.LabelNesting.Web.HealthChecks;
using Shunty.LabelNesting.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults for Aspire
builder.AddServiceDefaults();

// Add custom health checks
builder.Services.AddHealthChecks()
    .AddCheck<LabelNestingCoreHealthCheck>("label_nesting_core", tags: ["ready"]);

// Add Blazor services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add label nesting services
builder.Services.AddLabelNestingCore();
builder.Services.AddScoped<NestingStateService>();

// Configure forwarded headers for reverse proxy support
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Allow any proxy - this is necessary for Docker/Kubernetes deployments
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Use forwarded headers before other middleware
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapStaticAssets();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();

// Make Program class accessible for testing
public partial class Program { }
