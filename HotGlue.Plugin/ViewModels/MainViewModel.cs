using FlatRedBall.Glue.MVVM;
using FlatRedBall.Glue.Plugins.ExportedImplementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotGlue.Plugin.ViewModels;
public class MainViewModel : ViewModel
{
    private readonly GlueCommands _glueCommands;

    public bool IsEnabled
    {
        get => Get<bool>();
        set
        {
            if(Set(value))
            {
                // Notify the code generator that the value has changed
                _glueCommands.GenerateCodeCommands.GenerateAllCode();
            }
        }
    }

    public MainViewModel(GlueCommands glueCommands)
    {
        _glueCommands = glueCommands;
    }

}
