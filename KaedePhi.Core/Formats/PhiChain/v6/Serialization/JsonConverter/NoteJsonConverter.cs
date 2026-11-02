using System;
using KaedePhi.Core.Formats.PhiChain.v6.Model;
using KaedePhi.Core.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NoteType = KaedePhi.Core.Formats.PhiChain.v6.Model.NoteType;

namespace KaedePhi.Core.Formats.PhiChain.v6.Serialization.JsonConverter
{
    /// <summary>
    /// 提供音符JSON转换功能。
    /// </summary>
    public sealed class NoteJsonConverter : JsonConverter<Note>
    {
        /// <summary>
        /// 将指定数据写入目标。
        /// </summary>
        /// <param name="writer">JSON 写入器</param>
        /// <param name="value">待处理的数值</param>
        /// <param name="serializer">序列化器</param>
        public override void WriteJson(JsonWriter writer, Note? value, JsonSerializer serializer)
        {
            var obj = new JObject
            {
                ["kind"] = ToKindString(
                    value?.Type
                        ?? throw new JsonSerializationException("Note value cannot be null.")
                ),
                ["above"] = value.Above,
                ["beat"] = JToken.FromObject(value.Beat, serializer),
                ["x"] = value.X,
                ["speed"] = value.Speed,
            };

            if (value.Type == NoteType.Hold)
            {
                obj["hold_beat"] = JToken.FromObject(value.HoldBeat, serializer);
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
        public override Note ReadJson(
            JsonReader reader,
            Type objectType,
            Note? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
        )
        {
            var obj = JObject.Load(reader);
            var note = existingValue ?? new Note();

            note.Type = ParseKind(
                obj.Value<string>("kind")
                    ?? throw new JsonSerializationException("Note kind is required.")
            );
            note.Above = obj.Value<bool?>("above") ?? false;
            note.Beat = obj["beat"]?.ToObject<Beat>(serializer) ?? new Beat(new[] { 0, 0, 1 });
            note.X = obj.Value<float?>("x") ?? 0f;
            note.Speed = obj.Value<float?>("speed") ?? 1f;

            if (note.Type == NoteType.Hold)
            {
                var holdBeatToken = obj["hold_beat"];
                if (holdBeatToken is null || holdBeatToken.Type == JTokenType.Null)
                    throw new JsonSerializationException("Hold 音符缺少持续拍。");

                var holdBeat = holdBeatToken.ToObject<Beat>(serializer);
                if (holdBeat <= new Beat(0))
                    throw new JsonSerializationException("Hold 音符的持续拍必须大于零。");

                note.HoldBeat = holdBeat;
            }
            else
            {
                note.HoldBeat = new Beat(new[] { 0, 0, 1 });
            }

            return note;
        }

        private static string ToKindString(NoteType type)
        {
            switch (type)
            {
                case NoteType.Tap:
                    return "tap";
                case NoteType.Drag:
                    return "drag";
                case NoteType.Hold:
                    return "hold";
                case NoteType.Flick:
                    return "flick";
                default:
                    throw new JsonSerializationException("Unknown note type.");
            }
        }

        private static NoteType ParseKind(string kind)
        {
            return kind switch
            {
                "tap" => NoteType.Tap,
                "drag" => NoteType.Drag,
                "hold" => NoteType.Hold,
                "flick" => NoteType.Flick,
                _ => throw new JsonSerializationException("Unsupported note type: " + kind),
            };
        }
    }
}
