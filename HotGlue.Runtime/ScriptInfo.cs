using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSScripting;

namespace HotGlue.Runtime;

public class ScriptInfo : IDisposable
{
    private static readonly string[] IgnoredMethodNames = ["GetType", "ToString", "GetHashCode", "Equals"];
    private readonly dynamic _script;
    private readonly Dictionary<(string, int), MethodInfo> _methods = new();

    public string SourceScriptName { get; set; }
    public IReadOnlyList<MethodInfo> Methods => _methods
        .Values
        .Where(x => !IgnoredMethodNames.Contains(x.Name))
        .ToArray();

    public ScriptInfo(string scriptSourceName, dynamic script)
    {
        _script = script;
        SourceScriptName = scriptSourceName;
        var methods = ((Type)script.GetType()).GetMethods(BindingFlags.Public | BindingFlags.Instance);

        // Assume no overload support for now
        foreach (var method in methods)
        {
            _methods.Add((method.Name, method.GetParameters().Length), method);
        }
    }

    public void Run(string methodName, params object[] parameters)
    {
        if (!_methods.TryGetValue((methodName, parameters.Length), out var method))
        {
            Console.WriteLine($"Error: No method '{methodName}' with " +
                              $"{parameters.Length} parameters was defined in '{SourceScriptName}'");

            return;
        }

        method.Invoke(_script, parameters);
    }

    public void Dispose()
    {
        var scriptType = (Type)_script.GetType();
        scriptType.Assembly.Unload();
    }
}