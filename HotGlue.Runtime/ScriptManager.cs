using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSScripting;
using CSScriptLib;

namespace HotGlue.Runtime;

public class ScriptManager
{
    private static readonly Lazy<ScriptManager> StaticInstance = new(new ScriptManager());
    private readonly Dictionary<string, ScriptInfo> _scripts = new();

    public static ScriptManager Instance => StaticInstance.Value;

    public IReadOnlyList<ScriptMethod> Methods
        => _scripts.Values
            .SelectMany(x => x.Methods.Select(y => new ScriptMethod(x.SourceScriptName, y)))
            .ToArray();

    private ScriptManager()
    {
    }

    public void AddScriptFile(string file)
    {
        dynamic script;
        try
        {
            var code = File.ReadAllText(file);
            script = CSScript.Evaluator
                .With(x => x.IsAssemblyUnloadingEnabled = true)
                .With(x => x.DebugBuild = true)
                .LoadMethod(code);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Error: Failed to read script file '{file}': {exception}");
            return;
        }

        if (_scripts.Remove(file, out var prevScript))
        {
            Console.WriteLine($"Unloading script '{file}'");
            prevScript.Dispose();
        }

        Console.WriteLine($"Loaded script '{file}'");
        _scripts.Add(file, new ScriptInfo(file, script));
    }

    public void Invoke(string file, string methodName, params object[] parameters)
    {
        if (!_scripts.TryGetValue(file, out var script))
        {
            Console.WriteLine($"Error: No script '{file}' loaded");
            return;
        }

        script.Run(methodName, parameters);
    }
}
