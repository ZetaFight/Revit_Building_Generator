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
    class CreateFrame : IExternalEventHandler
    {
        private List<XYZ> GetGridIntersection(Document document)
        {
            // 获取轴网的交点
            FilteredElementCollector gridCollector = new FilteredElementCollector(document).OfClass(typeof(Grid)).WhereElementIsNotElementType();

            List<XYZ> intersectionPoints = new List<XYZ>();

            // 获取轴网的几何曲线
            List<Grid> grids = new List<Grid>();

            foreach (Grid grid in gridCollector)
            {
                grids.Add(grid);
            }

            // 两两计算轴网交点
            for (int i = 0; i < grids.Count; i++)
            {
                for (int j = i + 1; j < grids.Count; j++)
                {
                    Curve curve1 = grids[i].Curve;
                    Curve curve2 = grids[j].Curve;

                    // 检查两个轴网是否相交，并获取交点
                    SetComparisonResult result = curve1.Intersect(curve2, out IntersectionResultArray intersectionResultArray);

                    if (result == SetComparisonResult.Overlap && intersectionResultArray != null)
                    {
                        foreach (IntersectionResult intersectionResult in intersectionResultArray)
                        {
                            // 获取交点坐标
                            XYZ intersectionPoint = intersectionResult.XYZPoint;
                            intersectionPoints.Add(intersectionPoint);
                        }
                    }
                }
            }
            return intersectionPoints;
        }

        private List<Level> GetLevel(Document document)
        {
            FilteredElementCollector levelCollector = new FilteredElementCollector(document).OfClass(typeof(Level)).WhereElementIsNotElementType();
            List<Level> levelsNeeded = levelCollector.Cast<Level>().Where(level => level.Name.Contains("标高")).ToList();

            return levelsNeeded;
        }

        public void Execute(UIApplication app)
        {
            Document document = app.ActiveUIDocument.Document;

            List<XYZ> intersections = GetGridIntersection(document);
           
            List<Level> allLevels = GetLevel(document);
            List<Level> columnLevels = allLevels.Take(allLevels.Count - 1).ToList();
            List<Level> beamLevels = allLevels.Skip(1).ToList();

            using (Transaction transaction = new Transaction(document))
            {
                try
                {
                    transaction.Start("CreateFrame");

                    // 先生成柱
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

                    foreach (Level level in columnLevels)
                    {
                        foreach (XYZ location in intersections)
                        {
                            FamilyInstance column = document.Create.NewFamilyInstance(location, columnSymbol, level, Autodesk.Revit.DB.Structure.StructuralType.Column);
                        }
                    }


                    // 再生成梁

                    FilteredElementCollector beamCollector = new FilteredElementCollector(document).OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_StructuralFraming);

                    FamilySymbol beamSymbol = beamCollector.Cast<FamilySymbol>().FirstOrDefault(symbol => symbol.Name.Contains("300 x 600mm"));

                    if (beamSymbol == null)
                    {
                        TaskDialog taskDialog = new TaskDialog("提示");

                        taskDialog.MainInstruction = "未找到名为 '300 x 600mm' 的梁族。";
                        taskDialog.MainContent = "即将尝试加载该族。";

                        taskDialog.CommonButtons = TaskDialogCommonButtons.Ok;
                        taskDialog.Show();

                        string familyPath = @"C:\ProgramData\Autodesk\RVT 2018\Libraries\China\结构\框架\混凝土\混凝土 - 矩形梁.rfa";

                        // 检查文件是否存在
                        if (!File.Exists(familyPath))
                        {
                            TaskDialog.Show("Error", "族文件未找到：" + familyPath);
                            transaction.RollBack();
                        }

                        Family family;
                        bool success = document.LoadFamily(familyPath, out family);

                        if (success)
                        {
                            TaskDialog.Show("Revit", "族文件加载成功：" + family.Name);
                            beamSymbol = beamCollector.Cast<FamilySymbol>().FirstOrDefault(symbol => symbol.Name.Contains("300 x 600mm"));
                        }
                        else
                        {
                            TaskDialog.Show("Error", "族文件加载失败。");
                            transaction.RollBack();
                        }

                    }


                    // 确保族类型被激活（如果尚未激活）
                    if (!beamSymbol.IsActive)
                    {
                        beamSymbol.Activate();
                        document.Regenerate();
                    }

                    // 生成竖向的梁
                    XYZ maxYPoint = intersections.OrderByDescending(p => p.Y).FirstOrDefault();
                    double maxY = maxYPoint.Y;
                    List<XYZ> pointsOnXAxis = intersections.Where(p => p.Y == 0 && p.Z == 0).ToList();

                    foreach (Level level in beamLevels)
                    {
                        foreach (XYZ XPoints in pointsOnXAxis)
                        {
                            Line beamLine = Line.CreateBound(XPoints + new XYZ(0, 0, level.Elevation), XPoints + new XYZ(0, maxY, level.Elevation));
                            FamilyInstance beam = document.Create.NewFamilyInstance(beamLine, beamSymbol, level, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                        }
                    }

                    // 生成横向的梁
                    XYZ maxXPoint = intersections.OrderByDescending(p => p.X).FirstOrDefault();
                    double maxX = maxXPoint.X;
                    List<XYZ> pointsOnYAxis = intersections.Where(p => p.X == 0 && p.Z == 0).ToList();

                    foreach (Level level in beamLevels)
                    {
                        foreach (XYZ YPoints in pointsOnYAxis)
                        {
                            Line beamLine = Line.CreateBound(YPoints + new XYZ(0, 0, level.Elevation), YPoints + new XYZ(maxX, 0, level.Elevation));
                            FamilyInstance beam = document.Create.NewFamilyInstance(beamLine, beamSymbol, level, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                        }
                    }

                    transaction.Commit();
                    TaskDialog.Show("Revit", $"梁柱框架生成完成。\n柱族：{columnSymbol.Name}；\n梁族：{beamSymbol.Name}");

                }
                catch (Exception error)
                {
                    TaskDialog.Show("Revit", "梁柱框架生成出现错误:" + error.Message);
                    transaction.RollBack();

                }
            }


        }

        public string GetName()
        {
            return "CreateFrame";
        }
    }
}
