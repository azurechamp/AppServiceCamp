using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CacheFill.TaskRunnerHelper
{
	// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
	internal class TaskContainer : IDisposable
	{
		private bool disposed;

		private readonly Stopwatch stopwatch;
		private EventHandler<TaskRunEventArgs> completed;

		public TaskContainer(Task task, EventHandler<TaskRunEventArgs> completed)
		{
			Task = task;
			this.completed = completed;
			Completed += completed;
			stopwatch = Stopwatch.StartNew();
			if (task.Status == TaskStatus.WaitingForActivation || task.Status == TaskStatus.Created)
			{
				task.Start();
			}
			if (task.IsCompleted)
			{
				OnCompleted();
				return;
			}

			task.GetAwaiter().OnCompleted(OnCompleted);
		}

		~TaskContainer()
		{
			Dispose(false);
		}

		private Task Task { get; set; }

		private void OnCompleted()
		{
			stopwatch.Stop();
			var handle = Completed;
			if (handle != null)
			{
				Completed.Invoke(this, new TaskRunEventArgs(Id, stopwatch.Elapsed));
			}
			Completed -= completed;
		}

		public event EventHandler<TaskRunEventArgs> Completed;

		public int Id
		{
			get
			{
				return Task.Id;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			var rt = obj as TaskContainer;
			return rt != null && Id.Equals(rt.Id);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}

			if (disposing)
			{
				Completed -= completed;
				completed = null;
			}

			disposed = true;
		}
	}
}