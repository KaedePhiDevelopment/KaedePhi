using System;
using Newtonsoft.Json;

namespace KaedePhi.Core.Formats.RePhiEdit.Serialization.JsonConverter
{
    /// <summary>
    /// 用于将cmdysj在JSON中表示为0和1的布尔值进行转换
    /// </summary>
    public class BoolConverter : JsonConverter<bool>
    {
        /// <summary>
        /// 将指定数据写入目标。
        /// </summary>
        /// <param name="writer">JSON 写入器</param>
        /// <param name="value">待处理的数值</param>
        /// <param name="serializer">序列化器</param>
        public override void WriteJson(JsonWriter writer, bool value, JsonSerializer serializer)
        {
            writer.WriteValue(value ? 1 : 0);
        }

        /// <summary>
        /// 从输入源读取JSON。
        /// </summary>
        /// <param name="reader">JSON 读取器</param>
        /// <param name="objectType">目标类型</param>
        /// <param name="existingValue">现有对象值</param>
        /// <param name="hasExistingValue">是否存在现有对象值</param>
        /// <param name="serializer">序列化器</param>
        /// <returns>如果条件成立，则为<see langword="true"/>；否则为<see langword="false"/>。</returns>
        public override bool ReadJson(
            JsonReader reader,
            Type objectType,
            bool existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
        )
        {
            if (reader.Value is long longValue)
            {
                return longValue == 1;
            }

            if (reader.Value is int intValue)
            {
                return intValue == 1;
            }

            return existingValue;
        }
    }
}
