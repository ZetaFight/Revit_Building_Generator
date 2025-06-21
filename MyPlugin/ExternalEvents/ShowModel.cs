using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace MyPlugin
{
    class ShowModel : IExternalEventHandler
    {
        private ViewModel _viewModel;

        public ShowModel(ViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        
        public void Execute(UIApplication app)
        {
            Document document = app.ActiveUIDocument.Document;

            // 使用 FilteredElementCollector 查找三维视图
            View3D view3D = new FilteredElementCollector(document)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault(v => !v.IsTemplate); // 排除模板视图

            if (view3D == null)
            {
                TaskDialog.Show("Error", "未找到三维视图。");
                return;
            }

            // 创建 PreviewControl
            PreviewControl pc = new PreviewControl(document, view3D.Id);
            pc.Width = 570;
            pc.Height = 567;

            // 更新 ViewModel 中的 PreviewControl
            _viewModel.UpdatePreviewControl(pc);
        }

        public string GetName()
        {
            return "ShowModel";
        }
    }
}
