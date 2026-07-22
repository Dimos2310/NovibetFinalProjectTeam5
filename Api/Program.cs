using Api.Middleware;
using Application;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Presentation
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Layer composition — each layer owns its own registrations.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// First in the pipeline on purpose - if anything below throws (including routing or
// a controller action), this is what catches it and turns it into a proper response.
app.UseExceptionHandling();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
