using Mono.Cecil;
using System.Reflection;

namespace no_attrs;

internal class Program {
    static void Main(string[] args) {
        if(args.Length == 0) {
            Console.WriteLine("Usage: no-attrs <path-to-assembly> [<optional-output-path>]");
            return;
        }
        var newPath = args.Length >= 2 ? args[1] : args[0].Replace(".dll", "-no-attrs.dll");
        var deps = Directory.GetFiles(Path.GetDirectoryName(args[0]), "*.dll", SearchOption.AllDirectories).ToList();
        deps.Remove(args[0]);
        deps.Remove(args[0]);
        var module = ModuleDefinition.ReadModule(args[0], new ReaderParameters { AssemblyResolver = new NAResolver(deps) });
        foreach(var t in module.Types) {
            t.CustomAttributes.Clear();
            foreach(var m in t.Methods) m.CustomAttributes.Clear();
            foreach(var f in t.Fields) f.CustomAttributes.Clear();
        }
        module.Write(newPath);
    }
}
class NAResolver : BaseAssemblyResolver {
    IEnumerable<string> modules;
    public NAResolver(IEnumerable<string> modules) {
        this.modules = modules;
    }
    public override AssemblyDefinition Resolve(AssemblyNameReference name) {
        foreach(string filename in modules) {
            try {
                if(name.FullName == AssemblyName.GetAssemblyName(filename).FullName)
                    return AssemblyDefinition.ReadAssembly(filename);
            } catch { }
        }
        return base.Resolve(name);
    }
}