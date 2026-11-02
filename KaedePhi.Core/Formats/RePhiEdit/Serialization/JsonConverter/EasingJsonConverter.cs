using System;
using KaedePhi.Core.Formats.RePhiEdit.Model;
using Newtonsoft.Json;

namespace KaedePhi.Core.Formats.RePhiEdit.Serialization.JsonConverter
{
    /// <summary>
    /// 提供缓动JSON转换功能。
    /// </summary>
    public class EasingJsonConverter : JsonConverter<Easing>
    {
        /// <summary>
        /// 将指定数据写入目标。
        /// </summary>
        /// <param name="writer">JSON 写入器</param>
        /// <param name="value">待处理的数值</param>
        /// <param name="serializer">序列化器</param>
        public override void WriteJson(JsonWriter writer, Easing? value, JsonSerializer serializer)
        {
            if (value is null)
                return;
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
        public override Easing ReadJson(
            JsonReader reader,
            Type objectType,
            Easing? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
        )
        {
            var easingNumber = serializer.Deserialize<int>(reader);
            return new Easing(easingNumber);
        }
    }
}
