var builder = DistributedApplication.CreateBuilder(args);

var db = builder
    .AddSqlServer("dbserver")
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("bd");

builder.AddProject<Projects.Api>("api").WithExternalHttpEndpoints().WithReference(db).WaitFor(db);

builder.Build().Run();
