using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace KaedePhi.Core.Formats.RePhiEdit.Serialization.JsonConverter
{
    /// <summary>
    /// 提供颜色转换功能。
    /// </summary>
    public class ColorConverter : JsonConverter<byte[]>
    {
        /// <summary>
        /// 将指定数据写入目标。
        /// </summary>
        /// <param name="writer">JSON 写入器</param>
        /// <param name="value">待处理的数值</param>
        /// <param name="serializer">序列化器</param>
        public override void WriteJson(JsonWriter writer, byte[]? value, JsonSerializer serializer)
        {
            if (value is null)
                return;
            writer.WriteStartArray();
            foreach (var b in value)
            {
                writer.WriteValue(b);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// 从输入源读取JSON。
        /// </summary>
        /// <param name="reader">JSON 读取器</param>
        /// <param name="objectType">目标类型</param>
        /// <param name="existingValue">现有对象值</param>
        /// <param name="hasExistingValue">是否存在现有对象值</param>
        /// <param name="serializer">序列化器</param>
        /// <returns>包含处理结果的列表。</returns>
        public override byte[] ReadJson(
            JsonReader reader,
            Type objectType,
            byte[]? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
        )
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                var list = new List<byte>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.EndArray)
                        break;

                    if (reader.TokenType == JsonToken.Integer)
                    {
                        list.Add(Convert.ToByte(reader.Value));
                    }
                }

                return list.ToArray();
            }

            return existingValue ?? new byte[] { 255, 255, 255 };
        }
    }
}
