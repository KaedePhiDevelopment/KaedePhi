using System;
using KaedePhi.Core.Primitives;
using Newtonsoft.Json;

namespace KaedePhi.Core.Formats.RePhiEdit.Serialization.JsonConverter
{
    /// <summary>
    /// 用于转换音符类型枚举的转换器
    /// </summary>
    public class NoteTypeConverter : JsonConverter<NoteType>
    {
        /// <summary>
        /// 将指定数据写入目标。
        /// </summary>
        /// <param name="writer">JSON 写入器</param>
        /// <param name="value">待处理的数值</param>
        /// <param name="serializer">序列化器</param>
        public override void WriteJson(JsonWriter writer, NoteType value, JsonSerializer serializer)
        {
            writer.WriteValue((int)value);
        }

        /// <summary>
        /// 从输入源读取JSON。
        /// </summary>
        /// <param name="reader">JSON 读取器</param>
        /// <param name="objectType">目标类型</param>
        /// <param name="existingValue">现有对象值</param>
        /// <param name="hasExistingValue">是否存在现有对象值</param>
        /// <param name="serializer">序列化器</param>
        /// <returns>操作得到的结果。</returns>
        public override NoteType ReadJson(
            JsonReader reader,
            Type objectType,
            NoteType existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
        )
        {
            if (reader.Value is long longValue)
                return (NoteType)longValue;
            if (reader.Value is int intValue)
                return (NoteType)intValue;
            return existingValue;
        }
    }
}
