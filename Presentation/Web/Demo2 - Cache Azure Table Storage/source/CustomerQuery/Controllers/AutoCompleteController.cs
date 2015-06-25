using System;
using System.Collections.Generic;
using System.Web.Http;
using BookSleeve;
using HelperLib;

namespace CustomerQuery.Controllers
{
	public class AutoCompleteController : ApiController
	{
		public List<string> Get(string prefix)
		{
			List<string> ret = new List<string>();
			RedisConnection connection = new RedisConnection(
				host: DemoSettings.CustomerRedisCache.Url, 
				password: DemoSettings.CustomerRedisCache.Password);
			connection.Open();
			var list = connection.Wait(connection.Keys.Find(0, "cust:" + prefix.Replace(' ', ':') + "*"));
			for (int i = 0; i < Math.Min(5, list.Length); i++)
			{
				ret.Add(list[i].Substring(5).Replace(':', ' '));
			}
			connection.Close(false);
			return ret;
		}
	}
}