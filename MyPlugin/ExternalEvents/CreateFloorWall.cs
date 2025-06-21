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
    class CreateFloorWall : IExternalEventHandler
    {
        private List<XYZ> GetGridCornerPoint(Document document)
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

            if (!intersectionPoints.Any())
            {
                return new List<XYZ>(); // 如果没有找到交点，返回空列表
            }

            // 找到X和Y的最小值和最大值
            double minX = intersectionPoints.Min(p => p.X);
            double maxX = intersectionPoints.Max(p => p.X);
            double minY = intersectionPoints.Min(p => p.Y);
            double maxY = intersectionPoints.Max(p => p.Y);

            // 构建四个角点
            List<XYZ> cornerPoints = new List<XYZ>
            {
                new XYZ(minX, minY, 0), // (0, 0, 0)
                new XYZ(maxX, minY, 0), // (Xmax, 0, 0)
                new XYZ(minX, maxY, 0), // (0, Ymax, 0)
                new XYZ(maxX, maxY, 0)  // (Xmax, Ymax, 0)
            };

            return cornerPoints;
        }

        private List<XYZ> GetGridEdgePoint(Document document)
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

            if (!intersectionPoints.Any())
            {
                return new List<XYZ>(); // 如果没有找到交点，返回空列表
            }

            // 找到X和Y的最小值和最大值
            double minX = intersectionPoints.Min(p => p.X);
            double maxX = intersectionPoints.Max(p => p.X);
            double minY = intersectionPoints.Min(p => p.Y);
            double maxY = intersectionPoints.Max(p => p.Y);

            // 构建边缘点
            List<XYZ> edgePoints = new List<XYZ>();
            foreach (XYZ p in intersectionPoints)
            {
                if (p.Y == 0 || p.Y == maxY || p.X == maxX || p.X == 0)
                {
                    edgePoints.Add(p);
                }
            }
          
            return edgePoints;
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
            
            using (Transaction transaction = new Transaction(document))
            {
                try
                {
                    transaction.Start("CreateFloorWall");

                    double offset = 305 / 304.8;

                    List<XYZ> gridCornerPoints = GetGridCornerPoint(document);

                    // 标高
                    List<Level> allLevels = GetLevel(document);

                    // 先生成板
                    FloorType floorType = new FilteredElementCollector(document).OfClass(typeof(FloorType)).Cast<FloorType>().FirstOrDefault(ft => ft.Name.Equals("常规 - 150mm"));
                    if (floorType == null)
                    {
                        TaskDialog.Show("Error", $"未找到楼板类型{floorType.Name}。");
                        transaction.RollBack();
                    }

                    foreach (Level level in allLevels)
                    {

                        double elevation = level.Elevation; // 获取标高的Z值
                        // 调整楼板边界到当前标高的平面 (Z = elevation)
                        XYZ conrnerPoint1 = gridCornerPoints[0] + new XYZ(-offset, -offset, elevation);
                        XYZ conrnerPoint2 = gridCornerPoints[1] + new XYZ(+offset, -offset, elevation);
                        XYZ conrnerPoint3 = gridCornerPoints[2] + new XYZ(-offset, +offset, elevation);
                        XYZ conrnerPoint4 = gridCornerPoints[3] + new XYZ(+offset, +offset, elevation);

                        if (level.Name.Equals("标高 1"))
                        {
                            // 创建楼板边界
                            XYZ up = new XYZ(0, 0, 150 / 304.8);
                            CurveArray profile = new CurveArray();
                            profile.Append(Line.CreateBound(conrnerPoint1 + up, conrnerPoint2 + up));  // 底边
                            profile.Append(Line.CreateBound(conrnerPoint2 + up, conrnerPoint4 + up));  // 右边
                            profile.Append(Line.CreateBound(conrnerPoint4 + up, conrnerPoint3 + up));  // 顶边
                            profile.Append(Line.CreateBound(conrnerPoint3 + up, conrnerPoint1 + up));  // 左边
                            Floor groundFloor = document.Create.NewFloor(profile, floorType, level, false);
                        }
                        else
                        {
                            // 创建楼板边界
                            CurveArray profile = new CurveArray();
                            profile.Append(Line.CreateBound(conrnerPoint1, conrnerPoint2));  // 底边
                            profile.Append(Line.CreateBound(conrnerPoint2, conrnerPoint4));  // 右边
                            profile.Append(Line.CreateBound(conrnerPoint4, conrnerPoint3));  // 顶边
                            profile.Append(Line.CreateBound(conrnerPoint3, conrnerPoint1));  // 左边
                            Floor floor = document.Create.NewFloor(profile, floorType, level, false);
                        }

                    }
                    
                    // 再生成外墙
                    List<XYZ> edgePoints = GetGridEdgePoint(document);
                    double maxY = edgePoints.Max(p => p.Y);
                    double maxX = edgePoints.Max(p => p.X);

                    List<XYZ> bottomEdgePoints = new List<XYZ>();
                    List<XYZ> upEdgePoints = new List<XYZ>();
                    List<XYZ> leftEdgePoints = new List<XYZ>();
                    List<XYZ> rightEdgePoints = new List<XYZ>();

                    foreach (XYZ p in edgePoints)
                    {
                        if (p.Y == 0)
                        {
                            bottomEdgePoints.Add(p);
                        }
                        else if (p.Y == maxY)
                        {
                            upEdgePoints.Add(p);
                        }
                    }

                    foreach (XYZ p in edgePoints)
                    {
                        if (p.X == 0)
                        {
                            leftEdgePoints.Add(p);
                        }
                        else if (p.X == maxX)
                        {
                            rightEdgePoints.Add(p);
                        }
                    }
                 
                    List<Line> wallLineList = new List<Line>();

                    for (int i = 0; i < bottomEdgePoints.Count - 1; ++i)
                    {
                        wallLineList.Add(Line.CreateBound(bottomEdgePoints[i], bottomEdgePoints[i + 1]));
                    }
                    for (int i = 0; i < upEdgePoints.Count - 1; ++i)
                    {
                        wallLineList.Add(Line.CreateBound(upEdgePoints[i], upEdgePoints[i + 1]));
                    }
                    for (int i = 0; i < leftEdgePoints.Count - 1; ++i)
                    {
                        wallLineList.Add(Line.CreateBound(leftEdgePoints[i], leftEdgePoints[i + 1]));
                    }
                    for (int i = 0; i < rightEdgePoints.Count - 1; ++i)
                    {
                        wallLineList.Add(Line.CreateBound(rightEdgePoints[i], rightEdgePoints[i + 1]));
                    }

                    WallType wallType = new FilteredElementCollector(document).OfClass(typeof(WallType)).Cast<WallType>().FirstOrDefault(wt => wt.Name.Equals("常规 - 200mm"));  

                    if (wallType == null)
                    {
                        TaskDialog.Show("Error", $"未找到墙体类型{wallType.Name}。");
                        transaction.RollBack();
                    }

                    
                    for (int i = 0; i < allLevels.Count; ++i)
                    {
                        if (i == allLevels.Count - 1)
                        {
                            continue;
                        }
                        else
                        {
                            foreach (Line wallLine in wallLineList)
                            {
                                double height = allLevels[i + 1].Elevation - allLevels[i].Elevation;       
                                Wall wall = Wall.Create(document, wallLine, wallType.Id, allLevels[i].Id, height, 0.0, false, false);
                            }
                        }
                    }

                    foreach (Line wallLine in wallLineList)
                    {
                        double height = 1200 / 304.8;
                        Wall wall = Wall.Create(document, wallLine, wallType.Id, allLevels.Last().Id, height, 0.0, false, false);
                    }


                    transaction.Commit();
                    TaskDialog.Show("Revit", $"板和外墙生成完成。\n板族：{floorType.Name}\n墙族：{wallType.Name}");
                }
                catch (Exception error)
                {
                    TaskDialog.Show("Revit", "板墙生成出现错误:" + error.Message);
                    transaction.RollBack();
                }
            }
        }

        public string GetName()
        {
            return "CreateFloorWall";
        }
    }
}
