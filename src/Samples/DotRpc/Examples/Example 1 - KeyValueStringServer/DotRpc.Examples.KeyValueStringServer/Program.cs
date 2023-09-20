using DotRpc;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDotRpc();
var services = builder.Services;

// register your services
services.AddSingleton<IKeyValueStore, KeyValueStore>();

services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.SchemaGeneratorOptions.SchemaIdSelector = x => x.DotRpcSwaggerSchemaIdGenerator();
});

var app = builder.Build();


app.AddDotRpcFromAssembly(typeof(Program).Assembly);
app.UseSwagger();
app.UseSwaggerUI();

app.Run();


