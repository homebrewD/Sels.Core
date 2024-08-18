using Sels.Core.Async.TaskManagement;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Threading;
using Sels.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Sels.Core.Delegates.Async;

namespace Sels.Core.Async.TaskManagement
{
    /// <summary>
    /// Services that manages <see cref="Task"/>(s) created by other services and schedules them on the thread pool.
    /// Makes sure tasks are properly stopped to ensure graceful shutdown.
    /// </summary>
    public interface ITaskManager
    {
        /// <summary>
        /// Schedules a new managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask Schedule<TOutput>(object owner, Func<CancellationToken, Task<TOutput>> action, Action<IManagedTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default);
        /// <summary>
        /// Logs a request to schedule a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="isGlobal">If the task is a global task. Only used if <paramref name="name"/> is set. Global task names are shared among all instances, otherwise the names are shared within the same <paramref name="owner"/></param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask TrySchedule<TOutput>(object owner, string? name, bool isGlobal, Func<CancellationToken, Task<TOutput>> action, Action<IManagedTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default);
        /// <summary>
        /// Schedules a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="isGlobal">If the task is a global task. Only used if <paramref name="name"/> is set. Global task names are shared among all instances, otherwise the names are shared within the same instance</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        Task<IManagedTask> ScheduleAsync<TOutput>(object owner, string? name, bool isGlobal, Func<CancellationToken, Task<TOutput>> action, Action<INamedManagedTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default);
        /// <summary>
        /// Schedules a new managed anonymous task.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedAnonymousTask ScheduleAnonymous<TOutput>(Func<CancellationToken, Task<TOutput>> action, Action<IManagedAnonymousTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default);

        /// <summary>
        /// Uses <paramref name="schedulerAction"/> to create the task to schedule after a delay of <paramref name="delay"/>.
        /// </summary>
        /// <param name="delay">How much to delay the scheduling by</param>
        /// <param name="schedulerAction">Delegate used to create the managed task to schedule</param>
        /// <param name="cascadeCancellation">If the task should be cancelled if it was already scheduled when the current instance is cancelled/disposed</param>
        /// <returns><inheritdoc cref="IDelayedPendingTask{T}"/></returns>
        IDelayedPendingTask<IManagedTask> ScheduleDelayed(TimeSpan delay, Func<ITaskManager, CancellationToken, Task<IManagedTask>> schedulerAction, bool cascadeCancellation = false);
        /// <summary>
        /// Uses <paramref name="schedulerAction"/> to create the task to schedule after a delay of <paramref name="delay"/>.
        /// </summary>
        /// <param name="delay">How much to delay the scheduling by</param>
        /// <param name="schedulerAction">Delegate used to create the managed task to schedule</param>
        /// <param name="cascadeCancellation">If the task should be cancelled if it was already scheduled when the current instance is cancelled/disposed</param>
        /// <returns><inheritdoc cref="IDelayedPendingTask{T}"/></returns>
        IDelayedPendingTask<IManagedTask> ScheduleDelayed(TimeSpan delay, Func<ITaskManager, CancellationToken, IManagedTask> schedulerAction, bool cascadeCancellation = false);
        /// <summary>
        /// Uses <paramref name="schedulerAction"/> to create the task to schedule after a delay of <paramref name="delay"/>.
        /// </summary>
        /// <param name="delay">How much to delay the scheduling by</param>
        /// <param name="schedulerAction">Delegate used to create the managed task to schedule</param>
        /// <param name="cascadeCancellation">If the task should be cancelled if it was already scheduled when the current instance is cancelled/disposed</param>
        /// <returns><inheritdoc cref="IDelayedPendingTask{T}"/></returns>
        IDelayedPendingTask<IManagedAnonymousTask> ScheduleDelayed(TimeSpan delay, Func<ITaskManager, CancellationToken, IManagedAnonymousTask> schedulerAction, bool cascadeCancellation = false);

        /// <summary>
        /// Creates a local queue tied to <paramref name="instance"/> that can be used to schedule throttled managed (anonymous) tasks.
        /// </summary>
        /// <param name="instance">The instance to tie the queue to</param>
        /// <param name="maxConcurrency">How many tasks can scheduled and executed in parallel on the queue</param>
        /// <returns>The local queue that was created</returns>
        IManagedTaskLocalQueue CreateLocalQueue(object instance, int maxConcurrency);
        /// <summary>
        /// Tries to create a global queue with name <paramref name="name"/> that is shared by the whole application.
        /// Global queues are only created if they do not exists yet. <paramref name="maxConcurrency"/> will be ignored if a queue exists.
        /// </summary>
        /// <param name="name">The unique name of the queue</param>
        /// <param name="maxConcurrency">How many tasks can scheduled and executed in parallel on the queue</param>
        /// <returns>The global queue that was created, or the existing one if one with <paramref name="name"/> was already created</returns>
        IManagedTaskGlobalQueue CreateOrGetGlobalQueue(string name, int maxConcurrency);

