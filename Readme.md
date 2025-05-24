# HotGlue

Provides simple C# scripting support for FlatRedBall projects.

## Getting Started

Create a `.cs` file with root level methods (namespace and wrapping classes are not required). 
`public` functions are then exposed, e.g.

```csharp
using System;

public void DoStuff(PlatformerMarch2025.Entities.Coin coin)
{
    Console.WriteLine("Hello World");
    coin.Test();

    Console.WriteLine("test4!");
    Foo();
}

private void Foo()
{
    Console.WriteLine("Foo called");
}

```

Load the script into the `ScriptManager` singleton:

```csharp
ScriptManager.Instance.AddScriptFile("script.cs");
```

Get a list of methods exposed by your script:

```csharp
var exposedMethods = ScriptManager.Instance.Methods;
```

Then invoke the method from the script that exposes it:

```csharp
ScriptManager.Instance.Invoke("script.cs", DoStuff, coinInstance);
```

Get updates to your script file by calling `AddScriptFile("script.cs")` again.

