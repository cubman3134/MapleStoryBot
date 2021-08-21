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

    }
}
