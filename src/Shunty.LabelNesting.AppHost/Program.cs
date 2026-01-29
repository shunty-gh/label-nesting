var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject("web", @"..\Shunty.LabelNesting.Web\Shunty.LabelNesting.Web.csproj");

builder.Build().Run();
