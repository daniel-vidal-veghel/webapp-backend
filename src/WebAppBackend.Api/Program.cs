using WebAppBackend.Api.DataAccess;
using WebAppBackend.Api.Services;
using WebAppBackend.Api.Validation;

var builder = WebApplication.CreateBuilder(args);

// Check appsettings.json for ContentSettings.
ContentPaths.ValidatePathSettings(builder.Configuration);

// ---------------------------------------------------------------------
// Services
// ---------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// The content service re-reads the XML file from disk on every call
builder.Services.AddScoped<IDataAccess, DataAccess>();
builder.Services.AddScoped<IContentService, XmlContentService>();
builder.Services.AddScoped<IContentValidator, ContentValidator>();

// Allow the Angular dev server (ng serve, default port 4200) to call
// this API while developing on Windows 10. Tighten this for production.
const string AngularDevCorsPolicy = "AngularDevCorsPolicy";
builder.Services.AddCors(options =>
{
	options.AddPolicy(AngularDevCorsPolicy, policy =>
	{
		policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
			  .AllowAnyHeader()
			  .AllowAnyMethod();
	});
});

var app = builder.Build();

// ---------------------------------------------------------------------
// Content validation
// ---------------------------------------------------------------------
using (var startupScope = app.Services.CreateScope())
{
	var contentService = startupScope.ServiceProvider.GetRequiredService<IContentService>();
	if (!contentService.InitValidation(out string? errorMessage))
	{
		throw new Exception(errorMessage);
	}
}

// ---------------------------------------------------------------------
// Middleware pipeline
// ---------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(AngularDevCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();