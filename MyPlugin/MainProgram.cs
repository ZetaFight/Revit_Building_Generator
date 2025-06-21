using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MyPlugin
{
    [Transaction(TransactionMode.Manual)]
    class MainProgram : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document document = commandData.Application.ActiveUIDocument.Document;

            using (Transaction transaction = new Transaction(document))
            {
                transaction.Start("LoadFamily");

                string windowFamilyPath = @"C:\ProgramData\Autodesk\RVT 2018\Libraries\China\建筑\窗\普通窗\推拉窗\推拉窗6.rfa";

                // 检查文件是否存在
                if (!File.Exists(windowFamilyPath))
                {
                    TaskDialog.Show("Error", "推拉窗族文件未找到：" + windowFamilyPath);
                }

                Family windowFamily;
                bool success1 = document.LoadFamily(windowFamilyPath, out windowFamily);               
                if (success1)
                {
                    TaskDialog.Show("Revit", "推拉窗族文件加载成功：" + windowFamily.Name);
                }
                else
                {
                    TaskDialog.Show("Error", "推拉窗族文件加载失败。");
                }

                string rotateDoorPath = @"C:\ProgramData\Autodesk\RVT 2018\Libraries\China\建筑\门\普通门\旋转门\旋转门 1.rfa";

                // 检查文件是否存在
                if (!File.Exists(rotateDoorPath))
                {
                    TaskDialog.Show("Error", "旋转门族文件未找到：" + rotateDoorPath);
                }

                Family rotateDoorFamily;
                bool success2 = document.LoadFamily(rotateDoorPath, out rotateDoorFamily);
                if (success2)
                {
                    TaskDialog.Show("Revit", "旋转门族文件加载成功：" + rotateDoorFamily.Name);
                }
                else
                {
                    TaskDialog.Show("Error", "旋转门族文件加载失败。");
                }

                transaction.Commit();
            }
            
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            return Result.Succeeded;
        }
    }
}
