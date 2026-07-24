using Api.Middleware;
using Application;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Επίπεδο παρουσίασης
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Σύνθεση των layers - κάθε layer δηλώνει μόνο του τι παρέχει.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// First in the pipeline on purpose - if anything below throws (including routing or
// a controller action), this is what catches it and turns it into a proper response.
app.UseExceptionHandling();

// Swagger only in Development - no reason to expose the API's shape publicly once deployed,
// and this is what makes /swagger actually the useful landing page instead of a 404.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
