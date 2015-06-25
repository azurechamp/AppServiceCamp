using System;
using System.Collections.Generic;
using System.Web.Http;
using BookSleeve;
using HelperLib;

namespace CustomerQuery.Controllers
{
	public class ProductAutoCompleteController : ApiController
	{
		public List<string> Get(string prefix, string category)
		{
			List<string> ret = new List<string>();
			RedisConnection connection = new RedisConnection(
				host: DemoSettings.ProductsRedisCache.Url,
				password: DemoSettings.ProductsRedisCache.Password);
			connection.Open();
			var list = connection.Wait(connection.Keys.Find(0, "prod:" + category + ":" + prefix.Replace(' ', ':') + "*"));
			for (int i = 0; i < Math.Min(5, list.Length); i++)
			{
				string s = list[i].Substring(5);
				s = s.Substring(s.IndexOf(':') + 1);
				ret.Add(s.Replace(':', ' '));
			}
			connection.Close(false);
			return ret;
		}
	}
}