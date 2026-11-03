using System.Reflection;
using KaedePhi.Core.Intermediate.Model;
using KaedePhi.Tool.Converter;
using Newtonsoft.Json;

namespace KaedePhi.Tests.Compatibility;

internal static class LegacyApi
{
    internal static Type ToolType(string name) =>
        typeof(ChartFormatDescriptor).Assembly.GetType(name, throwOnError: true)!;

    internal static Type CoreType(string name) =>
        typeof(Chart).Assembly.GetType(name, throwOnError: true)!;

    internal static object? Invoke(Type type, string name, object? instance, params object?[] arguments)
        => Invoke(
            type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Where(method => !method.ContainsGenericParameters),
            name, instance, arguments
        );

    internal static object? InvokeGeneric(
        Type type, string name, Type[] typeArguments, object? instance, params object?[] arguments
    ) => Invoke(
        type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .Where(method => method.Name == name && method.IsGenericMethodDefinition
                && method.GetGenericArguments().Length == typeArguments.Length)
            .Select(method => method.MakeGenericMethod(typeArguments)),
        name, instance, arguments
    );

    private static object? Invoke(
        IEnumerable<MethodInfo> methods, string name, object? instance, object?[] arguments
    )
    {
        var method = methods
            .Single(candidate => candidate.Name == name
                && candidate.GetParameters().Length == arguments.Length
                && candidate.GetParameters().Zip(arguments)
                    .All(pair => pair.Second is null || pair.First.ParameterType.IsInstanceOfType(pair.Second)));
        return method.Invoke(instance, BindingFlags.DoNotWrapExceptions, null, arguments, null);
    }

    internal static async Task<object?> AwaitResult(object? result)
    {
        var task = (Task)result!;
        await task;
        return task.GetType().GetProperty("Result")?.GetValue(task);
    }

    internal static object ConvertTo(object value, Type type) =>
        JsonConvert.DeserializeObject(
            JsonConvert.SerializeObject(value), type,
            new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace }
        )
            ?? throw new InvalidOperationException("兼容测试模型转换失败。");
}
