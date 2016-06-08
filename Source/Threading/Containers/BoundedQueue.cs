using System.Threading;

//Source code supplied under MIT license, original C++ source is distributed
//Original: http://www.1024cores.net/home/lock-free-algorithms/queues/bounded-mpmc-queue

namespace Threading.Containers
{

	public class BoundedQueue<T>
	{
		private class Cell
		{
			public long sequence;
			public T data;
		}

		private readonly Cell[] entries;
		private readonly int modMask;

		private long enqueuePosition;
		private long dequeuePosition;

		public BoundedQueue(int capacity)
		{
			capacity = NextPowerOfTwo(capacity);
			modMask = capacity - 1;
			entries = new Cell[capacity];

			for (int i = 0; i < capacity; i++)
			{
				entries[i] = new Cell();
				entries[i].data = default(T);
				entries[i].sequence = i;
			}

			enqueuePosition = 0;
			dequeuePosition = 0;
		}

		/// <summary>
		/// Lockless thread-safe Enqueue
		/// </summary>
		/// <param name="data"></param>
		/// <returns>Return true if succesfully enqueued, returns false if there was no place left.</returns>
		public bool Enqueue(T data)
		{
			Cell cell = null;

			long pos = Interlocked.Read(ref enqueuePosition);

			for (;;)
			{
				cell = entries[pos & modMask];
				long seq = Interlocked.Read(ref cell.sequence);

				long dif = seq - pos;

				if (dif == 0)
				{
					//http://geekswithblogs.net/BlackRabbitCoder/archive/2012/09/06/c.net-little-wonders-interlocked-compareexchange.aspx
					if (Interlocked.CompareExchange(ref enqueuePosition, pos + 1, pos) == pos)
					{
						break;
					}

				}
				else if (dif < 0)
				{
					return false;
				}
				else
				{
					pos = Interlocked.Read(ref enqueuePosition);
				}
			}

			cell.data = data;
			Interlocked.Exchange(ref cell.sequence, pos + 1);

			return true;
		}

		/// <summary>
		/// Lockless, threadsafe dequeue method.
		/// </summary>
		/// <param name="data"></param>
		/// <returns>Returns false if there was nothing to return, true if succeeded</returns>
		public bool Dequeue(out T data)
		{
			Cell cell = null;
			long pos = Interlocked.Read(ref dequeuePosition);

			for (;;)
			{
				cell = entries[pos & modMask];

				long seq = Interlocked.Read(ref cell.sequence);

				long dif = seq - (pos + 1);

				if (dif == 0)
				{
					if (Interlocked.CompareExchange(ref dequeuePosition, pos + 1, pos) == pos)
					{
						break;
					}
				}
				else if (dif < 0)
				{
					data = default(T);
					return false;
				}
				else
				{
					pos = Interlocked.Read(ref dequeuePosition);
				}

			}

			data = cell.data;
			Interlocked.Exchange(ref cell.sequence, pos + modMask + 1);

			return true;
		}

		private static int NextPowerOfTwo(int x)
		{
			int result = 2;

			while (result < x)
			{
				result <<= 1;
			}

			return result;
		}
	}
}
