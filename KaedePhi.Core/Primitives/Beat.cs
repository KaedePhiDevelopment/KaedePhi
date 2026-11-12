using System;
using KaedePhi.Core.Primitives.Mathematics;
using KaedePhi.Core.Primitives.Serialization;
using Newtonsoft.Json;

namespace KaedePhi.Core.Primitives
{
    /// <summary>
    /// 标准节拍格式，表示为 beat[0]:beat[1]/beat[2]。
    /// 使用 float 或 double 隐式转换时，返回 CurBeat = beat[1] / beat[2] + beat[0]。
    /// 使用 int[] 隐式转换时，返回包含三个元素的新数组。
    /// </summary>
    [JsonConverter(typeof(BeatJsonConverter))]
    public readonly struct Beat : IComparable<Beat>, IEquatable<Beat>
    {
        private const int MaxDenominator = 1024;

        private readonly int _whole;
        private readonly int _numerator;
        private readonly int _denominator;
        private readonly double _curBeatDouble;
        private readonly float _curBeatFloat;

        private int EffectiveDenominator => _denominator == 0 ? 1 : _denominator;

        /// <summary>
        /// 初始化拍的新实例。
        /// </summary>
        /// <param name="beatArray">表示拍值的整数数组。</param>
        public Beat(int[] beatArray)
        {
            if (beatArray is null)
                throw new ArgumentNullException(nameof(beatArray));
            if (beatArray.Length != 3)
                throw new ArgumentException(
                    Resources.illegal_beat_number_elements,
                    nameof(beatArray)
                );
            if (beatArray[2] == 0)
                throw new ArgumentException(
                    string.Format(Resources.illegal_beat_denominator, "0"),
                    nameof(beatArray)
                );
            if (beatArray[2] < 0)
                throw new ArgumentException(
                    Resources.illegal_beat_denominator_negative,
                    nameof(beatArray)
                );

            _whole = beatArray[0];
            _numerator = beatArray[1];
            _denominator = beatArray[2];
            _curBeatDouble = (double)_numerator / _denominator + _whole;
            _curBeatFloat = (float)_curBeatDouble;
        }

        /// <summary>
        /// 初始化拍的新实例。
        /// </summary>
        /// <param name="beat">待处理的拍</param>
        public Beat(double beat)
        {
            if (double.IsNaN(beat) || double.IsInfinity(beat))
                throw new ArgumentOutOfRangeException(
                    nameof(beat),
                    Resources.illegal_beat_infinity
                );
            if (beat is < int.MinValue or > int.MaxValue)
                throw new ArgumentOutOfRangeException(
                    nameof(beat),
                    Resources.illegal_beat_out_of_range
                );

            var wholePart = (int)Math.Floor(beat);
            var fractionalPart = beat - wholePart;

            if (Math.Abs(fractionalPart) < 1e-9)
            {
                _whole = wholePart;
                _numerator = 0;
                _denominator = 1;
            }
            else
            {
                int numerator = 1,
                    denominator = 0;
                int prevNumerator = 0,
                    prevDenominator = 1;
                var remaining = fractionalPart;

                for (var iteration = 0; iteration < 20; iteration++)
                {
                    var digit = (int)Math.Floor(remaining);

                    var tempNum = digit * numerator + prevNumerator;
                    var tempDen = digit * denominator + prevDenominator;

                    if (tempDen > MaxDenominator)
                        break;

                    prevNumerator = numerator;
                    prevDenominator = denominator;
                    numerator = tempNum;
                    denominator = tempDen;

                    remaining -= digit;
                    if (
                        Math.Abs(remaining) < Common.CoreConstants.FloatEpsilon
                        || Math.Abs((double)numerator / denominator - fractionalPart)
                        < Common.CoreConstants.FloatEpsilon
                    )
                        break;

                    remaining = 1.0 / remaining;
                }

                if (denominator == 0)
                {
                    denominator = MaxDenominator;
                    numerator = (int)Math.Round(fractionalPart * denominator);
                    var gcd = MathExtensions.Gcd(numerator, denominator);
                    numerator /= gcd;
                    denominator /= gcd;
                }

                _whole = wholePart;
                _numerator = numerator;
                _denominator = denominator;
            }

            _curBeatDouble = (double)_numerator / _denominator + _whole;
            _curBeatFloat = (float)_curBeatDouble;
        }

        private Beat(int whole, int numerator, int denominator)
        {
            _whole = whole;
            _numerator = numerator;
            _denominator = denominator;
            _curBeatDouble = (double)numerator / denominator + whole;
            _curBeatFloat = (float)_curBeatDouble;
        }

        /// <summary>
        /// 获取指定分量；索引零、一、二分别对应整数拍、分子和分母。
        /// </summary>
        /// <param name="index">分量索引</param>
        public int this[int index]
        {
            get
            {
                return index switch
                {
                    0 => _whole,
                    1 => _numerator,
                    2 => EffectiveDenominator,
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(index),
                        index,
                        Resources.illegal_beat_index_out_of_range
                    ),
                };
            }
        }

