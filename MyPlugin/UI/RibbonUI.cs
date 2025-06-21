using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace MyPlugin
{
    [Transaction(TransactionMode.Manual)]
    class RibbonUI : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            application.CreateRibbonTab("MyPlugin");

            RibbonPanel ribbonPanel = application.CreateRibbonPanel("MyPlugin", "框架结构建筑建模工具栏");

            PushButtonData pushButtonData = new PushButtonData("Generator", "框架结构建模工具", @"H:\A_Zetao\VS Solution\MyPlugin\MyPlugin\bin\Debug\MyPlugin.dll", "MyPlugin.MainProgram");
            PushButton pushButton = ribbonPanel.AddItem(pushButtonData) as PushButton;

            pushButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/MyPlugin;component/Assets/建筑修建(1).png", UriKind.Absolute));
            pushButton.ToolTip = "一键式框架结构自动建模";
            
            return Result.Succeeded;
        }
    }
}
