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
    class CreateLevel : IExternalEventHandler
    {
        private static string _textF;

        public static string TextF
        {
            get { return _textF; }
            set {_textF = value;}
        }

        private List<int> Converter(string text)
        {
            List<int> spanList = new List<int>();

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

            if (spanList.Count == 0)
            {
                MessageBox.Show("输入层高转换格式时发生错误。");
            }

            return spanList;
        }

        public void Execute(UIApplication app)
        {
            List<int> floorHeightList = Converter(TextF);
            
            int floorHeightListLen = floorHeightList.Count;    
            int vHeightListLen = floorHeightList.Count;

            Document document = app.ActiveUIDocument.Document;

            using (Transaction transaction = new Transaction(document))
            {
                try
                {
                    transaction.Start("CreateLevel");

                    // 先删除已有标高
                    FilteredElementCollector levelCollector = new FilteredElementCollector(document).OfClass(typeof(Level)).WhereElementIsNotElementType();
                    List<Level> levelsToDelete = levelCollector.Cast<Level>().Where(level => level.Name.Contains("标高") && !level.Name.Equals("标高 1")).ToList();
                    List<ElementId> levelIdsToDelete = levelsToDelete.Select(level => level.Id).ToList();
                    document.Delete(levelIdsToDelete);

                    // 创建标高
                    double mmToInch = 304.8;

                    FilteredElementCollector viewCollector = new FilteredElementCollector(document).OfClass(typeof(ViewFamilyType));

                    for (int i = 1; i < vHeightListLen + 1; i++)
                    {
                        Level newlevel = Level.Create(document, floorHeightList.Take(i).Sum() / mmToInch);
                        newlevel.Name = "标高 " + (i + 1).ToString();
                        
                        // 创建标高视图
                        foreach (ViewFamilyType viewFamilyType in viewCollector)
                        {
                            if (viewFamilyType.ViewFamily == ViewFamily.FloorPlan || viewFamilyType.ViewFamily == ViewFamily.CeilingPlan)
                            {
                                ViewPlan view = ViewPlan.Create(document, viewFamilyType.Id, newlevel.Id);
                            }
                        }
                    }
                                        
                    transaction.Commit();
                    TaskDialog.Show("Revit", "标高创建完成。");
                }
                catch (Exception error)
                {
                    TaskDialog.Show("Revit", "标高生成出现错误:" + error.Message);
                    transaction.RollBack();

                }
            }
        }

        public string GetName()
        {
            return "CreateLevel";
        }
    }
}
