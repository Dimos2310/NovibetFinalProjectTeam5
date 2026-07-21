using Api.Middleware;
using Application;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Επίπεδο παρουσίασης
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Σύνθεση των layers - κάθε layer δηλώνει μόνο του τι παρέχει.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// First in the pipeline on purpose - if anything below throws (including routing or
// a controller action), this is what catches it and turns it into a proper response.
app.UseExceptionHandling();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
