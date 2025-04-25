using Hangfire;
using Inventio.Jobs.NonConformance;

public static class HangfireWorker
{
    public static void StartRecurringJobs(this IApplicationBuilder app)
    {
        RecurringJob.AddOrUpdate<WeeklyJob>("non-conformance-weekly-job", x => x.ExecuteAsync(), Cron.Hourly);
    }
}
