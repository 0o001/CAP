using System;
using System.Collections.Generic;
using DotNetCore.CAP.Dashboard.Monitoring;

namespace DotNetCore.CAP.Dashboard
{
    public interface IMonitoringApi
    {
        IList<QueueWithTopEnqueuedJobsDto> Queues();
        IList<ServerDto> Servers();
        JobDetailsDto JobDetails(string jobId);
        StatisticsDto GetStatistics();

        JobList<EnqueuedJobDto> EnqueuedJobs(string queue, int from, int perPage);
        JobList<FetchedJobDto> FetchedJobs(string queue, int from, int perPage);

        JobList<ProcessingJobDto> ProcessingJobs(int from, int count);
        JobList<ScheduledJobDto> ScheduledJobs(int from, int count);
        JobList<SucceededJobDto> SucceededJobs(int from, int count);
        JobList<FailedJobDto> FailedJobs(int from, int count);
        JobList<DeletedJobDto> DeletedJobs(int from, int count);

        long ScheduledCount();
        long EnqueuedCount(string queue);
        long FetchedCount(string queue);
        long FailedCount();
        long ProcessingCount();

        long SucceededListCount();
        long DeletedListCount();
        
        IDictionary<DateTime, long> SucceededByDatesCount();
        IDictionary<DateTime, long> FailedByDatesCount();
        IDictionary<DateTime, long> HourlySucceededJobs();
        IDictionary<DateTime, long> HourlyFailedJobs();
    }
}