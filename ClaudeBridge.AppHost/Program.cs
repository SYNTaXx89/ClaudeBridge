var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.ClaudeBridge_ApiService>("apiservice")
    .WithExternalHttpEndpoints();

builder.Build().Run();