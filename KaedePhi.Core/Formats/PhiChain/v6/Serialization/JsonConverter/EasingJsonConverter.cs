using System;
using KaedePhi.Core.Formats.PhiChain.v6.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KaedePhi.Core.Formats.PhiChain.v6.Serialization.JsonConverter
{
    /// <summary>
    /// 提供缓动JSON转换功能。
    /// </summary>
    public sealed class EasingJsonConverter : JsonConverter<Easing>
    {
        /// <summary>
        /// 将指定数据写入目标。
        /// </summary>
        /// <param name="writer">JSON 写入器</param>
        /// <param name="value">待处理的数值</param>
        /// <param name="serializer">序列化器</param>
        public override void WriteJson(JsonWriter writer, Easing? value, JsonSerializer serializer)
        {
            var obj = new JObject
            {
                ["type"] = ToTypeString(
                    value?.EasingType ?? throw new InvalidOperationException("kind is null")
                ),
            };

            switch (value.EasingType)
            {
                case EasingKind.Custom:
                    obj["x1"] = value.X1;
                    obj["y1"] = value.Y1;
                    obj["x2"] = value.X2;
                    obj["y2"] = value.Y2;
                    break;
                case EasingKind.Steps:
                    obj["count"] = value.Count;
                    break;
                case EasingKind.Elastic:
                    obj["omega"] = value.Omega;
                    break;
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
        public override Easing ReadJson(
            JsonReader reader,
            Type objectType,
            Easing? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
        )
        {
            var obj = JObject.Load(reader);
            var easing = existingValue ?? new Easing();
            easing.EasingType = ParseType(
                obj.Value<string>("type")
                    ?? throw new JsonSerializationException("Missing 'type' property for Easing.")
            );

            if (easing.EasingType == EasingKind.Custom)
            {
                easing.X1 = obj.Value<float?>("x1") ?? 0f;
                easing.Y1 = obj.Value<float?>("y1") ?? 0f;
                easing.X2 = obj.Value<float?>("x2") ?? 0f;
                easing.Y2 = obj.Value<float?>("y2") ?? 0f;
            }
            else if (easing.EasingType == EasingKind.Steps)
            {
                easing.Count = obj.Value<int?>("count") ?? 0;
            }
            else if (easing.EasingType == EasingKind.Elastic)
            {
                easing.Omega = obj.Value<float?>("omega") ?? 0f;
            }

            return easing;
        }

        private static string ToTypeString(EasingKind kind)
        {
            return kind switch
            {
                EasingKind.Linear => "linear",
                EasingKind.EaseInSine => "ease_in_sine",
                EasingKind.EaseOutSine => "ease_out_sine",
                EasingKind.EaseInOutSine => "ease_in_out_sine",
                EasingKind.EaseInQuad => "ease_in_quad",
                EasingKind.EaseOutQuad => "ease_out_quad",
                EasingKind.EaseInOutQuad => "ease_in_out_quad",
                EasingKind.EaseInCubic => "ease_in_cubic",
                EasingKind.EaseOutCubic => "ease_out_cubic",
                EasingKind.EaseInOutCubic => "ease_in_out_cubic",
                EasingKind.EaseInQuart => "ease_in_quart",
                EasingKind.EaseOutQuart => "ease_out_quart",
                EasingKind.EaseInOutQuart => "ease_in_out_quart",
                EasingKind.EaseInQuint => "ease_in_quint",
                EasingKind.EaseOutQuint => "ease_out_quint",
                EasingKind.EaseInOutQuint => "ease_in_out_quint",
                EasingKind.EaseInExpo => "ease_in_expo",
                EasingKind.EaseOutExpo => "ease_out_expo",
                EasingKind.EaseInOutExpo => "ease_in_out_expo",
                EasingKind.EaseInCirc => "ease_in_circ",
                EasingKind.EaseOutCirc => "ease_out_circ",
                EasingKind.EaseInOutCirc => "ease_in_out_circ",
                EasingKind.EaseInBack => "ease_in_back",
                EasingKind.EaseOutBack => "ease_out_back",
                EasingKind.EaseInOutBack => "ease_in_out_back",
                EasingKind.EaseInElastic => "ease_in_elastic",
                EasingKind.EaseOutElastic => "ease_out_elastic",
                EasingKind.EaseInOutElastic => "ease_in_out_elastic",
                EasingKind.EaseInBounce => "ease_in_bounce",
                EasingKind.EaseOutBounce => "ease_out_bounce",
                EasingKind.EaseInOutBounce => "ease_in_out_bounce",
                EasingKind.Custom => "custom",
                EasingKind.Steps => "steps",
                EasingKind.Elastic => "elastic",
                _ => throw new JsonSerializationException("Unknown easing type."),
            };
        }

        private static EasingKind ParseType(string type)
        {
            return type switch
            {
                "linear" => EasingKind.Linear,
                "ease_in_sine" => EasingKind.EaseInSine,
                "ease_out_sine" => EasingKind.EaseOutSine,
                "ease_in_out_sine" => EasingKind.EaseInOutSine,
                "ease_in_quad" => EasingKind.EaseInQuad,
                "ease_out_quad" => EasingKind.EaseOutQuad,
                "ease_in_out_quad" => EasingKind.EaseInOutQuad,
                "ease_in_cubic" => EasingKind.EaseInCubic,
                "ease_out_cubic" => EasingKind.EaseOutCubic,
                "ease_in_out_cubic" => EasingKind.EaseInOutCubic,
                "ease_in_quart" => EasingKind.EaseInQuart,
                "ease_out_quart" => EasingKind.EaseOutQuart,
                "ease_in_out_quart" => EasingKind.EaseInOutQuart,
                "ease_in_quint" => EasingKind.EaseInQuint,
                "ease_out_quint" => EasingKind.EaseOutQuint,
                "ease_in_out_quint" => EasingKind.EaseInOutQuint,
                "ease_in_expo" => EasingKind.EaseInExpo,
                "ease_out_expo" => EasingKind.EaseOutExpo,
                "ease_in_out_expo" => EasingKind.EaseInOutExpo,
                "ease_in_circ" => EasingKind.EaseInCirc,
                "ease_out_circ" => EasingKind.EaseOutCirc,
                "ease_in_out_circ" => EasingKind.EaseInOutCirc,
                "ease_in_back" => EasingKind.EaseInBack,
                "ease_out_back" => EasingKind.EaseOutBack,
                "ease_in_out_back" => EasingKind.EaseInOutBack,
                "ease_in_elastic" => EasingKind.EaseInElastic,
                "ease_out_elastic" => EasingKind.EaseOutElastic,
                "ease_in_out_elastic" => EasingKind.EaseInOutElastic,
                "ease_in_bounce" => EasingKind.EaseInBounce,
                "ease_out_bounce" => EasingKind.EaseOutBounce,
                "ease_in_out_bounce" => EasingKind.EaseInOutBounce,
                "custom" => EasingKind.Custom,
                "steps" => EasingKind.Steps,
                "elastic" => EasingKind.Elastic,
                _ => throw new JsonSerializationException("Unsupported easing type: " + type),
            };
        }
    }
}