        /// <summary>
        /// Gets the global managed task with <see cref="IManagedTask.Name"/> set to <paramref name="name"/> if it exists and is still running.
        /// </summary>
        /// <param name="name">The name of the managed task to get</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The managed task with <see cref="IManagedTask.Name"/> set to <paramref name="name"/> if it exists</returns>
        IManagedTask GetByName(string name, CancellationToken token = default);

        /// <summary>
        /// Gets all running managed tasks for instance <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">The instance to fetch the running tasks for</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>All managed tasks owned by <paramref name="instance"/> or an empty array if there aren't any</returns>
        IManagedTask[] GetOwnedBy(object instance, CancellationToken token = default);

        /// <summary>
        /// Cancels all running managed tasks for instance <paramref name="instance"/> (if not cancelled already).
        /// Method does not wait on the cancellation.
        /// </summary>
        /// <param name="instance">The instance to cancel the tasks for</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>All the managed tasks cancellation was requested on</returns>
        IManagedTask[] CancelAllFor(object instance, CancellationToken token = default);

        /// <summary>
        /// Cancels all running managed tasks for instance <paramref name="instance"/> (if not cancelled already) and waits for all managed tasks to stop. 
        /// Also cancels any pending work in queues tied to <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">The instance to cancel the tasks for</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>All managed tasks that were stopped or an empty array if none were stopped</returns>
        Task<IManagedTask[]> StopAllForAsync(object instance, CancellationToken token = default);

