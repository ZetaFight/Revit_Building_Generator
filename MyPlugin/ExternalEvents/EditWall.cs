using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
namespace MyPlugin
{
    class EditWall : IExternalEventHandler
    {
        public class WallSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                // 检查元素类别是否为墙
                if (elem.Category != null && elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Walls)
                {
                    return true;
                }
                return false;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }

        private List<Level> GetLevel(Document document)
        {
            FilteredElementCollector levelCollector = new FilteredElementCollector(document).OfClass(typeof(Level)).WhereElementIsNotElementType();
            List<Level> levelsNeeded = levelCollector.Cast<Level>().Where(level => level.Name.Contains("标高")).ToList();

            return levelsNeeded;
        }
        
        private FamilySymbol GetWindowType(Document doc, string windowName)
        {
            // 查找窗户类型
            FilteredElementCollector collector = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Windows);

            foreach (FamilySymbol symbol in collector)
            {
                if (symbol.Name.Equals(windowName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return symbol;
                }
            }

            return null; // 找不到指定名称的窗户类型
        }

        public void Execute(UIApplication app)
        {
            UIDocument uiDocument = app.ActiveUIDocument;
            Document document = uiDocument.Document;

            TaskDialog.Show("Revit", "请选择一面墙");


            try
            {
                while (true)
                {

                    Reference selectedWallRef = uiDocument.Selection.PickObject(ObjectType.Element, new WallSelectionFilter(), "请选择一面墙");
                    Element selectedWall = document.GetElement(selectedWallRef);

                    List<Level> allLevels = GetLevel(document);
                    
                    List<Level> selectLevels = allLevels.Take(allLevels.Count - 1).Skip(1).ToList();
                    List<string> selectLevelsName = new List<string>();
                    foreach (Level levels in selectLevels)
                    {
                        selectLevelsName.Add(levels.Name);
                    }

                    if (selectedWall != null && selectedWall is Wall wall)
                    {
                        using (Transaction transaction = new Transaction(document))
                        {
                            transaction.Start("EditWall");
                            
                            // 获取墙的创建标高
                            Level level = document.GetElement(wall.LevelId) as Level;

                            // 获取墙的高度
                            double wallHeight = wall.LookupParameter("无连接高度").AsDouble(); 

                            // 获取墙的创建曲线
                            LocationCurve locationCurve = wall.Location as LocationCurve;

                            // 获取墙的创建轴线
                            Line line = locationCurve.Curve as Line;

                            // 获取墙的长度
                            double wallLength = line.Length;
                            
                            // 计算两个三等分点
                            XYZ point1 = line.GetEndPoint(0).Add(line.Direction.Multiply(wallLength / 3));
                            XYZ point2 = line.GetEndPoint(0).Add(line.Direction.Multiply(2 * wallLength / 3));

                            // 收集同一轴线上的所有墙
                            FilteredElementCollector collector = new FilteredElementCollector(document)
                                .OfClass(typeof(Wall))
                                .WhereElementIsNotElementType();

                            List<Wall> wallsOnSameAxis = new List<Wall>();

                            foreach (Wall upWall in collector)
                            {
                                LocationCurve upWallLocationCurve = upWall.Location as LocationCurve;

                                Line upWallLine = upWallLocationCurve.Curve as Line;
                                if (upWallLine.GetEndPoint(0).X == line.GetEndPoint(0).X && upWallLine.GetEndPoint(1).X == line.GetEndPoint(1).X
                                    && upWallLine.GetEndPoint(0).Y == line.GetEndPoint(0).Y && upWallLine.GetEndPoint(1).Y == line.GetEndPoint(1).Y)
                                {
                                    wallsOnSameAxis.Add(upWall);
                                }
                            }

                            // 查找窗户类型
                            FamilySymbol windowType = GetWindowType(document, "1200 x 1500mm"); // 根据名称查找窗户类型

                            if (windowType != null && !windowType.IsActive)
                            {
                                windowType.Activate(); // 激活窗户类型
                            }


                            // 在墙壁上创建窗户
                            foreach (Wall windowWall in wallsOnSameAxis)
                            {
                                Level windowWallLevel = document.GetElement(windowWall.LevelId) as Level;

                                if (selectLevelsName.Contains(windowWallLevel.Name))
                                {
                                    // 在计算出的点上创建窗户
                                    if (windowType != null)
                                    {
                                        // 创建窗户
                                        FamilyInstance window1 = document.Create.NewFamilyInstance(point1, windowType, windowWall, windowWallLevel, StructuralType.NonStructural);
                                        Parameter bottomOffsetParam1 = window1.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM);
                                        bottomOffsetParam1.Set(800 / 304.8);

                                        FamilyInstance window2 = document.Create.NewFamilyInstance(point2, windowType, windowWall, windowWallLevel, StructuralType.NonStructural);
                                        Parameter bottomOffsetParam2 = window2.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM);
                                        bottomOffsetParam2.Set(800 / 304.8);
                                    }
                                }
                            }


                            if (level.Name == "标高 1")
                            {
                                // 将第一层的外墙替换为玻璃幕墙
                                document.Delete(selectedWall.Id);

                                WallType wallType = new FilteredElementCollector(document).OfClass(typeof(WallType)).Cast<WallType>().FirstOrDefault(wt => wt.Name.Equals("外部玻璃"));
                                Wall glassWall = Wall.Create(document, line, wallType.Id, level.Id, wallHeight, 0.0, false, false);
                                                                                                                                                           
                            }


                            transaction.Commit();
                        }
                    
                    }
                }

            }
            catch (Exception error)
            {
                TaskDialog.Show("Revit", "墙壁选取中止: " + error.Message);
                return;
            }
        }

        public string GetName()
        {
            return "EditWall" ;
        }
    }
}
