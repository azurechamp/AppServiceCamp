using System;
using System.Configuration;

namespace HelperLib
{
	public static class DemoSettings
	{
		private static string cloudStorageAccount;
		private static RedisCacheSettings redisCache1;
		private static RedisCacheSettings redisCache2;

		public static class Storage
		{
			/// <summary>
			/// The connection string "Storage connection string" in config.
			/// </summary>
			public static string ConnectionString
			{
				get
				{
					return ConfigurationManager.ConnectionStrings["Storage connection string"].ConnectionString;
				}
			}

			/// <summary>
			/// "customers"
			/// </summary>
			public static string CustomerTableName { get { return "customers"; } }
			/// <summary>
			/// "products"
			/// </summary>
			public static string ProductsTableName { get { return "products"; } }
		}
	
		public static RedisCacheSettings CustomerRedisCache
		{
			get
			{
				if (redisCache1 != null) return redisCache1;
				
				var redisUrl = ConfigurationManager.AppSettings["redis-cache-customers-url"];
				var redisPassword = ConfigurationManager.AppSettings["redis-cache-customers-password"];
				int redisPort;
				if (!Int32.TryParse(ConfigurationManager.AppSettings["redis-cache-customers-port"], out redisPort))
					redisPort = RedisCacheSettings.DefaultPort;
		
				redisCache1 = new RedisCacheSettings(redisUrl, redisPassword, redisPort);
				return redisCache1;
			}
		}
	
		public static RedisCacheSettings ProductsRedisCache
		{
			get
			{
				if (redisCache2 != null) return redisCache2;
				
				var redisUrl = ConfigurationManager.AppSettings["redis-cache-products-url"];
				var redisPassword = ConfigurationManager.AppSettings["redis-cache-products-password"];
				int redisPort;
				if (!Int32.TryParse(ConfigurationManager.AppSettings["redis-cache-products-port"], out redisPort))
					redisPort = RedisCacheSettings.DefaultPort;

				redisCache2 = new RedisCacheSettings(redisUrl, redisPassword, redisPort);
				return redisCache2;
			}
		}
	}
}