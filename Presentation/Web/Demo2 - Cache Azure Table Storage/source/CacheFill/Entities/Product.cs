using Microsoft.WindowsAzure.Storage.Table;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace CacheFill.Entities
{
	public class Product : TableEntity
	{
		public Product() { }

		public Product(string manufactor, string id)
		{
			PartitionKey = manufactor;
			RowKey = id;
			Id = id;
			Manufactor = manufactor;
		}

		public string Id { get; set; }
		public string Name { get; set; }
		public string Manufactor { get; set; }
		public double Price { get; set; }
		public string Category { get; set; }
		public int Rate { get; set; }
	}
}