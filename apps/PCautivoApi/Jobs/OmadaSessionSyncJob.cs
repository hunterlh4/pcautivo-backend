using MediatR;
using Quartz;
using PCautivoCore.Application.Features.Omada.Actions;

namespace PCautivoApi.Jobs;

[DisallowConcurrentExecution]
public class OmadaSessionSyncJob(ISender sender) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
       

        var result = await sender.Send(new SyncOmadaSessionsCommand(), context.CancellationToken);

        return ;
    }
}
