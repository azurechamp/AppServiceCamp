namespace HelperLib
{
	public class RedisCacheSettings
	{
		public const int DefaultPort = 6379;

		public RedisCacheSettings(string url, string password, int port = DefaultPort)
		{
			Url = url;
			Password = password;
			Port = port;
		}

		/// <summary>
		/// {your-redis-account}.redis.cache.windows.net
		/// </summary>
		public string Url { get; private set; }

		public string Password { get; private set; }
		public int Port { get; private set; }
	}
}