using Sels.Core.Async.TaskManagement;
using Sels.Core.Dispose;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Threading;
using Sels.Core.Scope.Actions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Async.Templates.TaskManagement
{
    /// <summary>
    /// base class for creating a <see cref="IDelayedPendingTask{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the pending task</typeparam>
    public abstract class BaseDelayedPendingTask<T> : IDelayedPendingTask<T>, IExposedDisposable
        where T : IManagedAnonymousTask
    {
        // Fields
        private readonly object _lock = new object();
        private readonly ITaskManager _taskManager;
        private readonly System.Timers.Timer _timer;
        private readonly TaskCompletionSource<T> _taskSource = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly Func<ITaskManager, CancellationToken, Task<T>> _scheduleAction;
        private readonly CancellationTokenRegistration _registration;
        private readonly CancellationTokenSource _cancelSource = new CancellationTokenSource();
        private readonly CancellationTokenSource _cancelScheduledSource = new CancellationTokenSource();
        private readonly bool _cascadeCancel;

        // State
        private bool _isScheduling;

        // Properties
        /// <inheritdoc/>
        public DateTime Created { get; } = DateTime.Now;
        /// <inheritdoc/>
        public Task<T> Callback => _taskSource.Task;
        /// <inheritdoc/>
        public bool IsCancelled => _cancelSource.IsCancellationRequested;
        /// <inheritdoc/>
        public TimeSpan Delay { get; }
        /// <inheritdoc/>
        public DateTime ScheduleTime { get; }
        /// <inheritdoc/>
        public bool? IsDisposed { get; private set; }

        /// <inheritdoc cref="BaseDelayedPendingTask{T}"/>
        /// <param name="scheduleAction">Delegate that schedules the pending task</param>
        /// <param name="taskManager">Task manager to use to schedule the pending task</param>
        /// <param name="delay">How long to delay the pending task by</param>
        /// <param name="cascadeCancel">If the task should be cancelled if it was already scheduled when the current instance is cancelled/disposed</param>
        /// <param name="token">Token that can be used to cancel the pending task</param>
        public BaseDelayedPendingTask(Func<ITaskManager, CancellationToken, Task<T>> scheduleAction, ITaskManager taskManager, TimeSpan delay, bool cascadeCancel, CancellationToken token)
        {
            _scheduleAction = scheduleAction.ValidateArgument(nameof(scheduleAction));
            _taskManager = taskManager.ValidateArgument(nameof(taskManager));
            Delay = delay;
            _cascadeCancel = cascadeCancel;
            _registration = token.Register(Cancel);

            _timer = new System.Timers.Timer();
            _timer.AutoReset = false;
            _timer.Interval = Delay.TotalMilliseconds == 0 ? 1 : Delay.TotalMilliseconds;
            _timer.Elapsed += (s, a) => Trigger();
            _timer.Enabled = true;
            ScheduleTime = DateTime.Now.Add(Delay);
        }

        /// <inheritdoc/>
        public void Cancel()
        {
            try
            {
                lock (_lock)
                {
                    if (IsDisposed.HasValue) return;
                    // Cancel token
                    _cancelSource.Cancel();
                    // Cascade cancel
                    if(_cascadeCancel) _cancelScheduledSource.Cancel();
                }
            }
            finally
            {
                Dispose();
            }
        }

        private async void Trigger()
        {
            try
            {
                // Check if cancelled
                lock (_lock)
                {
                    if (IsDisposed.HasValue) return;
                    if (_cancelSource.IsCancellationRequested) return;
                    if (_cancelScheduledSource.IsCancellationRequested) return;
                    _isScheduling = true;
                }

                // Schedule action
                var task = await _scheduleAction(_taskManager, _cancelScheduledSource.Token).ConfigureAwait(false);

                lock (_lock)
                {
                    _taskSource.TrySetResult(task);
                    if (_cascadeCancel && IsCancelled) task.Cancel();
                }
            }
            catch (OperationCanceledException)
            {
                lock (_lock)
                {
                    _taskSource.TrySetCanceled();
                }
                
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    _taskSource.TrySetException(ex);
                }                
            }
            finally
            {
                Cleanup();
            }
        }


        /// <inheritdoc/>
        public void Dispose()
        {
            if (IsDisposed.HasValue) return;

            lock(_lock)
            {
                if (IsDisposed.HasValue) return;
                using (new ExecutedAction(x => IsDisposed = x))
                {
                    Cleanup();

                    // Dispose registration
                    _registration.Dispose();

                    // Dispose token
                    lock (_lock)
                    {
                        _cancelSource.Dispose();
                    }

                    lock (_lock)
                    {
                        if (_cascadeCancel && Callback.IsCompleted && !Callback.IsCanceled)
                        {
                            Callback.Result.Cancel();
                        }
                    }
                }
            }
        }

        private void Cleanup()
        {
            // Dispose timer
            _timer.Dispose();

            // Complete callback task if not already
            if (!_isScheduling)
            {
                _taskSource.TrySetCanceled();
            }

            lock (_lock)
            {
                _cancelScheduledSource.Dispose();
            }
        }
    }
}
