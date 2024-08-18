using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Async.TaskManagement
{
    /// <summary>
    /// Represents an anonymous task that was scheduled using a <see cref="ITaskManager"/>.
    /// Disposing will cancel the task.
    /// </summary>
    public interface IManagedAnonymousTask : IDisposable
    {
        /// <summary>
        /// The cancellation token used to cancel the current task.
        /// </summary>
        CancellationToken Token { get; }
        /// <summary>
        /// The task that was scheduled on the Thread Pool.
        /// </summary>
        Task Task { get; }
        /// <summary>
        /// If cancellation for the managed task was requested.
        /// </summary>
        bool CancellationRequested { get; }
        /// <summary>
        /// The options used when creating the task.
        /// </summary>
        ManagedTaskOptions Options { get; }
        /// <summary>
        /// Any properties added to <see cref="Task"/>.
        /// </summary>
        IReadOnlyDictionary<string, object> Properties { get; }

        /// <summary>
        /// Task that will complete when the current task finishes executing and all continuations are triggered.
        /// </summary>
        Task OnExecuted { get; }
        /// <summary>
        /// Task that will complete when the task manager has handled the execution of the managed (anonymous) task.
        /// </summary>
        Task OnFinalized { get; }
        /// <summary>
        /// The result from executing <see cref="Task"/>. Will be the return value from the task if it executed successfully, null if executed successfully but doesn't return a value or the exception if it failed.
        /// </summary>
        object? Result { get; }
        /// <summary>
        /// The date the managed (anonymous) task was created and scheduled.
        /// </summary>
        DateTime CreatedDate { get; }
        /// <summary>
        /// The date the managed (anonymous) task started executing.
        /// </summary>
        DateTime? StartedDate { get; }
        /// <summary>
        /// How long the managed (anonymous) task executed.
        /// </summary>
        TimeSpan? Duration { get; }
        /// <summary>
        /// The date the managed (anonymous) task finished executing.
        /// </summary>
        DateTime? FinishedDate { get; }

        /// <summary>
        /// If this task should be kept alive if it fails with an exception.
        /// </summary>
        bool KeepAlive => Options.HasFlag(ManagedTaskOptions.KeepAlive);
        /// <summary>
        /// If this task can be cancelled instantly when graceful cancellation is requested.
        /// </summary>
        bool FastCancellation => Options.HasFlag(ManagedTaskOptions.GracefulCancellation);

        /// <summary>
        /// Any continuations that were created when the current task finished executing. Will be empty array if none were triggered.
        /// </summary>
        IManagedTask[] Continuations { get; }
        /// <summary>
        /// Any anonymous continuations that were created when the current task finished executing. Will be empty array if none were triggered.
        /// </summary>
        IManagedAnonymousTask[] AnonymousContinuations { get; }

        /// <summary>
        /// Cancels the task if it is running.
        /// </summary>
        void Cancel();
        /// <summary>
        /// Cancels the task after <paramref name="delay"/> if it is still running by then.
        /// </summary>
        /// <param name="delay">How long to wait before cancelling</param>
        void CancelAfter(TimeSpan delay);

        /// <summary>
        /// Returns the result of the executed task.
        /// </summary>
        /// <typeparam name="T">The expected type of the result</typeparam>
        /// <returns><see cref="Result"/> of the executed task casted to <typeparamref name="T"/></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public T GetResult<T>()
        {
            if (!OnExecuted.IsCompleted) throw new InvalidOperationException($"Managed task is not executed yet");

            if(Result is Exception exception) exception.Rethrow();
            return Result.CastTo<T>();
        }

        /// <summary>
        /// Waits for the task to execute and returns the result of the executed task.
        /// </summary>
        /// <typeparam name="T">The expected type of the result</typeparam>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns><see cref="Result"/> of the executed task casted to <typeparamref name="T"/></returns>
        public async Task<T> GetResultAsync<T>(CancellationToken token = default)
        {
            await Helper.Async.WaitOn(OnExecuted, token).ConfigureAwait(false);
            return GetResult<T>();
        }
    }
}
