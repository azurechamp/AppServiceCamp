using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BookSleeve;
using CacheFill.Entities;
using CacheFill.TaskRunnerHelper;
using HelperLib;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace CacheFill
{
	public static class Program
	{
		static readonly CloudStorageAccount CloudStorageAccount = DemoSettings.Storage.ConnectionString.ToCloudStorageAccount();

		private static readonly object BatchLock = new object();
		private const int Batchsize = 100;

		public static void Main()
		{
			var log = Console.Out;

			GenerateCustomerRecords(log);
			PrimeCustomerCache(log);
			GenerateProuctRecords(log);
			PrimeProuctCache(log);
		}

		private static void GenerateCustomerRecords(TextWriter log)
		{
			if (!ConfirmOperation("YOUR STORAGE TABLE WILL BE RECREATED! ALL EXISTING DATA WILL BE LOST! Are you sure?", log))
				return;

			var cloudTableClient = CloudStorageAccount.CreateCloudTableClient();
			var cloudTable = cloudTableClient.GetTableReference(DemoSettings.Storage.CustomerTableName);

			if (cloudTable.DeleteIfExists())
			{
				log.WriteLine("DELETING PREEXISTING TABLE '{0}'", cloudTable.Name);
			}
			while (true)
			{
				try
				{
					cloudTable.CreateIfNotExists();
					break;
				}
				catch
				{
					Task.Delay(TimeSpan.FromSeconds(5));
				}
				log.WriteLine("RETRY TABLE CREATION");
			}

			int count = 1;
			const int totalRecords = 1000000;
			string[] firstNames = File.ReadAllLines("CSV_Database_of_First_Names.csv");
			string[] lastNames = File.ReadAllLines("CSV_Database_of_Last_Names.csv");
			var rand = new Random(DateTime.UtcNow.Millisecond);
			var batches = new Dictionary<string, TableBatchOperation>();

			var tasks = new List<Task>();

			while (count <= totalRecords)
			{
				count++;
				var company = "Company " + rand.Next(1, 11);
				lock (BatchLock)
				{
					if (!batches.ContainsKey(company))
					{
						batches.Add(company, new TableBatchOperation());
					}
				}

				TableBatchOperation tableBatchOperation;

				lock (BatchLock)
				{
					var customer = new Customer(company, count.ToString(CultureInfo.InvariantCulture))
					{
						Value = (rand.NextDouble() - 0.5) * 99999.0,
						ContractDate = DateTime.Now,
						Name = firstNames[rand.Next(0, firstNames.Length)] + " " + lastNames[rand.Next(0, lastNames.Length)]
					};

					tableBatchOperation = batches[company];

					tableBatchOperation.Insert(customer);

					if (tableBatchOperation.Count < Batchsize) continue;

					batches[company] = new TableBatchOperation();
				}

				tasks.Add(new Task(() => cloudTable.ExecuteBatch(tableBatchOperation)));
			}

			var remainingTasks = batches
				.Where(keyValuePair => keyValuePair.Value.Count > 0)
				.Select(keyValuePair => new Task(() => cloudTable.ExecuteBatch(keyValuePair.Value)));
			tasks.AddRange(remainingTasks);

			// No tasks are running before this line. The TaskRunner will throttle to a specific # tasks
			const int tasksInParallel = 10;
			var taskRunner = new TaskRunner(tasks, tasksInParallel);
			taskRunner.TaskCompleted += (o, e) => log.WriteLine("Tasks running: {0}. Average time: {1}. Tasks completed {2} . Total Time {3}.",
				e.TasksInParallel,
				CalculateAverage(e.TaskTimeTotal, e.TasksCompleted).ToString("g"),
				e.TasksCompleted,
				e.TaskTimeTotal.ToString("g"));
			// Run all tasks
			taskRunner.WaitAll();
		}

		private static void GenerateProuctRecords(TextWriter log)
		{
			if (!ConfirmOperation("YOUR STORAGE TABLE WILL BE RECREATED! ALL EXISTING DATA WILL BE LOST! Are you sure?", log))
				return;

			var cloudTableClient = CloudStorageAccount.CreateCloudTableClient();
			var cloudTable = cloudTableClient.GetTableReference(DemoSettings.Storage.ProductsTableName);
			if (cloudTable.DeleteIfExists())
			{
				log.WriteLine("DELETING PREEXISTING TABLE '{0}'", cloudTable.Name);
			}
			while (true)
			{
				try
				{
					cloudTable.CreateIfNotExists();
					break;
				}
				catch
				{
					Task.Delay(TimeSpan.FromSeconds(5));
				}
				log.WriteLine("RETRY TABLE CREATION");
			}
			int count = 0;
			const int totalRecords = 1000000;
			string[] categories = File.ReadAllLines("CSV_Database_of_Categories.csv");
			string[] products = File.ReadAllLines("CSV_Database_of_Products.csv");
			string[] prefixes = { "", "Super ", "Ultimate ", "New ", "", "", "" };
			string[] postfixes = { "", " Mini", " Pro", " Standard", " Lite", " Enterprise", " One", " 2", " X", " Zero", " 3", " III", " IV", "", "", "" };
			var random = new Random(DateTime.UtcNow.Millisecond);
			var batches = new Dictionary<string, TableBatchOperation>();
			var topRated = new Dictionary<string, Tuple<int, int>>();
			while (count < totalRecords)
			{
				count++;
				var category = categories[random.Next(0, categories.Length)];
				if (!batches.ContainsKey(category))
				{
					batches.Add(category, new TableBatchOperation());
				}

				if (!topRated.ContainsKey(category))
				{
					topRated.Add(category, new Tuple<int, int>(0, random.Next(4, 11)));
				}

				var product = new Product(category, count.ToString(CultureInfo.InvariantCulture))
				{
					Price = (random.NextDouble() + 0.1) * 99999.0,
					Name = prefixes[random.Next(0, prefixes.Length)] +
								 products[random.Next(0, products.Length)] +
								 postfixes[random.Next(0, postfixes.Length)],
					Category = category,
				};
				int rate = random.Next(1, 11);
				if (rate == 10)
				{
					if (topRated[category].Item1 < topRated[category].Item2)
					{
						topRated[category] = new Tuple<int, int>(topRated[category].Item1 + 1, topRated[category].Item2);
						var color = Console.ForegroundColor;
						Console.ForegroundColor = ConsoleColor.Cyan;
						log.WriteLine("\n\n\n************\n* TOP RANK!*\n************\n\n\n");
						Console.ForegroundColor = color;
					}
					else
						rate = random.Next(1, 10);
				}
				product.Rate = rate;
				log.WriteLine(count + " " + product.Name + " " + product.Rate + (product.Rate == 10 ? "<----------------" : ""));
				batches[category].Insert(product);
				if (batches[category].Count() >= Batchsize)
				{
					log.WriteLine("Committing " + batches[category].Count + " recrods to " + category);
					cloudTable.ExecuteBatch(batches[category]);
					batches[category].Clear();
				}
			}

			foreach (var batch in batches.Values.Where(batch => batch.Count > 0))
			{
				log.WriteLine("Committing " + batch.Count + " records...");
				cloudTable.ExecuteBatch(batch);
			}
		}

		private static void PrimeCustomerCache(TextWriter log)
		{
			if (!ConfirmOperation("YOUR CACHE WILL BE FLUSHED! ALL EXISTING INDEXES WILL BE REPLACED! Are you sure?", log))
				return;

			var client = CloudStorageAccount.CreateCloudTableClient();
			var table = client.GetTableReference(DemoSettings.Storage.CustomerTableName);
			var query = from customer in table.CreateQuery<Customer>() select customer;

			var connection = new RedisConnection(DemoSettings.CustomerRedisCache.Url, allowAdmin: true, password: DemoSettings.CustomerRedisCache.Password);
			connection.Open();
			connection.Server.FlushDb(0);
			foreach (var c in query)
			{
				string key = string.Format("{0}:{1}", c.PartitionKey, c.RowKey);
				log.WriteLine("{0}:{1}", key, c.Name);
				connection.SortedSets.Add(0, "customervalues", key, c.Value);
				connection.Strings.Set(0, "cust:" + c.Name.Replace(' ', ':'), key);
			}
			connection.Close(false);
		}

		private static void PrimeProuctCache(TextWriter log)
		{
			if (!ConfirmOperation("YOUR CACHE WILL BE FLUSHED! ALL EXISTING INDEXES WILL BE REPLACED! Are you sure?", log))
				return;

			var client = CloudStorageAccount.CreateCloudTableClient();
			var table = client.GetTableReference(DemoSettings.Storage.ProductsTableName);
			var query = from product in table.CreateQuery<Product>() select product;

			var connection = new RedisConnection(DemoSettings.ProductsRedisCache.Url, allowAdmin: true, password: DemoSettings.ProductsRedisCache.Password);
			connection.Open();
			connection.Server.FlushDb(0);
			foreach (var c in query)
			{
				string key = string.Format("{0}:{1}:{2}:{3}:{4}", c.PartitionKey, c.RowKey, c.Name, c.Price, c.Rate);
				log.WriteLine("{0}:{1}", key, c.Name);
				connection.SortedSets.Add(0, "cat:" + c.Category, key, c.Price);
				connection.SortedSets.Add(0, "rate:" + c.Category, key, c.Rate);
				connection.Strings.Set(0, "prod:" + c.Category + ":" + c.Name.Replace(' ', ':'), key);
			}
			connection.Close(false);
		}

		private static bool ConfirmOperation(string message, TextWriter log)
		{
			var color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
			log.Write("{1}{1}{1}{0}{1}{1}{1}Please confirm (Y/N):", message, Environment.NewLine);
			Console.ForegroundColor = color;
			var input = Console.ReadLine();
			return !string.IsNullOrWhiteSpace(input) && input.ToLower() == "y";
		}

		private static TimeSpan CalculateAverage(TimeSpan taskTimeTotal, int tasksCompleted)
		{
			return tasksCompleted == 0 ? TimeSpan.Zero : TimeSpan.FromTicks(taskTimeTotal.Ticks / tasksCompleted);
		}
	}
}