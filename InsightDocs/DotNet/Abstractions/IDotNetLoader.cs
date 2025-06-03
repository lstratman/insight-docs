using InsightDocs.DotNet.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InsightDocs.DotNet.Abstractions;

public interface IDotNetLoader
{
    DotNetAssembly LoadAssembly(Assembly assembly);
    DotNetType LoadType(Type type);
    DotNetIndex LoadAssemblies(List<string> assemblyPaths, List<string> runtimeAssemblyPaths, Func<Type, bool>? typeFilter);
}
