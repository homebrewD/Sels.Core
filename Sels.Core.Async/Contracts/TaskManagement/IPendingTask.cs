using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Async.TaskManagement
{
    /// <summary>
    /// Represents a managed (anonymous) task that still needs to be queued on the Thread Pool and executed.
    /// Disposing will cancel the task.
    /// </summary>
    /// <typeparam name="T">The type of the pending task</typeparam>
    public interface IPendingTask<T> : IDisposable where T : IManagedAnonymousTask
    {
        /// <summary>
        /// When the pending task was created.
        /// </summary>
        DateTime Created { get; }
        /// <summary>
        /// Task that will complete when the managed (anonymous) task is created and scheduled on the thread pool or when the task gets cancelled.
        /// </summary>
        Task<T> Callback { get; }
        /// <summary>
        /// Whether or not the pending task was cancelled.
        /// </summary>
        public bool IsCancelled { get; }
        /// <summary>
        /// Cancels the pending task if it wasn't scheduled yet, otherwise cancels the managed task if cascade enabled.
        /// Same as calling <see cref="IDisposable.Dispose"/>.
        /// </summary>
        public void Cancel();

        /// <summary>
        /// Cancels the pending task and wait until it's cancelled.
        /// </summary>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Taskt hat will complete when either the pending task is cancelled or when <paramref name="token"/> gets cancelled</returns>
        public async Task CancelAndWait(CancellationToken token = default)
        {
            Cancel();

            try
            {
                if (Callback.IsCompleted) return;

                await Helper.Async.WaitOn(Callback, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        /// <summary>
        /// Cancels the pending task and wait until it's cancelled.
        /// If the task was scheduled the managed task will be cancelled and the method will block until the task is executed.
        /// </summary>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Taskt hat will complete when either the pending task is cancelled or when <paramref name="token"/> gets cancelled</returns>
        public async Task CancelAndWaitOnExecution(CancellationToken token = default)
        {
            try
            {
                // Cancel
                await CancelAndWait(token).ConfigureAwait(false);

                // Task was fully scheduled so wait on execution
                if (Callback.IsCompletedSuccessfully)
                {
                    var managedTask = await Helper.Async.WaitOn(Callback, token).ConfigureAwait(false);
                    managedTask.Cancel();
                    await WaitOnExecution(token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        /// <summary>
        /// Cancels the pending task and wait until it's cancelled.
        /// If the task was scheduled the managed task will be cancelled and the method will block until the task is finalized.
        /// </summary>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Taskt hat will complete when either the pending task is cancelled or when <paramref name="token"/> gets cancelled</returns>
        public async Task CancelAndWaitOnFinalization(CancellationToken token = default)
        {
            try
            {
                // Cancel
                await CancelAndWait(token).ConfigureAwait(false);

                // Task was fully scheduled so wait on execution
                if (Callback.IsCompletedSuccessfully)
                {
                    var managedTask = await Helper.Async.WaitOn(Callback, token).ConfigureAwait(false);
                    managedTask.Cancel();
                    await WaitOnFinalization(token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        /// <summary>
        /// Waits for the pending task to get scheduled and executed.
        /// </summary>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The managed task after it was executed</returns>
        public async Task<T> WaitOnExecution(CancellationToken token = default)
        {
            var managedTask = await Helper.Async.WaitOn(Callback, token).ConfigureAwait(false);

            await Helper.Async.WaitOn(managedTask.OnExecuted, token).ConfigureAwait(false);
            return managedTask;
        }

        /// <summary>
        /// Waits for the pending task to get scheduled, executed and finalized.
        /// </summary>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The managed task after it was executed and finalized</returns>
        public async Task<T> WaitOnFinalization(CancellationToken token = default)
        {
            var managedTask = await Helper.Async.WaitOn(Callback, token).ConfigureAwait(false);

            await Helper.Async.WaitOn(managedTask.OnFinalized, token).ConfigureAwait(false);
            return managedTask;
        }
    }
}
