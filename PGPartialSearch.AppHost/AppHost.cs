var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithImage("postgres")
    .WithImageTag("17-alpine")
    .WithPgAdmin();

var db = postgres.AddDatabase("searchdb");

builder.AddProject<Projects.PGPartialSearch_ApiService>("apiservice")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(db)
    .WaitFor(db);

builder.Build().Run();
