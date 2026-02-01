var builder = DistributedApplication.CreateBuilder(args);

// Configure the web project to trust the developer certificate for dashboard communication
builder.AddProject("web", @"..\Shunty.LabelNesting.Web\Shunty.LabelNesting.Web.csproj")
    .WithDeveloperCertificateTrust(trust: true);

builder.Build().Run();
