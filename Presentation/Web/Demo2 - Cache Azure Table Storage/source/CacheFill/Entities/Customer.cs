using System;
using Microsoft.WindowsAzure.Storage.Table;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace CacheFill.Entities
{
	public class Customer : TableEntity
	{
		public Customer() { }

		public Customer(string company, string id)
		{
			PartitionKey = company;
			RowKey = id;
			Company = company;
			Id = id;
		}

		public string Id { get; set; }
		public string Company { get; set; }
		public string Name { get; set; }
		public double Value { get; set; }
		public string Comment { get; set; }
		public DateTime ContractDate { get; set; }
	}
}