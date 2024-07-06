using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Tracing
{
    /// <summary>
    /// How duplicate log parameter values should be handled.
    /// </summary>
    public enum TraceableConflictHandling
    {
        /// <summary>
        /// Value will only be added if the log parameters doesn't exist yet.
        /// </summary>
        Ignore = 0,
        /// <summary>
        /// Value will be updated if it exists.
        /// </summary>
        Update = 1,
        /// <summary>
        /// Value will be updated if it exists and is null
        /// </summary>
        UpdateIfNull = 2,
        /// <summary>
        /// Value will be updated if it exists and is the default value.
        /// </summary>
        UpdateIfDefault = 3,
        /// <summary>
        /// Throw an exception when adding a duplicate log parameter.
        /// </summary>
        Exception = 4
    }
}
