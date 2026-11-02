using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KaedePhi.Core.PhiChain.v6.JsonConverter
{
    /// <summary>
    /// 提供节拍速度列表JSON转换功能。
    /// </summary>
    public sealed class BpmListJsonConverter : JsonConverter<BpmList>
    {
        /// <summary>
        /// 将指定数据写入目标。
        /// </summary>
        /// <param name="writer">JSON 写入器</param>
        /// <param name="value">待处理的数值</param>
        /// <param name="serializer">序列化器</param>
        public override void WriteJson(JsonWriter writer, BpmList? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartArray();
            foreach (var point in value)
            {
                serializer.Serialize(writer, point);
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
        /// <returns>操作得到的结果。</returns>
        public override BpmList ReadJson(
            JsonReader reader,
            Type objectType,
            BpmList? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
        )
        {
            var array = JArray.Load(reader);
            var points = array.ToObject<List<BpmPoint>>(serializer) ?? new List<BpmPoint>();
            var list = new BpmList(points);
            list.ComputeTimes();
            return list;
        }
    }
}