        // 隐式转换为 float
        /// <summary>
        /// 将拍转换为单精度浮点数。
        /// </summary>
        /// <param name="beat">待处理的拍</param>
        /// <returns>拍的单精度浮点数表示。</returns>
        public static implicit operator float(Beat beat) => beat._curBeatFloat;

        // 隐式转换为 double
        /// <summary>
        /// 将拍转换为双精度浮点数。
        /// </summary>
        /// <param name="beat">待处理的拍</param>
        /// <returns>拍的双精度浮点数表示。</returns>
        public static implicit operator double(Beat beat) => beat._curBeatDouble;

        // 隐式转换为 int[]，返回新数组
        /// <summary>
        /// 将拍转换为包含整数拍、分子和分母的新数组。
        /// </summary>
        /// <param name="beat">待处理的拍</param>
        /// <returns>拍的整数数组表示。</returns>
        public static implicit operator int[](Beat beat) =>
            new[] { beat._whole, beat._numerator, beat.EffectiveDenominator };

        /// <summary>
        /// 将两个拍相加。
        /// </summary>
        /// <param name="a">左侧操作数</param>
        /// <param name="b">右侧操作数</param>
        /// <returns>相加得到的拍。</returns>
        public static Beat operator +(Beat a, Beat b)
        {
            var wholePart = checked(a._whole + b._whole);

            var numerator =
                (long)a._numerator * b.EffectiveDenominator
                + (long)b._numerator * a.EffectiveDenominator;
            var denominator = (long)a.EffectiveDenominator * b.EffectiveDenominator;

            if (numerator >= denominator)
            {
                var carry = numerator / denominator;
                wholePart = checked(wholePart + (int)carry);
                numerator %= denominator;
            }

            if (numerator < 0)
            {
                var borrowCount = (-numerator + denominator - 1) / denominator;
                wholePart = checked(wholePart - (int)borrowCount);
                numerator += borrowCount * denominator;
            }

            if (numerator == 0)
                return new Beat(wholePart, 0, 1);

            var gcd = MathExtensions.Gcd(Math.Abs(numerator), denominator);
            numerator /= gcd;
            denominator /= gcd;

            if (numerator > int.MaxValue || denominator > int.MaxValue)
                throw new OverflowException(
                    "Beat calculation resulted in values too large for int representation."
                );

            return new Beat(wholePart, (int)numerator, (int)denominator);
        }

        /// <summary>
        /// 将右侧拍从左侧拍中减去。
        /// </summary>
        /// <param name="a">左侧操作数</param>
        /// <param name="b">右侧操作数</param>
        /// <returns>相减得到的拍。</returns>
        public static Beat operator -(Beat a, Beat b)
        {
            var wholePart = checked(a._whole - b._whole);

            var numerator =
                (long)a._numerator * b.EffectiveDenominator
                - (long)b._numerator * a.EffectiveDenominator;
            var denominator = (long)a.EffectiveDenominator * b.EffectiveDenominator;

            if (numerator < 0)
            {
                var borrowCount = (-numerator + denominator - 1) / denominator;
                wholePart = checked(wholePart - (int)borrowCount);
                numerator += borrowCount * denominator;
            }

            if (numerator == 0)
                return new Beat(wholePart, 0, 1);

            var gcd = MathExtensions.Gcd(Math.Abs(numerator), denominator);
            numerator /= gcd;
            denominator /= gcd;

            if (numerator > int.MaxValue || denominator > int.MaxValue)
                throw new OverflowException(
                    "Beat calculation resulted in values too large for int representation."
                );

            return new Beat(wholePart, (int)numerator, (int)denominator);
        }

