using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KaedePhi.Core.Formats
{
    /// <summary>
    /// 定义谱面格式的流式读取和写入能力。
    /// </summary>
    /// <typeparam name="TChart">该格式的谱面类型。</typeparam>
    public interface IChartSerializer<TChart>
    {
        /// <summary>
        /// 从输入流读取谱面，不关闭输入流。
        /// </summary>
        /// <param name="stream">谱面输入流。</param>
        /// <param name="ct">取消令牌。</param>
        /// <returns>读取到的谱面。</returns>
        Task<TChart> ReadAsync(Stream stream, CancellationToken ct = default);

        /// <summary>
        /// 将谱面写入输出流，不关闭输出流。
        /// </summary>
        /// <param name="chart">待写入的谱面。</param>
        /// <param name="stream">谱面输出流。</param>
        /// <param name="indented">是否格式化输出；不支持格式化的格式可以忽略。</param>
        /// <param name="ct">取消令牌。</param>
        /// <returns>写入任务。</returns>
        Task WriteAsync(TChart chart, Stream stream, bool indented, CancellationToken ct = default);
    }
}
