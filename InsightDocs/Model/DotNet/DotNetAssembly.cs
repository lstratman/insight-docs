using System.Reflection;

namespace InsightDocs.Model.DotNet;

public class DotNetAssembly
{
    private readonly static Dictionary<string, DotNetAssembly> AssemblyCache = [];

    public static DotNetAssembly Resolve(Assembly assembly)
    {
        string key = assembly.Location;

        if (!AssemblyCache.TryGetValue(key, out DotNetAssembly? assemblyMetadata))
        {
            assemblyMetadata = new DotNetAssembly(assembly);
        }

        return assemblyMetadata;
    }

    protected DotNetAssembly(Assembly assembly)
    {
        AssemblyName assemblyName = assembly.GetName();

        Name = assemblyName.Name;
        FullName = assemblyName.FullName;
    }

    public string? Name
    {
        get;
        set;
    }

    public string FullName
    {
        get;
        set;
    }
}