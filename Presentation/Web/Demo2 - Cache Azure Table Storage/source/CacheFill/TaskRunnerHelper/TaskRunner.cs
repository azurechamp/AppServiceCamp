using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CacheFill.TaskRunnerHelper
{
	public class TaskRunner
	{
		private readonly int tasksInParallel;
		private readonly Queue<Task> tasksToRun;
		private readonly HashSet<TaskContainer> runningTasks;
		private int tasksCompleted;
		private TimeSpan taskTimeTotal;
		private readonly object taskTimeTotalLock;

		public event EventHandler<TaskRunnerStatus> TaskCompleted;

		public TaskRunner(IEnumerable<Task> tasksToRun, int tasksInParallel)
		{
			this.tasksInParallel = tasksInParallel;
			this.tasksToRun = new Queue<Task>(tasksToRun);
			runningTasks = new HashSet<TaskContainer>(new TaskContainerEqualityComparer());
			taskTimeTotalLock = new object();

			tasksCompleted = 0;
			taskTimeTotal = TimeSpan.Zero;

			if (this.tasksToRun.Any(t => t.Status == TaskStatus.Running))
			{
				throw new Exception("The tasks may not be running when sent to this throttling mechanism.");
			}
		}

		public void WaitAll()
		{
			var totalTasks = tasksToRun.ToArray();
			MakeSureToRunEnoughTasks();
			Task.WaitAll(totalTasks);
		}

		private void MakeSureToRunEnoughTasks()
		{
			int numberToRun;
			do
			{
				lock (runningTasks)
				{
					numberToRun = tasksInParallel - runningTasks.Count + 1;
					Task task;
					if (numberToRun > 0 && GetNextTask(out task))
						RunTask(task);
					else break;
				}
			} while (numberToRun > 0);
		}

		private void OnTaskCompleted()
		{
			var handle = TaskCompleted;
			if (handle != null)
			{
				handle.Invoke(this, new TaskRunnerStatus(tasksCompleted, taskTimeTotal, runningTasks.Count));
			}
		}

		private bool GetNextTask(out Task task)
		{
			task = null;
			lock (tasksToRun)
			{
				if (!tasksToRun.Any()) return false;
				task = tasksToRun.Dequeue();
			}
			return true;
		}

		private void RunTask(Task task)
		{
			var taskContainer = new TaskContainer(task,
				(o, s) =>
				{
					// Record that one more task is complete
					Interlocked.Increment(ref tasksCompleted);

					// Record the total run time of all tasks.
					lock (taskTimeTotalLock)
					{
						taskTimeTotal = taskTimeTotal.Add(s.TaskExecutionTime);
					}

					// Remove my own task from the running tasks.
					var completedTask = runningTasks.FirstOrDefault(m => m.Id == s.TaskId);
					if (completedTask != null) runningTasks.Remove(completedTask);

					OnTaskCompleted();

					MakeSureToRunEnoughTasks();
				});

			lock (runningTasks)
			{
				runningTasks.Add(taskContainer);
			}
		}
	}
}