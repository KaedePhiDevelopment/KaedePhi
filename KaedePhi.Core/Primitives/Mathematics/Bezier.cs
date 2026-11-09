using System;

namespace KaedePhi.Core.Primitives.Mathematics
{
    /// <summary>
    /// 表示贝塞尔曲线数据。
    /// </summary>
    public static class Bezier
    {
        /// <summary>
        /// 三次贝塞尔缓动插值。
        /// points = [x1, y1, x2, y2]，隐含端点 (0,0) → (1,1)。
        /// 控制点可以是负数或大于1，产生过冲/欠冲效果。
        /// </summary>
        public static T Do<T>(
            float[] points,
            float t,
            T startValue,
            T endValue,
            float left = 0.0f,
            float right = 1.0f
        )
            where T : struct, IComparable, IFormattable, IConvertible
        {
            if (points == null || points.Length < 4)
                throw new ArgumentException("points 需要至少 4 个元素 [x1, y1, x2, y2]");

            var type = typeof(T);
            if (
                type != typeof(float)
                && type != typeof(double)
                && type != typeof(int)
                && type != typeof(byte)
            )
                throw new NotSupportedException("T必须是float、double、int或byte");

            float x1 = points[0],
                y1 = points[1];
            float x2 = points[2],
                y2 = points[3];

            // t → 映射到参数区间
            var mappedT = left + t * (right - left);

            switch (mappedT)
            {
                // 边界快速返回
                case <= 0f:
                    return startValue;
                case >= 1f:
                    return endValue;
            }

            // 从 x(u) = mappedT 反解参数 u
            var u = SolveU(x1, x2, mappedT);

            // 保留过冲
            var easing = SampleCurveY(u, y1, y2);

            if (type == typeof(float))
            {
                var s = (float)(object)startValue;
                var e = (float)(object)endValue;
                return (T)(object)(s + easing * (e - s));
            }

            if (type == typeof(double))
            {
                var s = (double)(object)startValue;
                var e = (double)(object)endValue;
                return (T)(object)(s + easing * (e - s));
            }

            if (type == typeof(int))
            {
                var s = (int)(object)startValue;
                var e = (int)(object)endValue;
                return (T)(object)(int)(s + easing * (e - s));
            }

            if (type == typeof(byte))
            {
                var s = (byte)(object)startValue;
                var e = (byte)(object)endValue;
                return (T)(object)(byte)(s + easing * (e - s));
            }

            throw new NotSupportedException("T必须是float、double、int或byte");
        }

        /// <summary>
        /// 计算三次贝塞尔缓动曲线在指定进度处的缓动值。
        /// 进度小于 0 或大于 1 时沿曲线延伸，不会被截断为端点值。
        /// </summary>
        /// <param name="points">控制点数组 [x1, y1, x2, y2]，隐含端点 (0,0) → (1,1)。</param>
        /// <param name="progress">输入进度，允许小于 0 或大于 1。</param>
        /// <returns>缓动值；控制点超出 [0, 1] 时会产生过冲。</returns>
        public static double Evaluate(float[] points, double progress)
        {
            if (points == null || points.Length < 4)
                throw new ArgumentException("points 需要至少 4 个元素 [x1, y1, x2, y2]");

            var x = (float)progress;
            var u =
                x < 0f || x > 1f
                    ? SolveExtendedU(points[0], points[2], x)
                    : SolveU(points[0], points[2], x);
            return SampleCurveY(u, points[1], points[3]);
        }

        #region 核心数学

        /// <summary>
        /// 牛顿迭代 + 二分法兜底，求 u 使得 x(u) = targetX
        /// </summary>
        private static float SolveU(float x1, float x2, float targetX)
        {
            if (TryNewtonIterate(x1, x2, targetX, out var u))
                return Math.Clamp(u, 0f, 1f);

            var (uLo, uHi) = FindBracket(x1, x2, targetX);
            return RefineByBisection(x1, x2, targetX, uLo, uHi);
        }

