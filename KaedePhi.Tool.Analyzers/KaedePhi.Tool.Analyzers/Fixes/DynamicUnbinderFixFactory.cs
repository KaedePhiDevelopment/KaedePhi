using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace KaedePhi.Tool.Analyzers.Fixes;

/// <summary>
/// 生成将动态解绑调用改写为普通解绑调用的替换表达式。
/// </summary>
internal static class DynamicUnbinderFixFactory
{
    /// <summary>
    /// 创建移除容差实参并改用普通解绑方法的调用表达式。
    /// </summary>
    /// <param name="invocation">原始调用表达式</param>
    /// <param name="operation">原始调用的操作信息</param>
    /// <returns>改写后的调用表达式</returns>
    public static InvocationExpressionSyntax Create(
        InvocationExpressionSyntax invocation,
        IInvocationOperation operation
    )
    {
        if (operation.TargetMethod.Name == "Unbind")
            return CreateOptionsFix(invocation, operation);

        // 先记录需要移除的容差实参在实参列表中的索引
        var removedIndices = new HashSet<int>(
            operation
                .Arguments.Select((argument, index) => (argument, index))
                .Where(item => item.argument.Parameter?.Name is "tolerance" or "mergeTolerance")
                .Select(item => item.index)
        );
        var arguments = invocation.ArgumentList.Arguments;
        // 从后向前移除，避免前面的索引发生偏移
        for (var i = arguments.Count - 1; i >= 0; i--)
        {
            if (removedIndices.Contains(i))
                arguments = arguments.RemoveAt(i);
        }

        var expression = invocation.Expression;
        // 将方法名 FatherUnbindDynamic 替换为普通解绑方法 FatherUnbind
        if (
            expression is MemberAccessExpressionSyntax memberAccess
            && memberAccess.Name.Identifier.Text == "FatherUnbindDynamic"
        )
        {
            expression = memberAccess.WithName(
                SyntaxFactory.IdentifierName("FatherUnbind").WithTriviaFrom(memberAccess.Name)
            );
        }

        return invocation
            .WithExpression(expression)
            .WithArgumentList(invocation.ArgumentList.WithArguments(arguments));
    }

    private static InvocationExpressionSyntax CreateOptionsFix(
        InvocationExpressionSyntax invocation,
        IInvocationOperation operation
    )
    {
        var optionsArgument = operation.Arguments.FirstOrDefault(argument =>
            argument.Parameter?.Name == "options"
        );
        if (
            optionsArgument?.Value.Syntax is not ObjectCreationExpressionSyntax objectCreation
            || objectCreation.Initializer is not { } initializer
        )
            return invocation;

        var expressions = initializer.Expressions;
        var samplingModeReplaced = false;
        for (var i = expressions.Count - 1; i >= 0; i--)
        {
            if (expressions[i] is not AssignmentExpressionSyntax assignment)
                continue;

            var propertyName = assignment.Left switch
            {
                IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
                MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
                _ => string.Empty,
            };
            if (propertyName is "Tolerance" or "MergeTolerance")
            {
                expressions = expressions.RemoveAt(i);
                continue;
            }

            if (propertyName == "SamplingMode")
            {
                var equalSpacing = SyntaxFactory
                    .ParseExpression(
                        "global::KaedePhi.Tool.JudgeLine.JudgeLineUnbindSamplingMode.EqualSpacing"
                    )
                    .WithTriviaFrom(assignment.Right);
                expressions = expressions.Replace(assignment, assignment.WithRight(equalSpacing));
                samplingModeReplaced = true;
            }
        }

        return samplingModeReplaced
            ? invocation.ReplaceNode(
                objectCreation,
                objectCreation.WithInitializer(initializer.WithExpressions(expressions))
            )
            : invocation;
    }
}
