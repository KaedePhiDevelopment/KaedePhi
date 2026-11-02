using System;
using Newtonsoft.Json;

namespace KaedePhi.Core.Primitives.Serialization
{
    /// <summary>
    /// 提供拍JSON转换功能。
    /// </summary>
    public class BeatJsonConverter : JsonConverter<Beat>
    {
        /// <summary>
        /// 将指定数据写入目标。
        /// </summary>
        /// <param name="writer">JSON 写入器</param>
        /// <param name="value">待处理的数值</param>
        /// <param name="serializer">序列化器</param>
        public override void WriteJson(JsonWriter writer, Beat value, JsonSerializer serializer)
        {
            // 将 Beat 序列化为 int[] 数组
            int[] beatArray = value;
            serializer.Serialize(writer, beatArray);
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
        public override Beat ReadJson(
            JsonReader reader,
            Type objectType,
            Beat existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
        )
        {
            if (reader.TokenType == JsonToken.Null)
                return default;

            var beatArray = serializer.Deserialize<int[]>(reader);
            if (beatArray is null)
                return default;

            return new Beat(beatArray);
        }
    }
}
