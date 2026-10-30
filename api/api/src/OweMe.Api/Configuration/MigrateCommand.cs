using JasperFx.CommandLine;
using Microsoft.EntityFrameworkCore;
using OweMe.Persistence.Data;

namespace OweMe.Api.Configuration;

[Description("Apply pending EF Core migrations and exit")]
public class MigrateCommand : JasperFxAsyncCommand<NetCoreInput>
{
    public override async Task<bool> Execute(NetCoreInput input)
    {
        // The host is built but never started: no Kestrel, no Wolverine, no ValidateOnStart.
        using var host = input.BuildHost();
        await using var scope = host.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database;
        database.SetCommandTimeout(TimeSpan.FromMinutes(10)); // default 30s per statement is too short for backfills on large tables
        await database.MigrateAsync();
        return true;
    }
}
