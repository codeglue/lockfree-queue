using Threading.Pool;

namespace PerformanceTest
{
	class Program
	{
		static void Main(string[] args)
		{
			//Lockfree queue (bounded)
			//Lock queue(bounded)
			//Lock list(resizable)

			SimpleThreadPool threadPool = new SimpleThreadPool(4, 128);

		}
	}
}
