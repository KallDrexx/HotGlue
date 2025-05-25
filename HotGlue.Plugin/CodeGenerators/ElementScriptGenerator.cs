using FlatRedBall.Glue.CodeGeneration;
using FlatRedBall.Glue.CodeGeneration.CodeBuilder;
using FlatRedBall.Glue.Plugins.ExportedImplementations;
using FlatRedBall.Glue.SaveClasses;
using FlatRedBall.IO;
using Gum.DataTypes;
using HotGlue.Plugin.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotGlue.Plugin.CodeGenerators;
public class ElementScriptGenerator : ElementComponentCodeGenerator
{
    private readonly GlueState _glueState;
    private readonly GlueCommands _glueCommands;
    private readonly MainViewModel _viewModel;

    public ElementScriptGenerator(GlueState glueState, GlueCommands glueCommands, MainViewModel viewModel)
    {
        _glueState = glueState;
        _glueCommands = glueCommands;
        _viewModel = viewModel;
    }


    public override ICodeBlock GenerateAdditionalMethods(ICodeBlock codeBlock, IElement element)
    {
        // todo: use CallCodeGenerationStart
        if (_viewModel.IsEnabled && !HasScriptFile(element))
        {
            CreateScriptFile(element);
        }

        return codeBlock;
    }

    private bool HasScriptFile(IElement element)
    {
        return GetScriptFilePathFor(element).Exists();
    }

    private FilePath GetScriptFilePathFor(IElement element)
    {
        string generatedCodeFileName = _glueState.CurrentGlueProjectDirectory + element.Name + ".Script.cs";
        return generatedCodeFileName;
    }

    private void CreateScriptFile(IElement element)
    {
        if(_glueState.CurrentGlueProject == null)
        {
            return;
        }

        var filePath = GetScriptFilePathFor(element);

        var contents = GetEmptyFileContentsFor(element);

        _glueCommands.FileCommands.SaveIfDiffers(filePath, contents);
    }

    private string GetEmptyFileContentsFor(IElement iElement)
    {
        var element = iElement as GlueElement;

        var strippedElementName = element.ClassName;
        var instanceName = "instance";
        var elementNamespace = _glueCommands.GenerateCodeCommands.GetNamespaceForElement(element);
        return 
$@"

using {elementNamespace};

public void ScriptInitialize({strippedElementName} {instanceName})
{{
    
}}


public void ScriptActivity({strippedElementName} {instanceName})
{{
    
}}


public void ScriptDestroy({strippedElementName} {instanceName})
{{
    
}}
";
    }
}
