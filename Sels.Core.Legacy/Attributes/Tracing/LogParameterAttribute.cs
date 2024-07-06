using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Tracing
{
    /// <summary>
    /// Attribute that can be used to mark classes, structs, properties or parameters with a static log parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    public class LogParameterAttribute : Attribute
    {
        // Properties
        /// <summary>
        /// The name of the log parameter to use.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The value for the log parameter.
        /// </summary>
        public object Value { get; set; }

        /// <inheritdoc cref="LogParameterAttribute"/>
        /// <param name="name"><inheritdoc cref="Name"/></param>
        /// <param name="value"><inheritdoc cref="Value"/></param>
        public LogParameterAttribute(string name, object value)
        {
            Name = name.ValidateArgumentNotNullOrWhitespace(nameof(name));
            Value = value;
        }
    }
}
