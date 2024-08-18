using Sels.Core.Async.Templates.TaskManagement;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Threading;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Async.TaskManagement
{
    /// <inheritdoc cref="IDelayedPendingTask{T}"/>
    public class DelayedPendingManagedTask : BaseDelayedPendingTask<IManagedTask>
    {
        /// <inheritdoc cref="DelayedPendingManagedTask"/>
        /// <param name="scheduleAction">Delegate that schedules the pending task</param>
        /// <param name="taskManager">Task manager to use to schedule the pending task</param>
        /// <param name="delay">How long to delay the pending task by</param>
        /// <param name="cascadeCancel">If the task should be cancelled if it was already scheduled when the current instance is cancelled/disposed</param>
        /// <param name="token">Token that can be used to cancel the pending task</param>
        public DelayedPendingManagedTask(Func<ITaskManager, CancellationToken, Task<IManagedTask>> scheduleAction, ITaskManager taskManager, TimeSpan delay, bool cascadeCancel, CancellationToken token) : base(scheduleAction, taskManager, delay, cascadeCancel, token)
        {
        }
        /// <inheritdoc cref="DelayedPendingManagedTask"/>
        /// <param name="scheduleAction">Delegate that schedules the pending task</param>
        /// <param name="taskManager">Task manager to use to schedule the pending task</param>
        /// <param name="delay">How long to delay the pending task by</param>
        /// <param name="cascadeCancel">If the task should be cancelled if it was already scheduled when the current instance is cancelled/disposed</param>
        /// <param name="token">Token that can be used to cancel the pending task</param>
        public DelayedPendingManagedTask(Func<ITaskManager, CancellationToken, IManagedTask> scheduleAction, ITaskManager taskManager, TimeSpan delay, bool cascadeCancel, CancellationToken token) : base((m, t) => scheduleAction(m, t).ToTaskResult(), taskManager, delay, cascadeCancel, token)
        {
            scheduleAction.ValidateArgument(nameof(scheduleAction));
        }
    }
}
