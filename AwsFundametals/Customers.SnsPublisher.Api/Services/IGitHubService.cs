namespace Customers.SnsPublisher.Api.Services;

public interface IGitHubService
{
    Task<bool> IsValidGitHubUser(string username);
}
