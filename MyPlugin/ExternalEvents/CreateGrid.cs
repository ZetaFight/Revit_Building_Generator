using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


namespace MyPlugin
{
    class CreateGrid : IExternalEventHandler
    {
     
        private static string _textV;
        private static string _textH;

        public static string TextV
        {
            get { return _textV; }
            set { _textV = value; }
        }

        public static string TextH
        {
            get { return _textH; }
            set { _textH = value; }
        }

        private List<int> Converter(string text)
        {
            List<int> spanList = new List<int>();
            spanList.Add(0);

            string[] textTrimed = text.Split(',');

            foreach (string num in textTrimed)
            {
                int convertedNum;
                if (int.TryParse(num, out convertedNum))
                {
                    spanList.Add(convertedNum);
                }
                else
                {
                    MessageBox.Show($"输入 '{num}' 不能被转换为整数。");
                    return null;
                }
            }

            if (spanList.Count == 1)
            {
                MessageBox.Show("输入间距转换格式时发生错误。");
            }

            return spanList;
        }


        public void Execute(UIApplication app)
        {
            List<int> verticalSpanList = Converter(TextV);
            List<int> horizontalSpanList = Converter(TextH);

            int vSpanListLen = verticalSpanList.Count;
            int hSpanListLen = horizontalSpanList.Count;

            Document document = app.ActiveUIDocument.Document;

            using (Transaction transaction = new Transaction(document))
            {
                try
                {
                    transaction.Start("CreateGrid");

                    // 先删除已有轴网
                    FilteredElementCollector gridCollector = new FilteredElementCollector(document).OfClass(typeof(Grid));

                    ICollection<ElementId> gridIds = gridCollector.ToElementIds();

                    if  (gridIds.Count > 0)
                    {
                        document.Delete(gridIds);
                    }

                    // 创建轴网
                    double mmToInch = 304.8;
                    double offset = 1500 / 304.8;

                    // 先生成竖向的轴线
                    for (int i = 1; i < vSpanListLen + 1; i++)
                    {
                        XYZ start = new XYZ(verticalSpanList.Take(i).Sum() / mmToInch, 0, 0);
                        XYZ end = new XYZ(verticalSpanList.Take(i).Sum() / mmToInch, horizontalSpanList.Sum() / mmToInch, 0);
                        Grid grid = Grid.Create(document, Line.CreateBound(start - new XYZ(0, offset, 0), end + new XYZ(0, offset, 0)));
                        char gridName = (char)('A' + i - 1);
                        grid.Name = gridName.ToString();
                    }

                    // 再生成水平的轴线
                    for (int i = 1; i < hSpanListLen + 1; i++)
                    {
                        XYZ start = new XYZ(0, horizontalSpanList.Take(i).Sum() / mmToInch, 0);
                        XYZ end = new XYZ(verticalSpanList.Sum() / mmToInch, horizontalSpanList.Take(i).Sum() / mmToInch, 0);
                        Grid grid = Grid.Create(document, Line.CreateBound(start - new XYZ(offset, 0, 0), end + new XYZ(offset, 0, 0)));
                        grid.Name = i.ToString();
                    }

                    transaction.Commit();

                    TaskDialog.Show("Revit", "轴网创建完成。");
                }
                catch (Exception error)
                {
                    TaskDialog.Show("Revit", "轴线生成出现错误:" + error.Message);
                    transaction.RollBack();
                }

            }

        }

        public string GetName()
        {
            return "CreateGrid";
        }
    }
}
