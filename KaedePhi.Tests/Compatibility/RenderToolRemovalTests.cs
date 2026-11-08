using System.Reflection;
using System.Runtime.CompilerServices;
using KaedePhi.Tool.Common;

namespace KaedePhi.Tests.Compatibility;

public sealed class RenderToolRemovalTests
{
    private static readonly Assembly ToolAssembly = typeof(IrChartValidator).Assembly;

    // 渲染工具类型已被标记为编译期错误，测试只能通过反射验证旧入口
    [Theory]
    [InlineData("KaedePhi.Tool.Render.IChartRenderExporter`2")]
    [InlineData("KaedePhi.Tool.Render.Intermediate.IrChartRenderExporter")]
    [InlineData("KaedePhi.Tool.Render.Intermediate.IrEventLayerRenderer")]
    [InlineData("KaedePhi.Tool.Render.Intermediate.IrRenderOptions")]
    [InlineData("KaedePhi.Tool.Render.KaedePhi.KpcChartRenderExporter")]
    [InlineData("KaedePhi.Tool.Render.KaedePhi.KpcEventLayerRenderer")]
    [InlineData("KaedePhi.Tool.Render.KaedePhi.KpcRenderOptions")]
    [InlineData("KaedePhi.Tool.Common.IrRenderValidator")]
    [InlineData("KaedePhi.Tool.Common.KpcRenderValidator")]
    public void RemovedRenderTypes_AreObsoleteErrorsAndStayPublic(string typeName)
    {
        var type = GetRemovedType(typeName);

        type.IsPublic.Should().BeTrue();
        var obsolete = type.GetCustomAttribute<ObsoleteAttribute>();
        obsolete.Should().NotBeNull();
        obsolete!.IsError.Should().BeTrue();
        obsolete.Message.Should().Contain("渲染工具已被彻底移除");
    }

    [Theory]
    [InlineData("KaedePhi.Tool.Render.Intermediate.IrChartRenderExporter")]
    [InlineData("KaedePhi.Tool.Render.Intermediate.IrEventLayerRenderer")]
    [InlineData("KaedePhi.Tool.Render.Intermediate.IrRenderOptions")]
    [InlineData("KaedePhi.Tool.Render.KaedePhi.KpcChartRenderExporter")]
    [InlineData("KaedePhi.Tool.Render.KaedePhi.KpcEventLayerRenderer")]
    [InlineData("KaedePhi.Tool.Common.IrRenderValidator")]
    [InlineData("KaedePhi.Tool.Common.KpcRenderValidator")]
    public void RemovedRenderTypes_PublicMembersAlwaysThrowNotImplemented(string typeName)
    {
        var type = GetRemovedType(typeName);
        var methods = type.GetMethods(
                BindingFlags.Public
                    | BindingFlags.Instance
                    | BindingFlags.Static
                    | BindingFlags.DeclaredOnly
            )
            .ToList();
        methods.Should().NotBeEmpty();

        foreach (var method in methods)
        {
            // 绕过必然抛出异常的构造函数，使实例成员可以被直接调用
            var target = method.IsStatic ? null : RuntimeHelpers.GetUninitializedObject(type);
            var arguments = method
                .GetParameters()
                .Select(parameter =>
                    parameter.ParameterType.IsValueType
                        ? Activator.CreateInstance(parameter.ParameterType)
                        : null
                )
                .ToArray();

            Action act = () => method.Invoke(target, arguments);

            act.Should()
                .Throw<TargetInvocationException>(because: method.Name)
                .WithInnerException<NotImplementedException>()
                .WithMessage("*渲染工具已被彻底移除*");
        }
    }

    [Fact]
    public void RemovedRenderOptions_CannotBeConstructed()
    {
        var constructor = GetRemovedType("KaedePhi.Tool.Render.Intermediate.IrRenderOptions")
            .GetConstructor(Type.EmptyTypes)!;

        Action act = () => constructor.Invoke(null);

        act.Should()
            .Throw<TargetInvocationException>()
            .WithInnerException<NotImplementedException>()
            .WithMessage("*渲染工具已被彻底移除*");
    }

    private static Type GetRemovedType(string typeName) =>
        ToolAssembly.GetType(typeName, throwOnError: true)!;
}
