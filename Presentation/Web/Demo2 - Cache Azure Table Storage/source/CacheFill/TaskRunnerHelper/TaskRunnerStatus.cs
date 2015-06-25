using System;

namespace CacheFill.TaskRunnerHelper
{
	public class TaskRunnerStatus : EventArgs
	{
		public int TasksCompleted { get; private set; }
		public TimeSpan TaskTimeTotal { get; private set; }
		public int TasksInParallel { get; private set; }

		public TaskRunnerStatus(int tasksCompleted, TimeSpan taskTimeTotal, int tasksInParallel)
		{
			TasksCompleted = tasksCompleted;
			TaskTimeTotal = taskTimeTotal;
			TasksInParallel = tasksInParallel;
		}
	}
}