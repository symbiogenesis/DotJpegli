using System.Reflection;
using System.Runtime.Versioning;
using Xunit;

namespace DotJpegli.Tests;

public sealed class ManagedArchitectureTests
{
    [Fact]
    public void EncoderAssemblyTargetsSelectedFramework()
    {
#if DOTJPEGLI_NETSTANDARD_TESTS
        const string expected = ".NETStandard,Version=v2.1";
#else
        string expected = typeof(ManagedArchitectureTests).Assembly
            .GetCustomAttribute<TargetFrameworkAttribute>()!.FrameworkName;
#endif
        Assert.Equal(expected, typeof(JpegliEncoder).Assembly
            .GetCustomAttribute<TargetFrameworkAttribute>()!.FrameworkName);
    }

    [Fact]
    public void EncoderAssemblyHasNoNativeImportsOrNonFrameworkDependencies()
    {
        Assembly assembly = typeof(JpegliEncoder).Assembly;
        Assert.All(assembly.GetReferencedAssemblies(), reference =>
            Assert.True(reference.Name == "netstandard" ||
                reference.Name!.StartsWith("System.", StringComparison.Ordinal),
                $"Unexpected encoder dependency: {reference.Name}"));
        foreach (Type type in assembly.GetTypes())
        {
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                Assert.False(method.Attributes.HasFlag(MethodAttributes.PinvokeImpl));
                Assert.False(method.ReturnType.IsPointer);
                Assert.All(method.GetParameters(), parameter => Assert.False(parameter.ParameterType.IsPointer));
            }
            Assert.All(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Static | BindingFlags.Instance), field => Assert.False(field.FieldType.IsPointer));
        }
    }
}
