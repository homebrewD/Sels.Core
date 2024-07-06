using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sels.Core.Tracing
{
    /// <summary>
    /// Attribute that can be used to mark classes, structs, properties or parameters as traceable.
    /// The attribute point to a value to add as a log parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    public class TraceableAttribute : Attribute
    {
        // Properties
        /// <summary>
        /// The name of the log parameter to use.
        /// When set to null the name will be inferred based on what the property is defined on.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The path that points to the (nested) property to needs to be added as value.
        /// Nested properties can be defined by using '.' as separator.
        /// When set to null the object the attribute is defined on will be used as value.
        /// </summary>
        public string Path { get; set; }

        /// <inheritdoc cref="TraceableAttribute"/>
        public TraceableAttribute()
        {
            
        }

        /// <inheritdoc cref="TraceableAttribute"/>
        /// <param name="name"><inheritdoc cref="Name"/></param>
        /// <param name="path"><inheritdoc cref="Path"/></param>
        public TraceableAttribute(string name, string path = null)
        {
            Name = name;
            Path = path;
        }

        /// <summary>
        /// Gets the log parameter name when the attribute is defined on class/struct <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The class or struct the attribute is defined on</param>
        /// <returns>The log parameter name to use as defined by the attribute</returns>
        public virtual string GetLogParameterName(Type type)
        {
            type = type.ValidateArgument(nameof(type));

            if(Name.HasValue())
            {
                return Name;
            }

            return type.FullName ?? type.Name;
        }

        /// <summary>
        /// Gets the log parameter name when the attribute is defined on property <paramref name="property"/>.
        /// </summary>
        /// <param name="property">The property the attribute is defined on</param>
        /// <returns>The log parameter name to use as defined by the attribute</returns>
        public virtual string GetLogParameterName(PropertyInfo property)
        {
            property = property.ValidateArgument(nameof(property));

            if (Name.HasValue())
            {
                return Name;
            }

            return $"{property.ReflectedType.FullName ?? property.ReflectedType.Name ?? property.DeclaringType.FullName ?? property.DeclaringType.Name}.{property.Name}";
        }

        /// <summary>
        /// Gets the log parameter name when the attribute is defined on method parameter <paramref name="parameter"/> of method <paramref name="method"/>.
        /// </summary>
        /// <param name="method">The method <paramref name="parameter"/> is from</param>
        /// <param name="parameter">The method parameter the attribute is defined on</param>
        /// <returns>The log parameter name to use as defined by the attribute</returns>
        public virtual string GetLogParameterName(MethodInfo method, ParameterInfo parameter)
        {
            method = method.ValidateArgument(nameof(method));
            parameter = parameter.ValidateArgument(nameof(parameter));

            if (Name.HasValue())
            {
                return Name;
            }

            var methodName = method.GetDisplayName(MethodDisplayOptions.IncludeParameterNames | MethodDisplayOptions.IncludeType);

            return $"{methodName}:{parameter.Name}";
        }

        /// <summary>
        /// Gets the log parameter name when the attribute is defined on method <paramref name="method"/>.
        /// </summary>
        /// <param name="method">The method the attribute is defined on</param>
        /// <returns>The log parameter name to use as defined by the attribute</returns>
        public virtual string GetLogParameterName(MethodInfo method)
        {
            method = method.ValidateArgument(nameof(method));

            if (Name.HasValue())
            {
                return Name;
            }

            var methodName = method.GetDisplayName(MethodDisplayOptions.IncludeParameterNames | MethodDisplayOptions.IncludeType);

            return $"{methodName}";
        }
    }
}
