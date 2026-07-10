using MvvmCross.Base;
using MvvmCross.ViewModels;
using TemplateGenerator.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace TemplateGenerator.Core
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            RegisterAppStart<ShellViewModel>();
        }
    }
}