        #region Schedule
        /// <summary>
        /// Schedules a new managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask Schedule<TOutput>(object owner, Func<CancellationToken, TOutput> action, Action<IManagedTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule(owner, (t) => {
                return action(t).ToTaskResult();
            }, options, token);
        }

        /// <summary>
        /// Schedules a new managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask ScheduleAction(object owner, Action<CancellationToken> action, Action<IManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule(owner, (t) => {
                action(t);
                return Task.FromResult(Null.Value);
            }, options, token);
        }
        /// <summary>
        /// Schedules a new managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask ScheduleAction(object owner, Func<CancellationToken, Task> action, Action<IManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule<Null>(owner, async (t) =>
            {
                await action(t).ConfigureAwait(false);
                return Null.Value;
            }, options, token);
        }

        /// <summary>
        /// Schedules a new managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask Schedule<TOutput>(object owner, Func<TOutput> action, Action<IManagedTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule(owner, (t) => {
                return action().ToTaskResult();
            }, options, token);
        }

        /// <summary>
        /// Schedules a new managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask ScheduleAction(object owner, Action action, Action<IManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule(owner, (t) => {
                action();
                return Task.FromResult(Null.Value);
            }, options, token);
        }
        /// <summary>
        /// Schedules a new managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask ScheduleAction(object owner, Func<Task> action, Action<IManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule<Null>(owner, async (t) =>
            {
                await action().ConfigureAwait(false);
                return Null.Value;
            }, options, token);
        }
        #endregion

        #region TrySchedule
        /// <summary>
        /// Logs a request to schedule a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="isGlobal">If the task is a global task. Only used if <paramref name="name"/> is set. Global task names are shared among all instances, otherwise the names are shared within the same <paramref name="owner"/></param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask TrySchedule<TOutput>(object owner, string name, bool isGlobal, Func<CancellationToken, TOutput> action, Action<IManagedTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return TrySchedule(owner, name, isGlobal, (t) => {
                return action(t).ToTaskResult();
            }, options, token);
        }

        /// <summary>
        /// Logs a request to schedule a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="isGlobal">If the task is a global task. Only used if <paramref name="name"/> is set. Global task names are shared among all instances, otherwise the names are shared within the same <paramref name="owner"/></param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask TryScheduleAction(object owner, string name, bool isGlobal, Action<CancellationToken> action, Action<IManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return TrySchedule(owner, name, isGlobal, (t) => {
                action(t);
                return Task.FromResult(Null.Value);
            }, options, token);
        }
        /// <summary>
        /// Logs a request to schedule a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="isGlobal">If the task is a global task. Only used if <paramref name="name"/> is set. Global task names are shared among all instances, otherwise the names are shared within the same <paramref name="owner"/></param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask TryScheduleAction(object owner, string name, bool isGlobal, Func<CancellationToken, Task> action, Action<IManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return TrySchedule<Null>(owner, name, isGlobal, async (t) =>
            {
                await action(t).ConfigureAwait(false);
                return Null.Value;
            }, options, token);
        }

        /// <summary>
        /// Logs a request to schedule a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="isGlobal">If the task is a global task. Only used if <paramref name="name"/> is set. Global task names are shared among all instances, otherwise the names are shared within the same <paramref name="owner"/></param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask TrySchedule<TOutput>(object owner, string name, bool isGlobal, Func<TOutput> action, Action<IManagedTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return TrySchedule(owner, name, isGlobal, (t) => {
                return action().ToTaskResult();
            }, options, token);
        }

        /// <summary>
        /// Logs a request to schedule a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="isGlobal">If the task is a global task. Only used if <paramref name="name"/> is set. Global task names are shared among all instances, otherwise the names are shared within the same <paramref name="owner"/></param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask TryScheduleAction(object owner, string name, bool isGlobal, Action action, Action<IManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return TrySchedule(owner, name, isGlobal, (t) => {
                action();
                return Task.FromResult(Null.Value);
            }, options, token);
        }
        /// <summary>
        /// Logs a request to schedule a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="isGlobal">If the task is a global task. Only used if <paramref name="name"/> is set. Global task names are shared among all instances, otherwise the names are shared within the same <paramref name="owner"/></param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask TryScheduleAction(object owner, string name, bool isGlobal, Func<Task> action, Action<IManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return TrySchedule<Null>(owner, name, isGlobal, async (t) =>
            {
                await action().ConfigureAwait(false);
                return Null.Value;
            }, options, token);
        }
        #endregion

        #region ScheduleAsync
        /// <summary>
        /// Schedules a new named managed task tied to <paramref name="owner"/> if it isn't started yet.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="isGlobal">If the task is a global task. Only used if <paramref name="name"/> is set. Global task names are shared among all instances, otherwise the names are shared within the same <paramref name="owner"/></param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        Task<IManagedTask> ScheduleAsync<TOutput>(object owner, string? name, bool isGlobal, Func<CancellationToken, TOutput> action, Action<INamedManagedTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAsync(owner, name, isGlobal, (t) => {
                return action(t).ToTaskResult();
            }, options, token);
        }

        /// <summary>
        /// Schedules a new named managed task tied to <paramref name="owner"/> if it isn't started yet.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="isGlobal">If the task is a global task. Only used if <paramref name="name"/> is set. Global task names are shared among all instances, otherwise the names are shared within the same <paramref name="owner"/></param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        Task<IManagedTask> ScheduleActionAsync(object owner, string? name, bool isGlobal, Action<CancellationToken> action, Action<INamedManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAsync(owner, name, isGlobal, (t) => {
                action(t);
                return Task.FromResult(Null.Value);
            }, options, token);
        }
        /// <summary>
        /// Schedules a new named managed task tied to <paramref name="owner"/> if it isn't started yet.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="isGlobal">If the task is a global task. Only used if <paramref name="name"/> is set. Global task names are shared among all instances, otherwise the names are shared within the same <paramref name="owner"/></param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        Task<IManagedTask> ScheduleActionAsync(object owner, string? name, bool isGlobal, Func<CancellationToken, Task> action, Action<INamedManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAsync(owner, name, isGlobal, async (t) => {
                await action(t).ConfigureAwait(false);
                return Null.Value;
            }, options, token);
        }

        /// <summary>
        /// Schedules a new named managed task tied to <paramref name="owner"/> if it isn't started yet.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="isGlobal">If the task is a global task. Only used if <paramref name="name"/> is set. Global task names are shared among all instances, otherwise the names are shared within the same <paramref name="owner"/></param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        Task<IManagedTask> ScheduleAsync<TOutput>(object owner, string? name, bool isGlobal, Func<TOutput> action, Action<INamedManagedTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAsync(owner, name, isGlobal, (t) => {
                return action().ToTaskResult();
            }, options, token);
        }

        /// <summary>
        /// Schedules a new named managed task tied to <paramref name="owner"/> if it isn't started yet.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="isGlobal">If the task is a global task. Only used if <paramref name="name"/> is set. Global task names are shared among all instances, otherwise the names are shared within the same <paramref name="owner"/></param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        Task<IManagedTask> ScheduleActionAsync(object owner, string? name, bool isGlobal, Action action, Action<INamedManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAsync(owner, name, isGlobal, (t) => {
                action();
                return Task.FromResult(Null.Value);
            }, options, token);
        }
        /// <summary>
        /// Schedules a new named managed task tied to <paramref name="owner"/> if it isn't started yet.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="isGlobal">If the task is a global task. Only used if <paramref name="name"/> is set. Global task names are shared among all instances, otherwise the names are shared within the same <paramref name="owner"/></param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        Task<IManagedTask> ScheduleActionAsync(object owner, string? name, bool isGlobal, Func<Task> action, Action<INamedManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAsync(owner, name, isGlobal, async (t) => {
                await action().ConfigureAwait(false);
                return Null.Value;
            }, options, token);
        }
        #endregion

        #region ScheduleAnonymous
        /// <summary>
        /// Schedules a new managed anonymous task.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedAnonymousTask ScheduleAnonymous<TOutput>(Func<CancellationToken, TOutput> action, Action<IManagedAnonymousTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAnonymous((t) => {
                return action(t).ToTaskResult();
            }, options, token);
        }

        /// <summary>
        /// Schedules a new managed anonymous task.
        /// </summary>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedAnonymousTask ScheduleAnonymousAction(Action<CancellationToken> action, Action<IManagedAnonymousTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAnonymous((t) => {
                action(t);
                return Task.FromResult(Null.Value);
            }, options, token);
        }
        /// <summary>
        /// Schedules a new managed anonymous task.
        /// </summary>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedAnonymousTask ScheduleAnonymousAction(Func<CancellationToken, Task> action, Action<IManagedAnonymousTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAnonymous(async (t) => {
                await action(t).ConfigureAwait(false);
                return Null.Value;
            }, options, token);
        }

        /// <summary>
        /// Schedules a new managed anonymous task.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedAnonymousTask ScheduleAnonymous<TOutput>(Func<TOutput> action, Action<IManagedAnonymousTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAnonymous((t) => {
                return action().ToTaskResult();
            }, options, token);
        }

        /// <summary>
        /// Schedules a new managed anonymous task.
        /// </summary>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedAnonymousTask ScheduleAnonymousAction(Action action, Action<IManagedAnonymousTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAnonymous((t) => {
                action();
                return Task.FromResult(Null.Value);
            }, options, token);
        }
        /// <summary>
        /// Schedules a new managed anonymous task.
        /// </summary>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedAnonymousTask ScheduleAnonymousAction(Func<Task> action, Action<IManagedAnonymousTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAnonymous(async (t) => {
                await action().ConfigureAwait(false);
                return Null.Value;
            }, options, token);
        }
        #endregion

        #region Delay
        /// <summary>
        /// Uses <paramref name="schedulerAction"/> to create the task to schedule after <paramref name="scheduleTime"/>.
        /// </summary>
        /// <param name="scheduleTime">The date after which the pending task can be scheduled</param>
        /// <param name="schedulerAction">Delegate used to create the managed task to schedule</param>
        /// <returns><inheritdoc cref="IDelayedPendingTask{T}"/></returns>
        IDelayedPendingTask<IManagedTask> ScheduleDelayed(DateTime scheduleTime, Func<ITaskManager, CancellationToken, Task<IManagedTask>> schedulerAction)
        {
            var delay = scheduleTime - DateTime.Now;

            return ScheduleDelayed(delay > TimeSpan.Zero ? delay : TimeSpan.Zero, schedulerAction);
        }
        /// <summary>
        /// Uses <paramref name="schedulerAction"/> to create the task to schedule after <paramref name="scheduleTime"/>.
        /// </summary>
        /// <param name="scheduleTime">The date after which the pending task can be scheduled</param>
        /// <param name="schedulerAction">Delegate used to create the managed task to schedule</param>
        /// <returns><inheritdoc cref="IDelayedPendingTask{T}"/></returns>
        IDelayedPendingTask<IManagedTask> ScheduleDelayed(DateTime scheduleTime, Func<ITaskManager, CancellationToken, IManagedTask> schedulerAction)
        {
            var delay = scheduleTime - DateTime.Now;

            return ScheduleDelayed(delay > TimeSpan.Zero ? delay : TimeSpan.Zero, schedulerAction);
        }
        /// <summary>
        /// Uses <paramref name="schedulerAction"/> to create the task to schedule after <paramref name="scheduleTime"/>.
        /// </summary>
        /// <param name="scheduleTime">The date after which the pending task can be scheduled</param>
        /// <param name="schedulerAction">Delegate used to create the managed task to schedule</param>
        /// <returns><inheritdoc cref="IDelayedPendingTask{T}"/></returns>
        IDelayedPendingTask<IManagedAnonymousTask> ScheduleDelayed(DateTime scheduleTime, Func<ITaskManager, CancellationToken, IManagedAnonymousTask> schedulerAction)
        {
            var delay = scheduleTime - DateTime.Now;

            return ScheduleDelayed(delay > TimeSpan.Zero ? delay : TimeSpan.Zero, schedulerAction);
        }
        #endregion
    }
}
