using System;
using System.Collections.Generic;
using System.Threading;
using Threading.Containers;

namespace Threading.Pool
{
	public class SimpleThreadPool : IDisposable
	{
		private List<Thread> _workers = null;
		private BoundedQueue<Action> _taskQueue = null;

		private bool run = true;


		public SimpleThreadPool(int workerCount, int queueSize)
		{
			_taskQueue = new BoundedQueue<Action>(queueSize);
			_workers = new List<Thread>(workerCount);

			for (int i = 0; i < workerCount; i++)
			{
				Thread t = new Thread(Consume) { IsBackground = true, Name = string.Format("Worker Thread {0}", i) };
				_workers.Add(t);
				t.Start();
			}

		}
		public void EnqueueTask(Action task)
		{
			if (!_taskQueue.Enqueue(task))
			{
				throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		/// Consumes the work in the task queue.
		/// </summary>
		private void Consume()
		{
			while (run)
			{
				Action task;

				if (_taskQueue.Dequeue(out task) && run)
				{
					task();
				}
				else
				{
					Thread.Sleep(0);
				}
			}
		}

		/// <summary>
		/// Disposes all worker threads.
		/// </summary>
		public void Dispose()
		{
			run = false;

			_workers.ForEach(thread => thread.Join());
		}
	}
}
