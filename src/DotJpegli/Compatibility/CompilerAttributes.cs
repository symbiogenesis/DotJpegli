#if NETSTANDARD2_1
// Compiler-recognized compatibility attributes require this namespace.
#pragma warning disable IDE0130
namespace System.Runtime.CompilerServices;

internal static class IsExternalInit;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class CallerArgumentExpressionAttribute(string parameterName) : Attribute
{
    public string ParameterName { get; } = parameterName;
}
#pragma warning restore IDE0130

#endif
