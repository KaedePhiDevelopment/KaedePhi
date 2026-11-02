using System;
using KaedePhi.Core.Primitives;
using Newtonsoft.Json;

namespace KaedePhi.Core.Formats.RePhiEdit.Serialization.JsonConverter
{
    /// <summary>
    /// 提供附加界面转换功能。
    /// </summary>
    public class AttachUiConverter : JsonConverter<AttachUi?>
    {
        /// <summary>
        /// 将指定数据写入目标。
        /// </summary>
        /// <param name="writer">JSON 写入器</param>
        /// <param name="value">待处理的数值</param>
        /// <param name="serializer">序列化器</param>
        public override void WriteJson(
            JsonWriter writer,
            AttachUi? value,
            JsonSerializer serializer
        )
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            switch (value)
            {
                case AttachUi.Bar:
                    writer.WriteValue("bar");
                    break;
                case AttachUi.Combo:
                    writer.WriteValue("combo");
                    break;
                case AttachUi.ComboNumber: // ReSharper disable once StringLiteralTypo
                    writer.WriteValue("combonumber");
                    break;
                case AttachUi.Level:
                    writer.WriteValue("level");
                    break;
                case AttachUi.Name:
                    writer.WriteValue("name");
                    break;
                case AttachUi.Pause:
                    writer.WriteValue("pause");
                    break;
                case AttachUi.Score:
                    writer.WriteValue("score");
                    break;
                default:
                    writer.WriteNull();
                    break;
            }
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
        public override AttachUi? ReadJson(
            JsonReader reader,
            Type objectType,
            AttachUi? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
        )
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            if (reader.TokenType == JsonToken.String)
            {
                var value = reader.Value as string;
                var lowerValue = value?.ToLower();
                return lowerValue switch
                {
                    "bar" => AttachUi.Bar,
                    "combo" => AttachUi.Combo,
                    // ReSharper disable once StringLiteralTypo
                    "combonumber" => AttachUi.ComboNumber,
                    "level" => AttachUi.Level,
                    "name" => AttachUi.Name,
                    "pause" => AttachUi.Pause,
                    "score" => AttachUi.Score,
                    _ => existingValue,
                };
            }

            return existingValue;
        }
    }
}
