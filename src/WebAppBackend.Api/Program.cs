using WebAppBackend.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// Services
// ---------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// The content service re-reads the XML file from disk on every call
// (see XmlContentService), so it is safe and cheap to register as a
// transient / scoped service. Scoped is used here since it is consumed
// once per HTTP request.
builder.Services.AddScoped<IContentService, XmlContentService>();

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
