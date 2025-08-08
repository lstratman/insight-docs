using InsightDocs.DotNet.Model;
using System.Reflection;

namespace InsightDocs.DotNet.Abstractions;

public interface IDotNetLoader
{
    DotNetAssembly LoadAssembly(Assembly assembly);
    DotNetType LoadType(Type type, DotNetIndex index);
    DotNetIndex LoadAssemblies(List<string> assemblyPaths, List<string> runtimeAssemblyPaths, Func<Type, bool>? typeFilter);
}
