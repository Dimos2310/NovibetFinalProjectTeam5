namespace Application.BackgroundJobs;

public interface IIpRefreshService
{
    Task RefreshAllAsync(CancellationToken cancellationToken = default);
}