        /// <summary>
        /// 目标进度位于 [0,1] 之外时，从最近的端点沿曲线向外扫描，
        /// 在第一个跨越目标的区间内二分求 u；扫描范围内无解时返回端点参数。
        /// </summary>
        private static float SolveExtendedU(float x1, float x2, float targetX)
        {
            const int maxSteps = 512;
            const float step = 1f / 32f;
            var direction = targetX > 1f ? 1f : -1f;
            var origin = targetX > 1f ? 1f : 0f;
            var previousU = origin;
            var previousErr = SampleCurveX(previousU, x1, x2) - targetX;

            for (var i = 1; i <= maxSteps; i++)
            {
                var currentU = origin + direction * step * i;
                var currentErr = SampleCurveX(currentU, x1, x2) - targetX;
                if (
                    (previousErr <= 0f && currentErr >= 0f)
                    || (previousErr >= 0f && currentErr <= 0f)
                )
                {
                    var (uLo, uHi) =
                        previousU < currentU ? (previousU, currentU) : (currentU, previousU);
                    return BisectUnclamped(x1, x2, targetX, uLo, uHi);
                }

                previousU = currentU;
                previousErr = currentErr;
            }

            return origin;
        }

        /// <summary>
        /// 在已知跨越目标的区间内二分，不对结果做 [0,1] 截断。
        /// </summary>
        private static float BisectUnclamped(
            float x1,
            float x2,
            float targetX,
            float uLo,
            float uHi
        )
        {
            var increasing = SampleCurveX(uHi, x1, x2) >= SampleCurveX(uLo, x1, x2);
            for (var i = 0; i < 30; i++)
            {
                var u = (uLo + uHi) * 0.5f;
                var err = SampleCurveX(u, x1, x2) - targetX;
                if (MathF.Abs(err) < 1e-7f)
                    return u;

                if (err < 0f == increasing)
                    uLo = u;
                else
                    uHi = u;
            }

            return (uLo + uHi) * 0.5f;
        }

        /// <summary>
        /// 第一阶段：牛顿迭代快速收敛。导数过小时提前退出。
        /// </summary>
        private static bool TryNewtonIterate(float x1, float x2, float targetX, out float u)
        {
            u = targetX;
            for (var i = 0; i < 8; i++)
            {
                var err = SampleCurveX(u, x1, x2) - targetX;
                if (MathF.Abs(err) < 1e-7f)
                    return true;

                var deriv = SampleCurveXDerivative(u, x1, x2);
                if (MathF.Abs(deriv) < 1e-7f)
                    return false; // 导数太小 → 切二分

                u -= err / deriv;
            }

            return false;
        }

        /// <summary>
        /// 第二阶段（上）：在 [0,1] 上均匀采样，定位跨越 targetX 的最右侧区间。
        /// </summary>
        private static (float uLo, float uHi) FindBracket(float x1, float x2, float targetX)
        {
            const int subdivisions = 32;
            const float step = 1f / subdivisions;
            float uLo = 0f,
                uHi = 1f;
            var prevX = 0f;

            for (var i = 1; i <= subdivisions; i++)
            {
                var ui = step * i;
                var xi = SampleCurveX(ui, x1, x2);

                var crosses =
                    (prevX <= targetX && xi >= targetX) || (prevX >= targetX && xi <= targetX);
                if (crosses)
                {
                    uLo = step * (i - 1);
                    uHi = ui;
                }

                prevX = xi;
            }

            return (uLo, uHi);
        }

        /// <summary>
        /// 第二阶段（下）：在已知区间内做精确二分，返回 Clamp 后的结果。
        /// </summary>
        private static float RefineByBisection(
            float x1,
            float x2,
            float targetX,
            float uLo,
            float uHi
        )
        {
            for (var i = 0; i < 20; i++)
            {
                var u = (uLo + uHi) * 0.5f;
                var err = SampleCurveX(u, x1, x2) - targetX;

                if (MathF.Abs(err) < 1e-7f)
                    break;

                if (err < 0f)
                    uLo = u;
                else
                    uHi = u;
            }

            return Math.Clamp((uLo + uHi) * 0.5f, 0f, 1f);
        }

        // x(u) = 3(1−u)²u·x1 + 3(1−u)u²·x2 + u³
        private static float SampleCurveX(float u, float x1, float x2)
        {
            var omu = 1f - u;
            return 3f * omu * omu * u * x1 + 3f * omu * u * u * x2 + u * u * u;
        }

        // y(u) = 3(1−u)²u·y1 + 3(1−u)u²·y2 + u³
        private static float SampleCurveY(float u, float y1, float y2)
        {
            var omu = 1f - u;
            return 3f * omu * omu * u * y1 + 3f * omu * u * u * y2 + u * u * u;
        }

        // x'(u) = 3(1−u)²·x1 + 6(1−u)u·(x2−x1) + 3u²·(1−x2)
        private static float SampleCurveXDerivative(float u, float x1, float x2)
        {
            var omu = 1f - u;
            return 3f * omu * omu * x1 + 6f * omu * u * (x2 - x1) + 3f * u * u * (1f - x2);
        }

        #endregion
    }
}
