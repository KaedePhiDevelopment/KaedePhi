using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace KaedePhi.Tool.Analyzers.Analysis;

/// <summary>
/// 父线解绑 API 的类型识别与容差实参检索。
/// </summary>
internal static class JudgeLineUnbinderApi
{
    // 按命名空间与元数据名称识别目标类型，避免依赖具体程序集引用
    private const string JudgeLineNamespace = "KaedePhi.Tool.JudgeLine";
    private const string ImplementationNamespace = "KaedePhi.Tool.JudgeLine.Intermediate";
    private const string LegacyInterfaceMetadataName = "IJudgeLineUnbinder`1";
    private const string ServiceInterfaceMetadataName = "IJudgeLineUnbindService`1";
    private const string ImplementationMetadataName = "JudgeLineUnbinder";
    private const string ToleranceParameterName = "tolerance";
    private const string MergeToleranceParameterName = "mergeTolerance";

    /// <summary>
    /// 尝试从调用中检索全部受支持的容差值。
    /// </summary>
    /// <param name="invocation">待检查的调用操作</param>
    /// <param name="values">匹配到的容差值集合</param>
    /// <returns>是否找到至少一个容差实参</returns>
    public static bool TryGetToleranceValues(
        IInvocationOperation invocation,
        out ImmutableArray<(IOperation Value, string Name)> values
    )
    {
        if (IsServiceUnbindMethod(invocation.TargetMethod))
        {
            if (!IsAdaptiveUnbind(invocation))
            {
                values = [];
                return false;
            }

            return TryGetOptionValues(
                invocation,
                ["Tolerance", "MergeTolerance"],
                out values
            );
        }

        values = [];
        if (!IsLegacySupportedMethod(invocation.TargetMethod))
            return false;

        // 仅收集形参名为 tolerance/mergeTolerance 且类型为 double 的实参
        var builder = ImmutableArray.CreateBuilder<IArgumentOperation>();
        foreach (var argument in invocation.Arguments)
        {
            if (
                argument.Parameter is not { } parameter
                || !IsToleranceParameter(parameter)
                || parameter.Type.SpecialType != SpecialType.System_Double
            )
                continue;

            builder.Add(argument);
        }

        values = builder
            .Select(argument => (argument.Value, argument.Parameter!.Name))
            .ToImmutableArray();
        return !values.IsDefaultOrEmpty;
    }

    /// <summary>
    /// 尝试从统一解绑选项中检索指定属性值。
    /// </summary>
    /// <param name="invocation">待检查的调用操作</param>
    /// <param name="propertyName">选项属性名称</param>
    /// <param name="value">匹配到的属性值操作</param>
    /// <returns>是否找到指定属性值</returns>
    public static bool TryGetOptionValue(
        IInvocationOperation invocation,
        string propertyName,
        out IOperation value
    )
    {
        value = null!;
        if (!IsServiceUnbindMethod(invocation.TargetMethod))
            return false;

        var matches = GetOptionValues(invocation, [propertyName]);
        if (matches.IsDefaultOrEmpty)
            return false;

        value = matches[0].Value;
        return true;
    }

    /// <summary>
    /// 判断解绑调用是否使用自适应采样。
    /// </summary>
    /// <param name="invocation">待检查的调用操作</param>
    /// <returns>使用自适应采样时返回 true</returns>
    public static bool IsAdaptiveUnbind(IInvocationOperation invocation)
    {
        if (IsLegacyDynamicMethod(invocation.TargetMethod))
            return true;
        if (!IsServiceUnbindMethod(invocation.TargetMethod))
            return false;

        var values = GetOptionValues(invocation, ["SamplingMode"]);
        if (values.IsDefaultOrEmpty)
            return false;

        var value = values[0].Value;
        if (value.ConstantValue is { HasValue: true, Value: int enumValue })
            return enumValue == 1;

        return value.Syntax.ToString().EndsWith(".Adaptive", StringComparison.Ordinal)
            || value.Syntax.ToString() == "Adaptive";
    }

    private static bool IsLegacySupportedMethod(IMethodSymbol method)
    {
        // 仅匹配解绑接口及实现中的普通解绑与动态解绑方法
        if (
            method.MethodKind != MethodKind.Ordinary
            || method.Name is not ("FatherUnbind" or "FatherUnbindDynamic")
        )
            return false;

        return IsUnbinderType(method.ContainingType);
    }

