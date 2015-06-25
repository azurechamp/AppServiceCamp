using System.Configuration;
using Microsoft.WindowsAzure.Storage;

namespace CustomerQuery
{
	public static class CloudStorageAccountExtensions
	{
		public static CloudStorageAccount ToCloudStorageAccount(this string connectionString)
		{
				return CloudStorageAccount.Parse(connectionString);
		}
	}
}