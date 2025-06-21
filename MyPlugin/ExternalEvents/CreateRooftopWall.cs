using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace MyPlugin
{
    class CreateRooftopWall : IExternalEventHandler
    {
        private List<Level> GetLevel(Document document)
        {
            FilteredElementCollector levelCollector = new FilteredElementCollector(document).OfClass(typeof(Level)).WhereElementIsNotElementType();
            List<Level> levelsNeeded = levelCollector.Cast<Level>().Where(level => level.Name.Contains("标高")).ToList();

            return levelsNeeded;
        }

        public class ColumnSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                // 检查元素类别是否为柱子
                if (elem.Category != null && elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Columns)
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

        public void Execute(UIApplication app)
        {
            UIDocument uiDocument = app.ActiveUIDocument;
            Document document = uiDocument.Document;
            
            try
            {
                XYZ columnPoint1 = null;
                XYZ columnPoint2 = null;

                while (columnPoint1 == null || columnPoint1.X == columnPoint2.X || columnPoint1.Y == columnPoint2.Y)
                {
                    // 提示用户选择第一个柱子
                    TaskDialog.Show("Revit", "请选择两根角柱");
                    Reference selectedColumn1 = uiDocument.Selection.PickObject(ObjectType.Element, new ColumnSelectionFilter(), "请选择第一个柱子");

                    // 获取第一个柱子的元素和位置
                    Element columnElement1 = document.GetElement(selectedColumn1.ElementId);

                    FamilyInstance columnInstance1 = columnElement1 as FamilyInstance;

                    if (columnInstance1 != null)
                    {
                        LocationPoint location1 = columnInstance1.Location as LocationPoint;
                        if (location1 != null)
                        {
                            columnPoint1 = location1.Point;
                        }
                    }

                    Reference selectedColumn2 = uiDocument.Selection.PickObject(ObjectType.Element, new ColumnSelectionFilter(), "请选择第二个柱子");

                    // 获取第二个柱子的元素和位置
                    Element columnElement2 = document.GetElement(selectedColumn2.ElementId);

                    FamilyInstance columnInstance2 = columnElement2 as FamilyInstance;

                    if (columnInstance2 != null)
                    {
                        LocationPoint location2 = columnInstance2.Location as LocationPoint;
                        if (location2 != null)
                        {
                            columnPoint2 = location2.Point;
                        }
                    }
                }


                using (Transaction transaction = new Transaction(document))
                {
                    transaction.Start("CreateRooftopRoom");

                    // 得到四个坐标点
                    XYZ columnPoint3 = new XYZ(columnPoint1.X, columnPoint2.Y, 0);
                    XYZ columnPoint4 = new XYZ(columnPoint2.X, columnPoint1.Y, 0);

                    //  创建梯屋柱
                    FilteredElementCollector columnCollector = new FilteredElementCollector(document).OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Columns);

                    FamilySymbol columnSymbol = columnCollector.Cast<FamilySymbol>().FirstOrDefault(symbol => symbol.Name.Contains("610 x 610mm"));

                    if (columnSymbol == null)
                    {
                        TaskDialog.Show("Error", $"未找到名为{columnSymbol.Name}的柱族。请先加载该族。");
                        transaction.RollBack();
                    }

                    // 确保族类型被激活（如果尚未激活）
                    if (!columnSymbol.IsActive)
                    {
                        columnSymbol.Activate();
                        document.Regenerate();
                    }

                    List<Level> allLevels = GetLevel(document);

                    List<XYZ> locations = new List<XYZ>();
                    locations.Add(columnPoint1);
                    locations.Add(columnPoint2);
                    locations.Add(columnPoint3);
                    locations.Add(columnPoint4);

                    foreach (XYZ location in locations)
                    {
                        FamilyInstance column = document.Create.NewFamilyInstance(location, columnSymbol, allLevels.Last(), Autodesk.Revit.DB.Structure.StructuralType.Column);
                    }




                    // 新建天面层标高，若已存在则不生成
                    FilteredElementCollector levelCollector = new FilteredElementCollector(document).OfClass(typeof(Level)).WhereElementIsNotElementType();
                    FilteredElementCollector viewCollector = new FilteredElementCollector(document).OfClass(typeof(ViewFamilyType));

                    string topLevelName = "天面层";
                    Level existingLevel = levelCollector.Cast<Level>().FirstOrDefault(level => level.Name == topLevelName);

                    if (existingLevel == null)
                    {
                        double mmToInch = 304.8; // 毫米转换为英寸
                        Level topLevel = Level.Create(document, allLevels.Last().Elevation + (4000/mmToInch));
                        topLevel.Name = topLevelName;
                        existingLevel = topLevel;

                        // 创建标高视图
                        foreach (ViewFamilyType viewFamilyType in viewCollector)
                        {
                            if (viewFamilyType.ViewFamily == ViewFamily.FloorPlan || viewFamilyType.ViewFamily == ViewFamily.CeilingPlan)
                            {
                                ViewPlan view = ViewPlan.Create(document, viewFamilyType.Id, existingLevel.Id);
                            }
                        }
                    }

                    // 获取梯屋的四条边界线
                    List<Line> roomLine = new List<Line>();
                    double elevation = existingLevel.Elevation;
                    roomLine.Add(Line.CreateBound(columnPoint1 + new XYZ(0, 0, elevation), columnPoint3 + new XYZ(0, 0, elevation)));
                    roomLine.Add(Line.CreateBound(columnPoint1 + new XYZ(0, 0, elevation), columnPoint4 + new XYZ(0, 0, elevation)));
                    roomLine.Add(Line.CreateBound(columnPoint2 + new XYZ(0, 0, elevation), columnPoint3 + new XYZ(0, 0, elevation)));
                    roomLine.Add(Line.CreateBound(columnPoint2 + new XYZ(0, 0, elevation), columnPoint4 + new XYZ(0, 0, elevation)));
                    
                    // 创建梁
                    FilteredElementCollector beamCollector = new FilteredElementCollector(document).OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_StructuralFraming);

                    FamilySymbol beamSymbol = beamCollector.Cast<FamilySymbol>().FirstOrDefault(symbol => symbol.Name.Contains("300 x 600mm"));

                    // 确保族类型被激活（如果尚未激活）
                    if (!beamSymbol.IsActive)
                    {
                        beamSymbol.Activate();
                        document.Regenerate();
                    }
                
                    foreach (Line beamLine in roomLine)
                    {
                        FamilyInstance beam = document.Create.NewFamilyInstance(beamLine, beamSymbol, existingLevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);                      
                    }

                    // 创建板
                    FloorType floorType = new FilteredElementCollector(document).OfClass(typeof(FloorType)).Cast<FloorType>().FirstOrDefault(ft => ft.Name.Equals("常规 - 150mm"));

                    // 调整楼板边界到当前标高的平面 (Z = elevation)
                    CurveArray profile = new CurveArray();
                    profile.Append(Line.CreateBound(columnPoint1 + new XYZ(0, 0, elevation), columnPoint3 + new XYZ(0, 0, elevation)));  // 底边
                    profile.Append(Line.CreateBound(columnPoint3 + new XYZ(0, 0, elevation), columnPoint2 + new XYZ(0, 0, elevation)));  // 右边
                    profile.Append(Line.CreateBound(columnPoint2 + new XYZ(0, 0, elevation), columnPoint4 + new XYZ(0, 0, elevation)));  // 顶边
                    profile.Append(Line.CreateBound(columnPoint4 + new XYZ(0, 0, elevation), columnPoint1 + new XYZ(0, 0, elevation)));  // 左边
                    
                    Floor floor = document.Create.NewFloor(profile, floorType, existingLevel, false);


                    // 创建墙
                    WallType wallType = new FilteredElementCollector(document).OfClass(typeof(WallType)).Cast<WallType>().FirstOrDefault(wt => wt.Name.Equals("常规 - 200mm"));
                    foreach (Line wallLine in roomLine)
                    {
                        Wall wall = Wall.Create(document, wallLine, wallType.Id, allLevels.Last().Id, 4000 / 304.8, 0.0, false, false);
                    }

                    transaction.Commit();
                }
              
            }
            catch (Exception error)
            {
                TaskDialog.Show("Revit", "角柱选取出现错误: " + error.Message);
                return;
            }

        }


        public string GetName()
        {
            return "CreateRooftopWall";
        }
    }
}
