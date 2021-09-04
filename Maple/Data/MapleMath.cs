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
        public int X;
        public int Y;
        public Vector2(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class MapleMath
    {
        public static Vector2 PixelToPixelCoordinate(int pixelLocation, int imageWidth)
        {
            return new Vector2(pixelLocation % imageWidth, pixelLocation / imageWidth);
        }

        public static int PixelCoordinateToPixel(Vector2 pixelLocation, int imageWidth)
        {
            return pixelLocation.Y * imageWidth + pixelLocation.X;
        }

        public static double PixelCoordinateDistance(Vector2 pixelCoordinate1, Vector2 pixelCoordinate2)
        {
            return Math.Sqrt(Math.Pow(pixelCoordinate2.X - pixelCoordinate1.X, 2) + Math.Pow(pixelCoordinate2.Y - pixelCoordinate1.Y, 2));
        }

        public static Vector2 MapCoordinatesToMiniMapCoordinates(Vector2 mapCoordinates)
        {
            return new Vector2((int)(mapCoordinates.X * MapData.MinimapToPixelRatio), (int)(mapCoordinates.Y * MapData.MinimapToPixelRatio));
        }

        public static bool LocationWithinBounds(Vector2 locationToCheck, Vector2 minLocation, Vector2 maxLocation)
        {
            return (locationToCheck.X >= minLocation.X && locationToCheck.X <= maxLocation.X
                && locationToCheck.Y >= minLocation.Y && locationToCheck.Y <= maxLocation.Y);
        }

        public static Vector2 GetAverage(List<Vector2> coordinateDataList)
        {
            int totalX = 0;
            int totalY = 0;
            foreach (var curCoordinate in coordinateDataList)
            {
                totalX += curCoordinate.X;
                totalY += curCoordinate.Y;
            }
            return new Vector2(totalX / coordinateDataList.Count, totalY / coordinateDataList.Count);
        }

        public static List<Vector2> PolynomialLeastSquares(List<Vector2> pointData, int degree)
        {
            double[] inputs = pointData.Select(x => (double)x.X).ToArray();
            double[] outputs = pointData.Select(x => (double)x.Y).ToArray();
            double error = 0.0;
            PolynomialRegression poly = null;
            double[] pred;
            do
            {
                var ls = new PolynomialLeastSquares()
                {
                    Degree = degree++
                };
                poly = ls.Learn(inputs, outputs);
                //string str = poly.ToString("N1");
                //double[] weights = poly.Weights;
                //double intercept = poly.Intercept; 
                
                // Finally, we can use this polynomial
                // to predict values for the input data
                pred = poly.Transform(inputs);
                error = new SquareLoss(outputs).Loss(pred); // 0.0
            } while (error > 0.0001);
            // We can create a learning algorithm
            var xVals = Enumerable.Range((int)inputs[0], (int)(inputs[inputs.Length - 1] - inputs[0]));
            pred = poly.Transform(xVals.Select(Convert.ToDouble).ToArray());
            List<Vector2> returnData = new List<Vector2>();
            for (int i = 0; i < pred.Length; i++)
            {
                returnData.Add(new Vector2((int)xVals.ElementAt(i), (int)pred[i]));
            }
            return returnData;
        }

    }
}
