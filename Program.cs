using Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}

builder.WebHost.UseKestrel(options =>
{
    // Since my 5000 port on the server is occupied, I switch it to port 6000 in production.
    var port = builder.Environment.IsDevelopment() ? 5000 : 6000;
    options.ListenAnyIP(port);
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<Api.Filters.ValidationFilter>();
});
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddFluentValidation();
builder.Services.AddSignalRConfiguration();
builder.Services.AddCorsPolicy();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerConfiguration();

var app = builder.Build();

app.ConfigureMiddleware();
app.ConfigureEndpoints();

app.Run();
