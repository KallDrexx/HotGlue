using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CSScripting;
using CSScriptLib;

namespace HotGlue.Runtime;

public class ScriptManager
{
    private static readonly Lazy<ScriptManager> StaticInstance = new(new ScriptManager());
    private readonly Dictionary<string, GlueElementScriptInfo> _scripts = new();

    public static ScriptManager Instance => StaticInstance.Value;

    private ScriptManager()
    {
    }

    public void AddGlueElementScriptFile<T>(string file)
    {
        var fileInfo = new FileInfo(file);
        IGlueElementScript<T> script;
        try
        {
            var code = File.ReadAllText(file);
            script = CSScript.Evaluator
                .With(x => x.IsAssemblyUnloadingEnabled = true)
                .With(x => x.DebugBuild = true)
                .LoadCode<IGlueElementScript<T>>(code);
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Error: Failed to read script file '{fileInfo.FullName}': {exception}");
            return;
        }

        if (_scripts.Remove(fileInfo.FullName, out var prevScript))
        {
            Debug.WriteLine($"Unloading script '{fileInfo.FullName}'");
            prevScript.Dispose();
        }

        Debug.WriteLine($"Loaded script '{fileInfo.FullName}'");
        _scripts.Add(fileInfo.FullName, new GlueElementScriptInfo(file, script));
    }

    public void InvokeScriptActivity<T>(string file, T obj)
    {
        var fileInfo = new FileInfo(file);

        if (!_scripts.TryGetValue(fileInfo.FullName, out var script))
        {
            Debug.WriteLine($"Error: No script '{fileInfo.FullName}' loaded");
            return;
        }

        script.RunScriptActivity(obj);
    }
}