    private static bool IsLegacyDynamicMethod(IMethodSymbol method) =>
        method.MethodKind == MethodKind.Ordinary
        && method.Name == "FatherUnbindDynamic"
        && IsLegacyUnbinderType(method.ContainingType);

    private static bool IsToleranceParameter(IParameterSymbol parameter) =>
        parameter.Name is ToleranceParameterName or MergeToleranceParameterName;

    private static bool IsServiceUnbindMethod(IMethodSymbol method) =>
        method.MethodKind == MethodKind.Ordinary
        && method.Name == "Unbind"
        && IsServiceType(method.ContainingType);

    private static bool IsUnbinderType(INamedTypeSymbol? type)
    {
        if (type is null)
            return false;

        var definition = type.OriginalDefinition;
        if (
            IsLegacyUnbinderInterface(definition)
            || IsServiceInterface(definition)
            || IsLegacyUnbinderImplementation(definition)
            || IsUnbinderImplementation(definition)
        )
            return true;

        // 实现类型可能通过间接接口继承解绑能力，需遍历全部接口
        foreach (var interfaceType in type.AllInterfaces)
        {
            if (
                IsLegacyUnbinderInterface(interfaceType.OriginalDefinition)
                || IsServiceInterface(interfaceType.OriginalDefinition)
            )
                return true;
        }

        return false;
    }

    private static bool IsLegacyUnbinderType(INamedTypeSymbol? type) =>
        type is not null
        && (
            IsLegacyUnbinderInterface(type.OriginalDefinition)
            || IsLegacyUnbinderImplementation(type.OriginalDefinition)
        );

    private static bool IsServiceType(INamedTypeSymbol? type) =>
        type is not null
        && (
            IsServiceInterface(type.OriginalDefinition)
            || IsUnbinderImplementation(type.OriginalDefinition)
        );

    private static bool IsLegacyUnbinderInterface(INamedTypeSymbol type) =>
        type.ContainingNamespace?.ToDisplayString() == JudgeLineNamespace
        && type.MetadataName == LegacyInterfaceMetadataName;

    private static bool IsServiceInterface(INamedTypeSymbol type) =>
        type.ContainingNamespace?.ToDisplayString() == JudgeLineNamespace
        && type.MetadataName == ServiceInterfaceMetadataName;

    private static bool IsLegacyUnbinderImplementation(INamedTypeSymbol type) =>
        type.ContainingNamespace?.ToDisplayString() == ImplementationNamespace
        && type.MetadataName == ImplementationMetadataName;

    private static bool IsUnbinderImplementation(INamedTypeSymbol type) =>
        type.ContainingNamespace?.ToDisplayString() == ImplementationNamespace
        && type.MetadataName == ImplementationMetadataName;

    private static bool TryGetOptionValues(
        IInvocationOperation invocation,
        ImmutableArray<string> propertyNames,
        out ImmutableArray<(IOperation Value, string Name)> values
    )
    {
        values = GetOptionValues(invocation, propertyNames);
        return !values.IsDefaultOrEmpty;
    }

    private static ImmutableArray<(IOperation Value, string Name)> GetOptionValues(
        IInvocationOperation invocation,
        ImmutableArray<string> propertyNames
    )
    {
        var optionsArgument = invocation.Arguments.FirstOrDefault(argument =>
            argument.Parameter?.Name == "options"
        );
        if (
            optionsArgument?.Value is not IObjectCreationOperation
            {
                Initializer: IObjectOrCollectionInitializerOperation initializer
            }
        )
            return [];

        var values = ImmutableArray.CreateBuilder<(IOperation Value, string Name)>();
        foreach (var item in initializer.Initializers)
        {
            if (
                item is ISimpleAssignmentOperation
                {
                    Target: IPropertyReferenceOperation property,
                } assignment
                && property.Property.Type.SpecialType == SpecialType.System_Double
                && propertyNames.Contains(property.Property.Name)
            )
                values.Add((assignment.Value, property.Property.Name));
            else if (
                item is ISimpleAssignmentOperation
                {
                    Target: IPropertyReferenceOperation enumProperty,
                } enumAssignment
                && propertyNames.Contains(enumProperty.Property.Name)
            )
                values.Add((enumAssignment.Value, enumProperty.Property.Name));
        }

        return values.ToImmutable();
    }
}
