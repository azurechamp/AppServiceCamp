using Microsoft.WindowsAzure.Storage;

namespace CacheFill
{
	public static class CloudStorageAccountExtensions
	{
		public static CloudStorageAccount ToCloudStorageAccount(this string connectionString)
		{
			return CloudStorageAccount.Parse(connectionString);
		}
	}
}