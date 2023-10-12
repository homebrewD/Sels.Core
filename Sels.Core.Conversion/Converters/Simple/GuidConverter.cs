﻿using Sels.Core.Conversion.Templates;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Collections;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;

namespace Sels.Core.Conversion.Converters.Simple
{
    /// <summary>
    /// Converts between guids and strings.
    /// </summary>
    public class GuidConverter : BaseTypeConverter
    {
        /// <summary>
        /// The argument for providing a custom guid format.
        /// </summary>
        public const string FormatArgument = "Guid.Format";

        /// <inheritdoc/>
        protected override bool CanConvertObject(object value, Type convertType, IReadOnlyDictionary<string, object> arguments = null)
        {
            return AreTypePair<string, Guid>(value.GetType(), convertType);
        }
        /// <inheritdoc/>
        protected override object ConvertObjectTo(object value, Type convertType, IReadOnlyDictionary<string, object> arguments = null)
        {
            convertType = Nullable.GetUnderlyingType(convertType) ?? convertType;

            // Converting string to guid
            if (convertType.Is<Guid>())
            {
                return new Guid(value.ToString());
            }
            // Converting guid to string
            else
            {
                return value.CastTo<Guid>().ToString(arguments.HasValue() && arguments.TryGetValue<string>(FormatArgument, out var format) ? format : string.Empty);
            }            
        }
    }
}
