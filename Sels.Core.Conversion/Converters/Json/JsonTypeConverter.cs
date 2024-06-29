using Newtonsoft.Json;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Conversion.Converters.Json
{
    /// <summary>
    /// Json converter that converts <see cref="System.Type"/>
    /// </summary>
    public class JsonTypeConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override System.Boolean CanConvert(System.Type objectType)
        {
            objectType.ValidateArgument(nameof(objectType));

            return objectType.Is<Type>();
        }
        /// <inheritdoc/>
        public override System.Object ReadJson(JsonReader reader, System.Type objectType, System.Object existingValue, JsonSerializer serializer)
        {
            reader.ValidateArgument(nameof(reader));
            objectType.ValidateArgument(nameof(objectType));
            serializer.ValidateArgument(nameof(serializer));

            if(existingValue is string typeName)
            {
                return Type.GetType(typeName, true);
            }

            return existingValue;
        }
        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, System.Object value, JsonSerializer serializer)
        {
            writer.ValidateArgument(nameof(writer));
            
            if(value is Type type)
            {
                writer.WriteValue(type.AssemblyQualifiedName);
            }
            else
            {
                writer.WriteValue(value?.ToString());
            }

        }
    }
}
