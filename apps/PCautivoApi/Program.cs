using PCautivoApi.Modules;
using PCautivoApi.Shared.Settings;
using PCautivoCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

var hostSetting = configuration.GetSection("Host").Get<HostSetting>() ?? throw new Exception("Failed to get HostSetting");

builder.Services.AddInfrastructure(configuration);
builder.Services.AddApplication();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor(); // Para acceder al HttpContext en los handlers

builder.Services.AddAuthentication(configuration);

builder.Services.AddCors(o =>
{
    o.AddPolicy(hostSetting.PolicyName, policy =>
    {
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.AllowAnyOrigin();
    });
});

builder.Services.AddSwagger();

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = long.MaxValue;
});

builder.WebHost.UseUrls(hostSetting.WebHostUrl);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI(options =>
    //{
    //    options.DefaultModelsExpandDepth(-1);
    //});
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.DefaultModelsExpandDepth(-1);
});


app.UseCors(hostSetting.PolicyName);
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();