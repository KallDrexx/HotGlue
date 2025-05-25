using System;
using CSScripting;

namespace HotGlue.Runtime;

internal class GlueElementScriptInfo
{
    private readonly object _script;
    private readonly string _sourceScriptName;

    public GlueElementScriptInfo(string scriptSourceName, object script)
    {
        _script = script;
        _sourceScriptName = scriptSourceName;
    }

    public void RunScriptActivity<T>(T obj)
    {
        GetScriptRunner<T>().ScriptActivity(obj);
    }

    public void Dispose()
    {
        var scriptType = _script.GetType();
        scriptType.Assembly.Unload();
    }

    private IGlueElementScript<T> GetScriptRunner<T>()
    {
        if (_script is not IGlueElementScript<T> runnableScript)
        {
            var message = $"Script at '{_sourceScriptName}' could not be resolved as an " +
                          $"IGlueElementScript<{typeof(T).FullName}>";
            throw new InvalidOperationException(message);
        }

        return runnableScript;
    }
}