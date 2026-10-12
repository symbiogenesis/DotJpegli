using System.Reflection;
using Xunit;

namespace DotJpegli.Tests;

public sealed class ManagedArchitectureTests
{
    [Fact]
    public void EncoderAssemblyHasNoNativeImportsOrNonFrameworkDependencies()
    {
        Assembly assembly = typeof(JpegliEncoder).Assembly;
        Assert.All(assembly.GetReferencedAssemblies(), reference =>
            Assert.StartsWith("System.", reference.Name!));
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
