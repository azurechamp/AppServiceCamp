using System;

namespace CacheFill.TaskRunnerHelper
{
	public class TaskRunEventArgs : EventArgs
	{
		public int TaskId { get; private set; }

		public TimeSpan TaskExecutionTime { get; private set; }

		public TaskRunEventArgs(int taskId, TimeSpan taskExecutionTime)
		{
			TaskId = taskId;
			TaskExecutionTime = taskExecutionTime;
		}
	}
}