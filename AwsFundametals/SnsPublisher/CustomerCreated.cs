﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnsPublisher
{
	public class CustomerCreated
	{
		public required Guid ID { get; init; }

		public required string FullName { get; init; }

		public required string Email { get; init; }

		public required string GitHubUsername { get; init; }

		public required DateTime DateOfBirth { get; init; }
	}
}
