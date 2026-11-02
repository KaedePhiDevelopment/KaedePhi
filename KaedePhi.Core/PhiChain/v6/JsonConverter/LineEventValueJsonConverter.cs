using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KaedePhi.Core.PhiChain.v6.JsonConverter
{
    /// <summary>
    /// 提供判定线事件数值JSON转换功能。
    /// </summary>
    public sealed class LineEventValueJsonConverter : JsonConverter<LineEventValue>
    {
        /// <summary>
        /// 将指定数据写入目标。
        /// </summary>
        /// <param name="writer">JSON 写入器</param>
        /// <param name="value">待处理的数值</param>
        /// <param name="serializer">序列化器</param>
        public override void WriteJson(
            JsonWriter writer,
            LineEventValue? value,
            JsonSerializer serializer
        )
        {
            var obj = new JObject
            {
                ["type"] = value?.Type == LineEventValueType.Constant ? "constant" : "transition",
            };

            if (value?.Type == LineEventValueType.Constant)
            {
                obj["value"] = value.Value;
            }
            else
            {
                obj["start"] = value?.Start;
                obj["end"] = value?.End;
                obj["easing"] = JToken.FromObject(
                    value?.Easing
                        ?? throw new InvalidOperationException("Transition type must have easing"),
                    serializer
                );
            }

            obj.WriteTo(writer);
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
        public override LineEventValue ReadJson(
            JsonReader reader,
            Type objectType,
            LineEventValue? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
        )
        {
            var obj = JObject.Load(reader);
            var type = obj.Value<string>("type");

            if (type == "constant")
            {
                return LineEventValue.Constant(obj.Value<float?>("value") ?? 0f);
            }

            if (type == "transition")
            {
                return LineEventValue.Transition(
                    obj.Value<float?>("start") ?? 0f,
                    obj.Value<float?>("end") ?? 0f,
                    obj["easing"]?.ToObject<Easing>(serializer) ?? Easing.Linear
                );
            }

            throw new JsonSerializationException("Unsupported line event value type: " + type);
        }
    }
}
