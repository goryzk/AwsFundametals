using Customers.SnsPublisher.Api.Contracts.Messages;
using Customers.SnsPublisher.Api.Domain;

namespace Customers.SnsPublisher.Api.Mapping
{
	public static class DomainToMessageMapper
	{
		public static CustomerCreated ToCustomerCreatedMessage(this Customer customer)
		{
			return new CustomerCreated
			{
				DateOfBirth = customer.DateOfBirth,
				FullName = customer.FullName,
				Email = customer.Email,
				GitHubUsername = customer.GitHubUsername,
				Id = customer.Id
			};
		}

		public static CustomerCreated ToCustomerUpdatedMessage(this Customer customer)
		{
			return new CustomerCreated
			{
				DateOfBirth = customer.DateOfBirth,
				FullName = customer.FullName,
				Email = customer.Email,
				GitHubUsername = customer.GitHubUsername,
				Id = customer.Id
			};
		}
	}
}
