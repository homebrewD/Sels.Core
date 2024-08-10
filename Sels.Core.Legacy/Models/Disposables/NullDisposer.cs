using System;
using System.Threading.Tasks;

namespace Sels.Core.Models.Disposables
{
    /// <summary>
    /// Class that implements <see cref="IDisposable"/> and <see cref="IAsyncDisposable"/> but does nothing in the dispose.
    /// </summary>
    public class NullDisposer : IDisposable, IAsyncDisposable
    {
        private NullDisposer()
        {

        }

        /// <inheritdoc/>
        public void Dispose()
        {

        }

#if NET6_0_OR_GREATER
        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
#else
        private readonly ValueTask _completedTask = new ValueTask();
        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            return _completedTask;
        }

#endif




        // Statics
        /// <summary>
        /// The singleton instance.
        /// </summary>
        public static NullDisposer Instance { get; } = new NullDisposer();
    }
}
