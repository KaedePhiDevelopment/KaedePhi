namespace KaedePhi.Core.Utils
{
    /// <summary>
    /// 提供数学扩展方法。
    /// </summary>
    public static class MathExtensions
    {
        /// <summary>
        /// 计算两个整数的最大公约数。
        /// </summary>
        /// <param name="a">左操作数</param>
        /// <param name="b">右操作数</param>
        /// <returns>最大公约数</returns>
        public static int Gcd(int a, int b)
        {
            while (b != 0)
            {
                var temp = b;
                b = a % b;
                a = temp;
            }

            return a;
        }
        
        /// <summary>
        /// 计算两个长整数的最大公约数。
        /// </summary>
        /// <param name="a">左操作数</param>
        /// <param name="b">右操作数</param>
        /// <returns>最大公约数</returns>
        public static long Gcd(long a, long b)
        {
            while (b != 0)
            {
                var temp = b;
                b = a % b;
                a = temp;
            }

            return a;
        }
    }
}