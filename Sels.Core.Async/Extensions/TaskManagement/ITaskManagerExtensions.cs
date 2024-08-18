using Sels.Core.Extensions;
using Sels.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Async.TaskManagement
{
    /// <summary>
    /// Contains static extension methods for <see cref="ITaskManager"/>
    /// </summary>
    public static class ITaskManagerExtensions
    {
        /// <summary>
        /// Schedules a recurring task that triggers <paramref name="action"/> every <paramref name="interval"/> during the scope.
        /// </summary>
        /// <param name="taskManager">The task manager to use to schedule the task</param>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="isGlobal">If the task is a global task. Only used if <paramref name="name"/> is set. Global task names are shared among all instances, otherwise the names are shared within the same <paramref name="owner"/></param>
        /// <param name="interval">The interval to execute <paramref name="action"/> on</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="exceptionHandler">Optional delegate that can be used to handle exceptions. The bool returned will determine if the task will restart (true) or rethrow the exception (false). When null the result will always be false</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="startImmediately">If the first run should start immediately, otherwise wait <paramref name="interval"/> before starting</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>Disposable used to define the scope to run the recurring task in</returns>
        public static async Task<IDisposable> ScheduleRecurringActionAsync(this ITaskManager taskManager, object owner, string name, bool isGlobal, TimeSpan interval, Func<CancellationToken, Task> action, Func<Exception, CancellationToken, Task<bool>>? exceptionHandler = null, Action<INamedManagedTaskCreationOptions<Null>>? options = null, bool startImmediately = false, CancellationToken token = default)
        {
            taskManager = taskManager.ValidateArgument(nameof(taskManager));
            owner = owner.ValidateArgument(nameof(owner));
            name.ValidateArgument(nameof(name));
            action.ValidateArgument(nameof(action));

            options ??= new Action<INamedManagedTaskCreationOptions<Null>>(x => x.WithManagedOptions(ManagedTaskOptions.GracefulCancellation).WithPolicy(NamedManagedTaskPolicy.CancelAndStart));
            var recurringAction = new Func<CancellationToken, Task>(async t =>
            {
                do
                {
                    try
                    {
                        await action(t).ConfigureAwait(false);
                    }
                    catch(OperationCanceledException) when(t.IsCancellationRequested)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        bool restart = false;

                        if (exceptionHandler != null)
                        {
                            restart = await exceptionHandler(ex, t).ConfigureAwait(false);
                        }

                        if (!restart)
                        {
                            throw;
                        }
                    }

                    await Helper.Async.Sleep(interval, t).ConfigureAwait(false);
                }
                while (!t.IsCancellationRequested);
            });

            if (startImmediately)
            {
                return await taskManager.ScheduleActionAsync(owner, name, isGlobal, recurringAction, options, token).ConfigureAwait(false);
            }
            else
            {
                return taskManager.ScheduleDelayed(interval, (m, t) => m.ScheduleActionAsync(owner, name, isGlobal, recurringAction, options, token), true);
            }
        }
    }
}
