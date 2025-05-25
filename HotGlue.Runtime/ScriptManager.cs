using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CSScripting;
using CSScriptLib;

namespace HotGlue.Runtime;

public class ScriptManager
{
    private static readonly Lazy<ScriptManager> StaticInstance = new(new ScriptManager());
    private readonly Dictionary<string, ScriptInfo> _scripts = new();
    private readonly HashSet<string> _reportedFailures = new();

    public static ScriptManager Instance => StaticInstance.Value;

    private ScriptManager()
    {
    }

    public void AddScriptFile(string file)
    {
        var fileInfo = new FileInfo(file);
        _reportedFailures.Remove(fileInfo.FullName);

        object script;
        try
        {
            var code = File.ReadAllText(fileInfo.FullName);
            script = CSScript.Evaluator
                .With(x => x.IsAssemblyUnloadingEnabled = true)
                .With(x => x.DebugBuild = true)
                .LoadCode(code);
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Error: Failed to read script file '{fileInfo.FullName}': {exception}");
            _reportedFailures.Add(file);
            return;
        }

        if (_scripts.Remove(fileInfo.FullName, out var prevScript))
        {
            Debug.WriteLine($"Unloading script '{fileInfo.FullName}'");
            prevScript.Dispose();
        }

        Debug.WriteLine($"Loaded script '{fileInfo.FullName}'");
        _scripts.Add(fileInfo.FullName, new ScriptInfo(file, script));
    }

    public void RunScriptInitialize<TInput>(string file, TInput input)
    {
        RunScriptFunction(file, x => x.RunScriptInitialize(input));
    }

    public void RunScriptActivity<TInput>(string file, TInput input)
    {
        RunScriptFunction(file, x => x.RunScriptActivity(input));
    }

    public void RunScriptDestroy<TInput>(string file, TInput input)
    {
        RunScriptFunction(file, x => x.RunScriptDestroy(input));
    }

    public void RunScriptEvent<TInput1, TInput2>(string file, TInput1 input1, TInput2 input2)
    {
        RunScriptFunction(file, x => x.RunScriptEvent(input1, input2));
    }

    private void RunScriptFunction(string file, Action<ScriptInfo> action)
    {
        var fileInfo = new FileInfo(file);
        if (!_scripts.TryGetValue(fileInfo.FullName, out var script))
        {
            if (!_reportedFailures.Contains(file))
            {
                Debug.WriteLine($"No script has been loaded with the name '{file}'");
                _reportedFailures.Add(file);
            }

            return;
        }

        action(script);
    }
}
