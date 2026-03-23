using PCautivoApi.Modules;
using PCautivoApi.Shared.Settings;
using PCautivoApi.Jobs;
using PCautivoCore;
using PCautivoCore.Infrastructure.Settings;
using Quartz;
using System.Globalization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

var hostSetting = configuration.GetSection("Host").Get<HostSetting>() ?? throw new Exception("Failed to get HostSetting");
var omadaSyncJob = configuration.GetSection("OmadaSyncJob").Get<OmadaSyncJobSettings>() ?? new OmadaSyncJobSettings();

if (!string.IsNullOrWhiteSpace(hostSetting.AllowedHosts))
{
    configuration["AllowedHosts"] = hostSetting.AllowedHosts;
}

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

builder.Services.AddQuartz(q =>
{
    if (!omadaSyncJob.Enabled)
    {
        return;
    }

    var timeZone = ResolveTimeZone(omadaSyncJob.TimeZoneId);
    var jobKey = new JobKey("OmadaSessionSyncJob");
    var runSchedule = BuildRunSchedule(omadaSyncJob, timeZone);

    q.AddJob<OmadaSessionSyncJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("OmadaSessionSyncJob-trigger")
        .WithSchedule(runSchedule));
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

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

static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
{
    try
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
        }

        return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
    }
    catch
    {
        return TimeZoneInfo.Utc;
    }
}

static CronScheduleBuilder BuildRunSchedule(OmadaSyncJobSettings settings, TimeZoneInfo timeZone)
{
    if (LooksLikeCron(settings.StartJob))
    {
        return CronScheduleBuilder.CronSchedule(settings.StartJob).InTimeZone(timeZone);
    }

    var runTime = ParseRunTime(settings.StartJob);
    return CronScheduleBuilder.DailyAtHourAndMinute(runTime.Hour, runTime.Minute).InTimeZone(timeZone);
}

static bool LooksLikeCron(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return false;
    }

    var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    return parts.Length is 6 or 7;
}

static TimeOnly ParseRunTime(string? runTime)
{
    if (TimeOnly.TryParseExact(runTime, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
    {
        return parsed;
    }

    return new TimeOnly(7, 0);
}