#if !NET11_0_OR_GREATER
// C# 15 recognizes these contracts when targeting runtimes predating .NET 11.
#pragma warning disable IDE0130
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
internal sealed class UnionAttribute : Attribute;

internal interface IUnion
{
    public object? Value { get; }
}
#pragma warning restore IDE0130
#endif
