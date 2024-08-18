using Sels.Core.Async.TaskManagement;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static Sels.Core.Delegates.Async;

namespace Sels.Core.Async.TaskManagement
{
    /// <summary>
    /// Base class for creating managed task wrappers.
    /// </summary>
    public abstract class BaseManagedTask : IManagedAnonymousTask
    {
        // Fields
        private readonly TimeSpan _maxCancelTime;
        private readonly TaskCompletionSource<bool> _onExecutedSource = new TaskCompletionSource<bool>();
        private readonly ManagedTaskCreationSharedOptions _taskOptions;
        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();

        // State
        /// <summary>
        /// Any continuations that created managed tasks. 
        /// </summary>
        protected List<IManagedTask> _continuations;
        /// <summary>
        /// Any continuations that created managed anonymous tasks. 
        /// </summary>
        protected List<IManagedAnonymousTask> _anonymousContinuations;
        private CancellationTokenRegistration _cancellationRegistration;
        private CancellationTokenRegistration _cancellingRegistration;
        private System.Timers.Timer _deadlockTimer;

        /// <inheritdoc cref="BaseManagedTask"/>
        /// <param name="taskOptions">The options for this task</param>
        /// <param name="maxCancelTime">How long a task is allowed to be cancelling before being declared deadlocked</param>
        /// <param name="cancellationToken">Token that the caller can use to cancel the managed task</param>
        public BaseManagedTask(ManagedTaskCreationSharedOptions taskOptions, TimeSpan maxCancelTime, CancellationToken cancellationToken)
        {
            _taskOptions = taskOptions.ValidateArgument(nameof(taskOptions));
            _maxCancelTime = maxCancelTime;

            // Register to cancellation
            Token = cancellationToken;
            _cancellationRegistration = cancellationToken.Register(Cancel);
            _cancellingRegistration = _cancellationSource.Token.Register(StartDeadlockTimer);
        }

        /// <summary>
        /// Schedules the task on the thread pool.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual void Start()
        {
            lock (_cancellationSource)
            {
                if (Task != null) throw new InvalidOperationException($"Task already started");

                // Schedule work on thread pool
                var scheduledTask = Task.Factory.StartNew(async () =>
                {
                    _cancellationSource.Token.ThrowIfCancellationRequested();
                    StartedDate = DateTime.Now;

                    try
                    {
                        using (Helper.Time.CaptureDuration(x => Duration = x))
                        {
                            return await _taskOptions.ExecuteDelegate(_cancellationSource.Token);
                        }
                    }
                    finally
                    {
                        FinishedDate = DateTime.Now;
                    }
                }, _cancellationSource.Token, _taskOptions.TaskCreationOptions & ~TaskCreationOptions.AttachedToParent, TaskScheduler.Default).Unwrap();

                Task = scheduledTask;
                // Handle task result using continuation
                scheduledTask.ContinueWith(OnCompleted);
            }
        }

        /// <inheritdoc/>
        public CancellationToken Token { get; }
        /// <inheritdoc/>
        public Task Task { get; private set; }
        /// <inheritdoc/>
        public bool CancellationRequested { get { lock (_cancellationSource) { return _cancellationSource.IsCancellationRequested; } } }
        /// <inheritdoc/>
        public ManagedTaskOptions Options => _taskOptions.ManagedTaskOptions;
        /// <inheritdoc/>
        public IReadOnlyDictionary<string, object> Properties => _taskOptions.Properties;
        /// <inheritdoc/>
        public Task OnExecuted => _onExecutedSource.Task;
        /// <inheritdoc/>
        public Task OnFinalized { get; protected set; }
        /// <inheritdoc/>
        public object? Result { get; private set; }
        /// <inheritdoc/>
        public DateTime CreatedDate { get; } = DateTime.Now;
        /// <inheritdoc/>
        public DateTime? StartedDate { get; private set; }
        /// <inheritdoc/>
        public TimeSpan? Duration { get; private set; }
        /// <inheritdoc/>
        public DateTime? FinishedDate { get; private set; }


        /// <inheritdoc/>
        public IManagedTask[] Continuations => _continuations?.ToArray() ?? Array.Empty<IManagedTask>();
        /// <inheritdoc/>
        public IManagedAnonymousTask[] AnonymousContinuations => _anonymousContinuations?.ToArray() ?? Array.Empty<IManagedAnonymousTask>();

        /// <inheritdoc/>
        public void Cancel() { lock (_cancellationSource) { _cancellationSource.Cancel(); } }
        /// <inheritdoc/>
        public void CancelAfter(TimeSpan delay) { lock (_cancellationSource) { _cancellationSource.CancelAfter(delay); } }

        private async Task OnCompleted(Task<object> completedTask)
        {
            try
            {
                // Get task result
                object result = null;
                try
                {
                    var taskResult = await completedTask.ConfigureAwait(false);
                    if (taskResult != null && taskResult != Null.Value) result = taskResult;
                }
                catch (Exception ex)
                {
                    result = ex;
                }
                Result = result;

                await TriggerContinuations().ConfigureAwait(false);
            }
            finally
            {
                lock(_onExecutedSource)
                {
                    _onExecutedSource.TrySetResult(true);
                }
            }
        } 

        private void StartDeadlockTimer()
        {
            lock(_cancellationSource)
            {
                _deadlockTimer = new System.Timers.Timer();
                _deadlockTimer.Elapsed += (s, a) => TryDeclareDeadlocked();
                _deadlockTimer.Interval = _maxCancelTime.TotalMilliseconds;
                _deadlockTimer.AutoReset = false;
                _deadlockTimer.Start();
            }
        }

        private void TryDeclareDeadlocked()
        {
            lock (_cancellationSource)
            {
                if (Task == null || Task.IsCompleted) return;

                lock (_onExecutedSource)
                {
                    _onExecutedSource.TrySetException(new ManagedTaskDeadlockedException(this.AsEnumerable()));
                }
            }
        }

        /// <summary>
        /// Trigger the continuations for this task if there are any defined.
        /// </summary>
        protected abstract Task TriggerContinuations();
        
        /// <summary>
        /// Releases any managed resources.
        /// </summary>
        public void Cleanup()
        {
            lock (_cancellationSource)
            {
                _cancellingRegistration.Dispose();
                if (!_cancellationSource.IsCancellationRequested) _cancellationSource.Cancel();
            }

            lock (_onExecutedSource)
            {
                _onExecutedSource.TrySetException(new ObjectDisposedException(GetType().Name));
            }

            lock (_cancellationSource)
            {
                _deadlockTimer?.Dispose();
            }

            _cancellationRegistration.Dispose();
        }
        /// <inheritdoc/>
        public void Dispose() => Cancel();
    }
}
