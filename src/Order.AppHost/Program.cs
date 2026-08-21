using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .AddDatabase("orderdb");
var redis = builder.AddRedis("redis")
    .WithDataVolume();

builder.AddProject("webapi", "..\\Order.WebApi\\Order.WebApi.csproj")
    .WithReference(postgres)
    .WithReference(redis)
    .WaitFor(postgres)
    .WaitFor(redis);

builder.Build().Run();
