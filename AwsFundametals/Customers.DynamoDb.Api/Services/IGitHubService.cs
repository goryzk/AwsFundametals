namespace Customers.DynamoDb.Api.Services;

public interface IGitHubService
{
    Task<bool> IsValidGitHubUser(string username);
}
