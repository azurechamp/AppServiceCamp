using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using BookSleeve;
using CustomerQuery.Models;
using HelperLib;
using Microsoft.WindowsAzure.Storage;
using StackExchange.Redis;

namespace CustomerQuery.Controllers
{
	public class HomeController : Controller
	{
		readonly CloudStorageAccount cloudStorageAccount = DemoSettings.Storage.ConnectionString.ToCloudStorageAccount();

		public ActionResult Index()
		{
			return View(new HomeViewModel());
		}
		public ActionResult Products()
		{
			return View(new HomeViewModel());
		}
		public ActionResult SearchCustomers(HomeViewModel data)
		{
			var client = cloudStorageAccount.CreateCloudTableClient();
			var table = client.GetTableReference(DemoSettings.Storage.CustomerTableName);
			data.MatchedCustomers.Clear();
			var watch = new Stopwatch();

			if (data.UseTable)
			{
				watch.Start();

				foreach (var customer in table.CreateQuery<Customer>().Where(c => data.SearchString == c.Name))
				{
					customer.Value = (int)(customer.Value * 100) / 100.0;
					data.TableCustomers.Add(customer);
				}
				watch.Stop();
				data.TableResponseTime = watch.ElapsedMilliseconds;
			}

			var connection = ConnectionMultiplexer.Connect(new ConfigurationOptions
			{
				EndPoints = { { DemoSettings.CustomerRedisCache.Url, DemoSettings.CustomerRedisCache.Port } },
				Password = DemoSettings.CustomerRedisCache.Password
			});

			var db = connection.GetDatabase();
			watch.Restart();

			var record = db.StringGet("cust:" + data.SearchString.Replace(' ', ':'));
			if (!record.IsNullOrEmpty)
			{
				string[] parts = Encoding.ASCII.GetString(record).Split(':');
				if (parts.Length == 2)
				{
					foreach (var customer in table.CreateQuery<Customer>().Where(c => c.PartitionKey == parts[0] && c.RowKey == parts[1]))
					{
						customer.Value = (int)(customer.Value * 100) / 100.0;
						data.MatchedCustomers.Add(customer);
					}
				}
			}
			watch.Stop();

			data.CachedResponseTime = watch.ElapsedMilliseconds;
			connection.Close(false);

			return View("Index", data);
		}
		public ActionResult SearchProducts(HomeViewModel data)
		{
			var client = cloudStorageAccount.CreateCloudTableClient();
			var table = client.GetTableReference("products");
			data.MatchedProducts.Clear();
			var watch = new Stopwatch();

			var searchType = SearchType.Name;

			double lowerRange = 0;
			double higherRange = 0;
			int top = 5;

			if (data.SearchString.IndexOf('-') > 0)
			{
				string[] parts = data.SearchString.Split('-');
				if (parts.Length == 2 && double.TryParse(parts[0], out lowerRange) && double.TryParse(parts[1], out higherRange))
					searchType = SearchType.Range;
			}
			else if (data.SearchString.StartsWith("top:"))
			{
				searchType = SearchType.Top;
				top = int.Parse(data.SearchString.Substring(4));
			}
			if (data.UseTable)
			{
				watch.Start();
				IQueryable<Product> query = null;

				switch (searchType)
				{
					case SearchType.Range:
						query = table
							.CreateQuery<Product>()
							.Where(product => product.Price >= lowerRange && product.Price <= higherRange && product.PartitionKey == data.ProductCategory);
						break;
					case SearchType.Name:
						query = table
							.CreateQuery<Product>()
							.Where(product => product.Name == data.SearchString && product.PartitionKey == data.ProductCategory);
						break;
					case SearchType.Top:
						query = table
							.CreateQuery<Product>()
							.Where(product => product.PartitionKey == data.ProductCategory);
						break;
				}

				// ReSharper disable once PossibleNullReferenceException
				foreach (var product in query)
				{
					product.Price = (int)(product.Price * 100) / 100.0;
					data.TableProducts.Add(product);
				}
				switch (searchType)
				{
					case SearchType.Range:
						data.TableProducts.Sort((p1, p2) => p2.Price.CompareTo(p1.Price)); //descending comparision
						break;
					case SearchType.Top:
						data.TableProducts.Sort((p1, p2) => p2.Rate.CompareTo(p1.Rate)); //descending comparision
						data.TableProducts = data.TableProducts.Take(top).ToList();
						break;
				}
				watch.Stop();
				data.TableResponseTime = watch.ElapsedMilliseconds;
			}

			var connection = ConnectionMultiplexer.Connect(new ConfigurationOptions
			{
				EndPoints = { { DemoSettings.ProductsRedisCache.Url, DemoSettings.ProductsRedisCache.Port } },
				Password = DemoSettings.ProductsRedisCache.Password
			});
			var db = connection.GetDatabase();
			watch.Restart();

			switch (searchType)
			{
				case SearchType.Range:
					List<Product> products = getProducts(data.ProductCategory, lowerRange, higherRange);
					data.MatchedProducts = products;
					break;
				case SearchType.Top:
					List<string> keys = getTopKeys(data.ProductCategory, top);
					foreach (string key in keys)
					{
						string[] parts = key.Split(':');
						if (parts.Length == 5)
						{
							var quickQuery = from c in table.CreateQuery<Product>()
															 where c.PartitionKey == parts[0] && c.RowKey == parts[1]
															 select c;
							foreach (var c in quickQuery)
							{
								c.Price = (int)(c.Price * 100) / 100.0;
								data.MatchedProducts.Add(c);
							}
						}
					}
					break;
				case SearchType.Name:
					var record = db.StringGet("prod:" + data.ProductCategory + ":" + data.SearchString.Replace(' ', ':'));
					if (!record.IsNullOrEmpty)
					{
						string[] parts = Encoding.ASCII.GetString(record).Split(':');
						if (parts.Length == 5)
						{
							var quickQuery = from c in table.CreateQuery<Product>()
															 where c.PartitionKey == parts[0] && c.RowKey == parts[1]
															 select c;
							foreach (var c in quickQuery)
							{
								c.Price = (int)(c.Price * 100) / 100.0;
								data.MatchedProducts.Add(c);
							}
						}
					}
					break;
			}

			watch.Stop();
			data.CachedResponseTime = watch.ElapsedMilliseconds;
			connection.Close(false);
			return View("Products", data);
		}
		private List<Product> getProducts(string category, double start, double end)
		{
			var products = new List<Product>();
			var connection = new RedisConnection(DemoSettings.ProductsRedisCache.Url,
					password: DemoSettings.ProductsRedisCache.Password);
			connection.Open();
			var list = connection.SortedSets.Range(0, "cat:" + category, min: start, max: end, ascending: false).Result;
			foreach (var item in list)
			{
				string[] parts = Encoding.ASCII.GetString(item.Key).Split(':');
				if (parts.Length == 5)
					products.Add(new Product
					{
						Category = parts[0],
						Name = parts[2],
						Price = (int)(double.Parse(parts[3]) * 100) / 100.0,
						Rate = int.Parse(parts[4])
					});
			}
			connection.Close(false);
			return products;
		}

		private List<string> getTopKeys(string category, int top)
		{
			List<string> ret = new List<string>();
			RedisConnection connection = new RedisConnection(DemoSettings.ProductsRedisCache.Url,
					password: DemoSettings.ProductsRedisCache.Password);
			connection.Open();
			var list = connection.SortedSets.Range(0, "rate:" + category, start: 0, stop: top, ascending: false).Result;
			foreach (var item in list)
				ret.Add(Encoding.ASCII.GetString(item.Key));
			connection.Close(false);
			return ret;
		}
	}

	internal enum SearchType
	{
		Name,
		Range,
		Top
	}
}