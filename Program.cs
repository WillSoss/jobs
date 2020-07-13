using Runly;
using System.Threading.Tasks;

namespace WillSoss.Photo
{
	class Program
	{
		static async Task Main(string[] args)
		{
			await JobHost.CreateDefaultBuilder(args)
				.Build()
				.RunJobAsync();
		}
	}
}
