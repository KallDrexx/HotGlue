using System.Reflection;

namespace HotGlue.Runtime;

public class ScriptMethod
{
    public string SourceScriptFile { get; }
    public MethodInfo Method { get; }

    public ScriptMethod(string sourceScriptFile, MethodInfo method)
    {
        SourceScriptFile = sourceScriptFile;
        Method = method;
    }
}