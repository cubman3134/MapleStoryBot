using Accord.Math;
using Accord.Math.Optimization.Losses;
using Accord.Statistics.Models.Regression.Linear;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Data
{
    public class Vector2
    {
        public double X;
        public double Y;
        public Vector2(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public class MapleMath
    {
        public struct Line
        {
            public double x1 { get; set; }
            public double y1 { get; set; }

            public double x2 { get; set; }
            public double y2 { get; set; }

            public Line(Vector2 begin, Vector2 end)
            {
                x1 = begin.X;
                x2 = end.X;
                y1 = begin.Y;
                y2 = end.Y;
            }
        }

        public class LineIntersection
        {
            //  Returns Point of intersection if do intersect otherwise default Point (null)
            public static bool FindIntersection(Line lineA, Line lineB, out Vector2 intersection, double tolerance = 0.001)
            {
                intersection = null;
                double x1 = lineA.x1, y1 = lineA.y1;
                double x2 = lineA.x2, y2 = lineA.y2;

                double x3 = lineB.x1, y3 = lineB.y1;
                double x4 = lineB.x2, y4 = lineB.y2;

                // equations of the form x = c (two vertical lines)
                if (Math.Abs(x1 - x2) < tolerance && Math.Abs(x3 - x4) < tolerance && Math.Abs(x1 - x3) < tolerance)
                {
                    // Both lines overlap vertically, ambiguous intersection points.
                    return false;
                }

                //equations of the form y=c (two horizontal lines)
                if (Math.Abs(y1 - y2) < tolerance && Math.Abs(y3 - y4) < tolerance && Math.Abs(y1 - y3) < tolerance)
                {
                    // Both lines overlap horizontally, ambiguous intersection points.
                    return false;
                }

                //equations of the form x=c (two vertical parallel lines)
                if (Math.Abs(x1 - x2) < tolerance && Math.Abs(x3 - x4) < tolerance)
                {
                    //return default (no intersection)
                    return false;
                }

                //equations of the form y=c (two horizontal parallel lines)
                if (Math.Abs(y1 - y2) < tolerance && Math.Abs(y3 - y4) < tolerance)
                {
                    //return default (no intersection)
                    return false;
                }

                //general equation of line is y = mx + c where m is the slope
                //assume equation of line 1 as y1 = m1x1 + c1 
                //=> -m1x1 + y1 = c1 ----(1)
                //assume equation of line 2 as y2 = m2x2 + c2
                //=> -m2x2 + y2 = c2 -----(2)
                //if line 1 and 2 intersect then x1=x2=x & y1=y2=y where (x,y) is the intersection point
                //so we will get below two equations 
                //-m1x + y = c1 --------(3)
                //-m2x + y = c2 --------(4)

                double x, y;

                //lineA is vertical x1 = x2
                //slope will be infinity
                //so lets derive another solution
                if (Math.Abs(x1 - x2) < tolerance)
                {
                    //compute slope of line 2 (m2) and c2
                    double m2 = (y4 - y3) / (x4 - x3);
                    double c2 = -m2 * x3 + y3;

                    //equation of vertical line is x = c
                    //if line 1 and 2 intersect then x1=c1=x
                    //subsitute x=x1 in (4) => -m2x1 + y = c2
                    // => y = c2 + m2x1 
                    x = x1;
                    y = c2 + m2 * x1;
                }
                //lineB is vertical x3 = x4
                //slope will be infinity
                //so lets derive another solution
                else if (Math.Abs(x3 - x4) < tolerance)
                {
                    //compute slope of line 1 (m1) and c2
                    double m1 = (y2 - y1) / (x2 - x1);
                    double c1 = -m1 * x1 + y1;

                    //equation of vertical line is x = c
                    //if line 1 and 2 intersect then x3=c3=x
                    //subsitute x=x3 in (3) => -m1x3 + y = c1
                    // => y = c1 + m1x3 
                    x = x3;
                    y = c1 + m1 * x3;
                }
                //lineA & lineB are not vertical 
                //(could be horizontal we can handle it with slope = 0)
                else
                {
                    //compute slope of line 1 (m1) and c2
                    double m1 = (y2 - y1) / (x2 - x1);
                    double c1 = -m1 * x1 + y1;

                    //compute slope of line 2 (m2) and c2
                    double m2 = (y4 - y3) / (x4 - x3);
                    double c2 = -m2 * x3 + y3;

                    //solving equations (3) & (4) => x = (c1-c2)/(m2-m1)
                    //plugging x value in equation (4) => y = c2 + m2 * x
                    x = (c1 - c2) / (m2 - m1);
                    y = c2 + m2 * x;

                    //verify by plugging intersection point (x, y)
                    //in orginal equations (1) & (2) to see if they intersect
                    //otherwise x,y values will not be finite and will fail this check
                    if (!(Math.Abs(-m1 * x + y - c1) < tolerance
                        && Math.Abs(-m2 * x + y - c2) < tolerance))
                    {
                        //return default (no intersection)
                        return false;
                    }
                }

                //x,y can intersect outside the line segment since line is infinitely long
                //so finally check if x, y is within both the line segments
                if (IsInsideLine(lineA, x, y) &&
                    IsInsideLine(lineB, x, y))
                {
                    intersection = new Vector2(x, y);
                    return true;
                }

                //return default (no intersection)
                return false;

            }

            // Returns true if given point(x,y) is inside the given line segment
            private static bool IsInsideLine(Line line, double x, double y)
            {
                return (x >= line.x1 && x <= line.x2
                            || x >= line.x2 && x <= line.x1)
                       && (y >= line.y1 && y <= line.y2
                            || y >= line.y2 && y <= line.y1);
            }
        }

        internal class PolynomialRegressionData
        {
            private PolynomialRegression poly;
            public PolynomialRegressionData(List<Vector2> pointData, int maxDegree = 100)
            {
                double[] inputs = pointData.Select(x => (double)x.X).ToArray();
                double[] outputs = pointData.Select(x => (double)x.Y).ToArray();
                poly = null;
                double[] pred;
                int degree = 1;
                double error = 0.0;
                do
                {
                    var ls = new PolynomialLeastSquares()
                    {
                        Degree = degree++
                    };
                    try
                    {
                        poly = ls.Learn(inputs, outputs);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    //string str = poly.ToString("N1");
                    //double[] weights = poly.Weights;
                    //double intercept = poly.Intercept; 

                    // Finally, we can use this polynomial
                    // to predict values for the input data
                    pred = poly.Transform(inputs);
                    error = new SquareLoss(outputs).Loss(pred); // 0.0
                    if (degree > maxDegree)
                    {
                        return;
                    }
                } while (error > 0.0001);
            }

            public List<Vector2> PredictData(List<int> xValues)
            {
                // We can create a learning algorithm
                //var xVals = Enumerable.Range((int)inputs[0], (int)(inputs[inputs.Length - 1] - inputs[0]));
                double[] pred = poly.Transform(xValues.Select(Convert.ToDouble).ToArray());
                List<Vector2> returnData = new List<Vector2>();
                for (int i = 0; i < pred.Length; i++)
                {
                    returnData.Add(new Vector2((int)xValues.ElementAt(i), (int)pred[i]));
                }
                return returnData;
            }

            public List<double> GetCoefficients()
            {
                if (poly == null || poly.Weights == null)
                {
                    return new List<double>();
                }
                List<double> coefficients = poly.Weights.ToList();
                coefficients.Add(poly.Intercept);
                return coefficients;
            }
        }

        public static List<MapPiece> FindRoute(MapData map, MapPiece sourceNode, MapPiece destinationNode)
        {
            List<MapPiece> path = new List<MapPiece>();
            path.Add(sourceNode);

            MapPiece currentNode = sourceNode;
            while (true)
            {
                //get all neighbors of current-node (nodes within transmission range)
                List<MapPiece> allNeighbors = currentNode.MapPieceLinkDataList.Select(x => x.JoiningMapPiece).ToList();

                //remove neighbors that are already added to path
                IEnumerable<MapPiece> neighbors = from neighbor in allNeighbors
                                                  where !path.Contains(neighbor)
                                                  select neighbor;

                //stop if no neighbors or destination reached
                if (neighbors.Count() == 0) break;
                if (neighbors.Contains(destinationNode))
                {
                    path.Add(destinationNode);
                    break;
                }

                //choose next-node (the neighbor with shortest distance to destination)
                double bestMinDist = double.MaxValue;
                MapPiece nearestNode = null;
                foreach (var curNeighbor in neighbors)
                {
                    var curMapPieceLink = curNeighbor.MapPieceLinkDataList.Where(x => x.JoiningMapPiece.Beginning.X == destinationNode.Beginning.X && x.JoiningMapPiece.Beginning.Y == destinationNode.Beginning.Y).FirstOrDefault();
                    if (curMapPieceLink == null)
                    {
                        continue;
                    }
                    var curMinDist = curMapPieceLink.MinimumDistance;
                    if (curMinDist < bestMinDist)
                    {
                        bestMinDist = curMinDist;
                        nearestNode = curNeighbor;
                    }
                }
                if (nearestNode == null)
                {
                    nearestNode = neighbors.OrderBy(x => MapleMath.PixelCoordinateDistance(x.Beginning, destinationNode.Beginning)).First(); // todo this isnt good
                }
                path.Add(nearestNode);
                currentNode = nearestNode;
            }

            return (path);
        }

        /*public static Vector2 CorrectImageHeight(Vector2 pixelCoordinate, int imageHeight)
        {
            return new Vector2(pixelCoordinate.X, Math.Abs(pixelCoordinate.Y - imageHeight));
        }*/

        public static int PixelCoordinateToPixel(Vector2 pixelLocation, int imageWidth)
        {
            return (int)(pixelLocation.Y * imageWidth + pixelLocation.X);
        }

        public static double PixelCoordinateDistance(Vector2 pixelCoordinate1, Vector2 pixelCoordinate2)
        {
            return Math.Sqrt(Math.Pow(pixelCoordinate2.X - pixelCoordinate1.X, 2) + Math.Pow(pixelCoordinate2.Y - pixelCoordinate1.Y, 2));
        }

        public static Vector2 MapCoordinatesToMiniMapCoordinates(Vector2 mapCoordinates, Vector2 playerScreenLocation, Vector2 playerMinimapLocation)
        {
            double mapCoordinatesChangeX = (mapCoordinates.X - playerScreenLocation.X) * MapData.MinimapToPixelRatio;
            double mapCoordinatesChangeY = (mapCoordinates.Y - playerScreenLocation.Y) * MapData.MinimapToPixelRatio;
            return new Vector2((int)(playerMinimapLocation.X + mapCoordinatesChangeX), (int)(playerMinimapLocation.Y + mapCoordinatesChangeY));
        }

        public static bool LocationWithinBounds(Vector2 locationToCheck, Vector2 minLocation, Vector2 maxLocation)
        {
            return (locationToCheck.X >= minLocation.X && locationToCheck.X <= maxLocation.X
                && locationToCheck.Y >= minLocation.Y && locationToCheck.Y <= maxLocation.Y);
        }

        public static Vector2 GetAverage(List<Vector2> coordinateDataList)
        {
            double totalX = 0;
            double totalY = 0;
            foreach (var curCoordinate in coordinateDataList)
            {
                totalX += curCoordinate.X;
                totalY += curCoordinate.Y;
            }
            return new Vector2(totalX / coordinateDataList.Count, totalY / coordinateDataList.Count);
        }

        public static List<double> PolynomialRegressionCoefficients(List<Vector2> pointData)
        {
            PolynomialRegressionData poly = new PolynomialRegressionData(pointData);
            return poly.GetCoefficients();
        }

        public static List<Vector2> PolynomialLeastSquares(List<Vector2> pointData)
        {
            PolynomialRegressionData poly = new PolynomialRegressionData(pointData);
            var xVals = Enumerable.Range((int)pointData[0].X, (int)(pointData[pointData.Count - 1].X - pointData[0].X));
            return poly.PredictData(xVals.ToList());
        }

    }
}
