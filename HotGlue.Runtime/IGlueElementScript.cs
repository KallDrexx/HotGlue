namespace HotGlue.Runtime;

public interface IGlueElementScript<T>
{
    void ScriptInitialize(T element);
    void ScriptActivity(T element);
    void ScriptDestroy(T element);
}