using FlatRedBall.Glue.Plugins;
using FlatRedBall.Glue.Plugins.ExportedImplementations;
using HotGlue.Plugin.CodeGenerators;
using HotGlue.Plugin.ViewModels;
using HotGlue.Plugin.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotGlue.Plugin;

[Export(typeof(PluginBase))]
public class MainPlugin : PluginBase
{
    ElementScriptGenerator _elementScriptGenerator;
    public override void StartUp()
    {
        var view = new MainView();
        var viewModel = new MainViewModel(GlueCommands.Self);
        view.DataContext = viewModel;

        this.CreateAndAddTab(view,  "HotGlue Plugin", TabLocation.Bottom);

        _elementScriptGenerator = new ElementScriptGenerator(GlueState.Self, GlueCommands.Self, viewModel);
        base.RegisterCodeGenerator(_elementScriptGenerator);
    }
}
