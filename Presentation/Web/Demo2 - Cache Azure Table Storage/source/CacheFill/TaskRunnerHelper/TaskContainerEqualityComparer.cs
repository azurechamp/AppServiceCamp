using System.Collections.Generic;

namespace CacheFill.TaskRunnerHelper
{
	internal class TaskContainerEqualityComparer : IEqualityComparer<TaskContainer>
	{

		public bool Equals(TaskContainer task1, TaskContainer task2)
		{
			return task1.Id == task2.Id;
		}

		public int GetHashCode(TaskContainer taskContainer)
		{
			return taskContainer.GetHashCode();
		}
	}
}