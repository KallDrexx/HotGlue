using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CSScripting;

namespace HotGlue.Runtime;

public class ScriptInfo : IDisposable
{
    private const string ScriptActivityMethodName = "ScriptActivity";
    private const string ScriptInitializeMethodName = "ScriptInitialize";
    private const string ScriptDestroyMethodName = "ScriptDestroy";
    private const string ScriptEventMethodName = "ScriptEvent";

    private readonly string _sourceFileName;
    private readonly object _script;
    private readonly IReadOnlyDictionary<Type, MethodInfo> _customActivities;
    private readonly IReadOnlyDictionary<Type, MethodInfo> _customInitializes;
    private readonly IReadOnlyDictionary<Type, MethodInfo> _customDestroys;
    private readonly IReadOnlyDictionary<(Type, Type), MethodInfo> _eventMethods;

    public ScriptInfo(string sourceFileName, object script)
    {
        _sourceFileName = sourceFileName;
        _script = script;

        _customActivities = GetSingleFunctionMethods(script, ScriptActivityMethodName);
        _customInitializes = GetSingleFunctionMethods(script, ScriptInitializeMethodName);
        _customDestroys = GetSingleFunctionMethods(script, ScriptDestroyMethodName);
        _eventMethods = GetEventFunctionMethods(script);
    }

    public void RunScriptInitialize<TInput>(TInput input)
    {
        RunSingleInputMethod(_customInitializes, ScriptInitializeMethodName, input);
    }

    public void RunScriptActivity<TInput>(TInput input)
    {
        RunSingleInputMethod(_customActivities, ScriptActivityMethodName, input);
    }

    public void RunScriptDestroy<TInput>(TInput input)
    {
        RunSingleInputMethod(_customDestroys, ScriptDestroyMethodName, input);
    }

    public void RunScriptEvent<TInput1, TInput2>(TInput1 input1, TInput2 input2)
    {
        var key = (typeof(TInput1), typeof(TInput2));
        if (!_eventMethods.TryGetValue(key, out var method))
        {
            var message = $"Script '{_sourceFileName}' does not contain a `{ScriptEventMethodName}` function that" +
                          $"takes two parameters of types {typeof(TInput1).FullName} and {typeof(TInput2).FullName}";

            Debug.WriteLine(message);
            return;
        }

        method.Invoke(method.IsStatic ? null : _script, [input1, input2]);
    }

    private static IReadOnlyDictionary<Type, MethodInfo> GetSingleFunctionMethods(object script, string methodName)
    {
        return script.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Where(x => x.Name == methodName)
            .Where(x => x.GetParameters().Length == 1)
            .ToDictionary(x => x.GetParameters()[0].ParameterType, x => x);
    }

    private static IReadOnlyDictionary<(Type, Type), MethodInfo> GetEventFunctionMethods(object script)
    {
        return script.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Where(x => x.Name == ScriptEventMethodName)
            .Where(x => x.GetParameters().Length == 2)
            .ToDictionary(x => (x.GetParameters()[0].ParameterType, x.GetParameters()[1].ParameterType), x => x);
    }

    private void RunSingleInputMethod<TInput>(
        IReadOnlyDictionary<Type, MethodInfo> methods,
        string methodName,
        TInput input)
    {
        if (!methods.TryGetValue(typeof(TInput), out var method))
        {
            var message = $"Script '{_sourceFileName}' does not contain a `{methodName}` function that" +
                          $"takes a single parameter of type {typeof(TInput).FullName}";

            Debug.WriteLine(message);
            return;
        }

        method.Invoke(method.IsStatic ? null : _script, [input]);
    }

    public void Dispose()
    {
        _script.GetType().Assembly.Unload();
    }
}