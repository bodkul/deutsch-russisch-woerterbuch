var builder = DistributedApplication.CreateBuilder(args);

// Dictionary import: started manually with the ▶ button in the dashboard (needed once).
builder.AddProject<Projects.Woerterbuch_Importer>("importer")
    .WithExplicitStart();

var api = builder.AddProject<Projects.Woerterbuch_Api>("api")
    .WithHttpHealthCheck("/health");

// Vite gets the API address via service discovery (services__api__*) and proxies /api.
builder.AddViteApp("frontend", "../../frontend")
    .WithNpm(install: true)
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
