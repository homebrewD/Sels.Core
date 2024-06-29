using Sels.ObjectValidationFramework.Profile;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.ObjectValidationFramework.Profile
{
    /// <summary>
    /// Attribute that can be put on properties to hint a validator that the property is ignored in validation.
    /// When defined on the class the ignore type will be applied to all properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class IgnoreInValidationAttribute : Attribute
    {
        // Properties
        /// <summary>
        /// For what the property is ignored for.
        /// </summary>
        public IgnoreType IgnoreType { get; } = IgnoreType.All;

        /// <inheritdoc cref="IgnoreInValidationAttribute"/>
        /// <param name="ignoreType"><inheritdoc cref="IgnoreType"/></param>
        public IgnoreInValidationAttribute(IgnoreType ignoreType = IgnoreType.All)
        {
            IgnoreType = ignoreType;
        }
    }
}