        /// <summary>
        /// 比较左侧拍是否小于右侧拍。
        /// </summary>
        /// <param name="a">左侧操作数</param>
        /// <param name="b">右侧操作数</param>
        /// <returns>如果比较条件成立，则为<see langword="true"/>；否则为<see langword="false"/>。</returns>
        public static bool operator <(Beat a, Beat b) => a._curBeatDouble < b._curBeatDouble;

        /// <summary>
        /// 比较左侧拍是否大于右侧拍。
        /// </summary>
        /// <param name="a">左侧操作数</param>
        /// <param name="b">右侧操作数</param>
        /// <returns>如果比较条件成立，则为<see langword="true"/>；否则为<see langword="false"/>。</returns>
        public static bool operator >(Beat a, Beat b) => a._curBeatDouble > b._curBeatDouble;

        /// <summary>
        /// 比较左侧拍是否小于或等于右侧拍。
        /// </summary>
        /// <param name="a">左侧操作数</param>
        /// <param name="b">右侧操作数</param>
        /// <returns>如果比较条件成立，则为<see langword="true"/>；否则为<see langword="false"/>。</returns>
        public static bool operator <=(Beat a, Beat b) => a._curBeatDouble <= b._curBeatDouble;

        /// <summary>
        /// 比较左侧拍是否大于或等于右侧拍。
        /// </summary>
        /// <param name="a">左侧操作数</param>
        /// <param name="b">右侧操作数</param>
        /// <returns>如果比较条件成立，则为<see langword="true"/>；否则为<see langword="false"/>。</returns>
        public static bool operator >=(Beat a, Beat b) => a._curBeatDouble >= b._curBeatDouble;

        /// <summary>
        /// 判断两个拍是否相等。
        /// </summary>
        /// <param name="a">左侧操作数</param>
        /// <param name="b">右侧操作数</param>
        /// <returns>如果比较条件成立，则为<see langword="true"/>；否则为<see langword="false"/>。</returns>
        public static bool operator ==(Beat a, Beat b) => a._curBeatDouble.Equals(b._curBeatDouble);

        /// <summary>
        /// 判断两个拍是否不相等。
        /// </summary>
        /// <param name="a">左侧操作数</param>
        /// <param name="b">右侧操作数</param>
        /// <returns>如果比较条件成立，则为<see langword="true"/>；否则为<see langword="false"/>。</returns>
        public static bool operator !=(Beat a, Beat b) =>
            !a._curBeatDouble.Equals(b._curBeatDouble);

        /// <summary>
        /// 返回当前对象的字符串表示。
        /// </summary>
        /// <returns>当前对象的字符串表示。</returns>
        public override string ToString() => $"{_whole}:{_numerator}/{EffectiveDenominator}";

        /// <summary>
        /// 确定当前对象是否等于指定对象。
        /// </summary>
        /// <param name="obj">待比较的对象</param>
        /// <returns>如果当前对象与指定对象相等，则为<see langword="true"/>；否则为<see langword="false"/>。</returns>
        public override bool Equals(object? obj) => obj is Beat other && Equals(other);

        /// <summary>
        /// 确定当前对象是否等于指定对象。
        /// </summary>
        /// <param name="other">用于比较的对象</param>
        /// <returns>如果当前对象与指定对象相等，则为<see langword="true"/>；否则为<see langword="false"/>。</returns>
        public bool Equals(Beat other) => _curBeatDouble.Equals(other._curBeatDouble);

        /// <summary>
        /// 返回当前对象的哈希代码。
        /// </summary>
        /// <returns>当前对象的哈希代码。</returns>
        public override int GetHashCode() => _curBeatDouble.GetHashCode();

        /// <summary>
        /// 比较当前对象与指定对象的排序顺序。
        /// </summary>
        /// <param name="other">用于比较的对象</param>
        /// <returns>比较结果；负值表示当前对象小于参数，零表示相等，正值表示当前对象大于参数。</returns>
        public int CompareTo(Beat other) => _curBeatDouble.CompareTo(other._curBeatDouble);

        /// <summary>
        /// 获取表示无限拍的静态实例。
        /// </summary>
        public static Beat Infinity => new(new[] { int.MaxValue, 0, 1 });

        /// <summary>
        /// 判断给定的拍是否表示无限拍。
        /// </summary>
        /// <param name="beat">待判断的拍</param>
        /// <returns>如果给定的拍表示无限拍，则为<see langword="true"/>；否则为<see langword="false"/>。</returns>
        public static bool IsInfinity(Beat beat) =>
            beat is { _whole: int.MaxValue, _numerator: 0, EffectiveDenominator: 1 };
    }
}