using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point = OpenCvSharp.Point;

namespace Maple.Data
{
    public enum ImageFindTypes
    {
        ExactImage = 0,
        SimilarImage = 1,
        Traditional = 2
    }

    public static class ImageFinder
    {

        /*static Point2d[] MyPerspectiveTransform1(Point2f[] yourData, Mat transformationMatrix)
        {
            using (Mat src = new Mat(yourData.Length, 1, MatType.CV_32FC2, yourData))
            using (Mat dst = new Mat())
            {
                Cv2.PerspectiveTransform(src, dst, transformationMatrix);
                Point2f[] dstArray = new Point2f[dst.Rows * dst.Cols];
                dst.GetArray(0, 0, dstArray);
                Point2d[] result = Array.ConvertAll(dstArray, Point2fToPoint2d);
                return result;
            }
        }

        // fixed FromArray behavior
        static Point2d[] MyPerspectiveTransform2(Point2f[] yourData, Mat transformationMatrix)
        {
            using (MatOfPoint2f s = MatOfPoint2f.FromArray(yourData))
            using (MatOfPoint2f d = new MatOfPoint2f())
            {
                Cv2.PerspectiveTransform(s, d, transformationMatrix);
                Point2f[] f = d.ToArray();
                return f.Select(Point2fToPoint2d).ToArray();
            }
        }

        // new API
        static Point2d[] MyPerspectiveTransform3(Point2f[] yourData, Mat transformationMatrix)
        {
            Point2f[] ret = Cv2.PerspectiveTransform(yourData, transformationMatrix);
            return ret.Select(Point2fToPoint2d).ToArray();
        }

        static int VoteForSizeAndOrientation(KeyPoint[] modelKeyPoints, KeyPoint[] observedKeyPoints, DMatch[][] matches, Mat mask, float scaleIncrement, int rotationBins)
        {
            int idx = 0;
            int nonZeroCount = 0;
            byte[] maskMat = new byte[mask.Rows];
            GCHandle maskHandle = GCHandle.Alloc(maskMat, GCHandleType.Pinned);
            using (Mat m = new Mat(mask.Rows, 1, MatType.CV_8U, maskHandle.AddrOfPinnedObject()))
            {
                mask.CopyTo(m);
                List<float> logScale = new List<float>();
                List<float> rotations = new List<float>();
                double s, maxS, minS, r;
                maxS = -1.0e-10f; minS = 1.0e10f;

                //if you get an exception here, it's because you're passing in the model and observed keypoints backwards.  Just switch the order.
                for (int i = 0; i < maskMat.Length; i++)
                {
                    if (maskMat[i] > 0)
                    {
                        KeyPoint observedKeyPoint = observedKeyPoints[i];
                        KeyPoint modelKeyPoint = modelKeyPoints[matches[i][0].TrainIdx];
                        s = Math.Log10(observedKeyPoint.Size / modelKeyPoint.Size);
                        logScale.Add((float)s);
                        maxS = s > maxS ? s : maxS;
                        minS = s < minS ? s : minS;

                        r = observedKeyPoint.Angle - modelKeyPoint.Angle;
                        r = r < 0.0f ? r + 360.0f : r;
                        rotations.Add((float)r);
                    }
                }

                int scaleBinSize = (int)Math.Ceiling((maxS - minS) / Math.Log10(scaleIncrement));
                if (scaleBinSize < 2)
                    scaleBinSize = 2;
                float[] scaleRanges = { (float)minS, (float)(minS + scaleBinSize + Math.Log10(scaleIncrement)) };

                using (MatOfFloat scalesMat = new MatOfFloat(rows: logScale.Count, cols: 1, data: logScale.ToArray()))
                using (MatOfFloat rotationsMat = new MatOfFloat(rows: rotations.Count, cols: 1, data: rotations.ToArray()))
                using (MatOfFloat flagsMat = new MatOfFloat(logScale.Count, 1))
                using (Mat hist = new Mat())
                {
                    flagsMat.SetTo(new Scalar(0.0f));
                    float[] flagsMatFloat1 = flagsMat.ToArray();

                    int[] histSize = { scaleBinSize, rotationBins };
                    float[] rotationRanges = { 0.0f, 360.0f };
                    int[] channels = { 0, 1 };
                    Rangef[] ranges = { new Rangef(scaleRanges[0], scaleRanges[1]), new Rangef(rotations.Min(), rotations.Max()) };
                    double minVal, maxVal;

                    Mat[] arrs = { scalesMat, rotationsMat };
                    Cv2.CalcHist(arrs, channels, null, hist, 2, histSize, ranges);
                    Cv2.MinMaxLoc(hist, out minVal, out maxVal);

                    Cv2.Threshold(hist, hist, maxVal * 0.5, 0, ThresholdTypes.Tozero);
                    Cv2.CalcBackProject(arrs, channels, hist, flagsMat, ranges);

                    MatIndexer<float> flagsMatIndexer = flagsMat.GetIndexer();

                    for (int i = 0; i < maskMat.Length; i++)
                    {
                        if (maskMat[i] > 0)
                        {
                            if (flagsMatIndexer[idx++] != 0.0f)
                            {
                                nonZeroCount++;
                            }
                            else
                                maskMat[i] = 0;
                        }
                    }
                    m.CopyTo(mask);
                }
            }
            maskHandle.Free();

            return nonZeroCount;
        }

        private static void VoteForUniqueness(DMatch[][] matches, Mat mask, float uniqnessThreshold = 0.80f)
        {
            byte[] maskData = new byte[matches.Length];
            GCHandle maskHandle = GCHandle.Alloc(maskData, GCHandleType.Pinned);
            using (Mat m = new Mat(matches.Length, 1, MatType.CV_8U, maskHandle.AddrOfPinnedObject()))
            {
                mask.CopyTo(m);
                for (int i = 0; i < matches.Length; i++)
                {
                    //This is also known as NNDR Nearest Neighbor Distance Ratio
                    if ((matches[i][0].Distance / matches[i][1].Distance) <= uniqnessThreshold)
                        maskData[i] = 255;
                    else
                        maskData[i] = 0;
                }
                m.CopyTo(mask);
            }
            maskHandle.Free();
        }*/

        private static Point2d Point2fToPoint2d(Point2f pf) => new Point2d(((int)pf.X), ((int)pf.Y));

        private static List<System.Drawing.Point> FindPicFromImage(Bitmap imgSrc, List<Bitmap> subImageDataList, double maxThreshold = 0.9, double minThreshold = 0.9)
        {
            OpenCvSharp.Mat srcMat = null;
            OpenCvSharp.Mat dstMat = null;
            OpenCvSharp.OutputArray outArray = null;
            List<System.Drawing.Point> subImageDataLocations = new List<System.Drawing.Point>();
            //Window.ShowImages(imgSrc.ToMat());
            srcMat = imgSrc.ToMat().CvtColor(ColorConversionCodes.BGR2GRAY);
            foreach (var curSubImage in subImageDataList)
            {
                try
                {

                    dstMat = curSubImage.ToMat().CvtColor(ColorConversionCodes.BGR2GRAY);
                    outArray = OpenCvSharp.OutputArray.Create(srcMat);

                    OpenCvSharp.Cv2.MatchTemplate(srcMat, dstMat, outArray, TemplateMatchModes.SqDiff);
                    double minValue, maxValue;
                    OpenCvSharp.Point location, point;
                    OpenCvSharp.Cv2.MinMaxLoc(OpenCvSharp.InputArray.Create(outArray.GetMat()), out minValue, out maxValue, out location, out point);
                    //Console.WriteLine(maxValue);
                    if (maxValue >= maxThreshold && minValue <= minThreshold)
                    {
                        subImageDataLocations.Add(new System.Drawing.Point(location.X, location.Y));
                    }
                    //return new System.Drawing.Point(point.X, point.Y);
                }
                catch (Exception ex) {}
                finally
                {
                    if (dstMat != null)
                        dstMat.Dispose();
                    if (outArray != null)
                        outArray.Dispose();
                }
            }
            if (srcMat != null)
            {
                srcMat.Dispose();
            }
            return subImageDataLocations;
        }

        private static List<System.Drawing.Point> MatchPicBySurf(Bitmap imgSrc, List<Bitmap> imgSubDataList, double threshold = 400)
        {
            var surf = OpenCvSharp.XFeatures2D.SURF.Create(threshold, 4, 2, true, true);
            KeyPoint[] keyPointsSrc;
            Mat matSrc = imgSrc.ToMat();
            Mat matSrcRet = new Mat();
            List<System.Drawing.Point> returnData = new List<System.Drawing.Point>();
            surf.DetectAndCompute(matSrc, null, out keyPointsSrc, matSrcRet);
            int curNum = 0;
            foreach (var imgSub in imgSubDataList)
            {
                curNum++;
                Mat matTo = imgSub.ToMat();
                //imgSub.RotateFlip(RotateFlipType.RotateNoneFlipX);
                //Mat matReverseTo = imgSub.ToMat();

                Mat matToRet = new Mat();
                Mat matReverseToRect = new Mat();

                KeyPoint[] keyPointsTo;


                surf.DetectAndCompute(matTo, null, out keyPointsTo, matToRet);
                //surf.DetectAndCompute(matReverseTo, null, out reverseKeyPointsTo, matReverseToRect);

                using (var flnMatcher = new OpenCvSharp.FlannBasedMatcher())
                {
                    var matches = flnMatcher.Match(matSrcRet, matToRet);
                    //Finding the Minimum and Maximum Distance
                    double minDistance = 1000;//Backward approximation
                    double maxDistance = 0;
                    for (int i = 0; i < matSrcRet.Rows; i++)
                    {
                        double distance = matches[i].Distance;
                        if (distance > maxDistance)
                        {
                            maxDistance = distance;
                        }
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                        }
                    }
                    //Console.WriteLine($"max distance : {maxDistance}");
                    //Console.WriteLine($"min distance : {minDistance}");

                    var pointsSrc = new List<Point2f>();
                    var pointsDst = new List<Point2f>();
                    //Screening better matching points
                    var goodMatches = new List<DMatch>();
                    for (int i = 0; i < matSrcRet.Rows; i++)
                    {
                        double distance = matches[i].Distance;
                        if (distance <= Math.Max(minDistance, 0.02))
                        {
                            pointsSrc.Add(keyPointsSrc[matches[i].QueryIdx].Pt);
                            pointsDst.Add(keyPointsTo[matches[i].TrainIdx].Pt);
                            //Compression of new ones with distances less than ranges DMatch
                            goodMatches.Add(matches[i]);
                        }
                    }

                    var outMat = new Mat();

                    // algorithm RANSAC Filter the matched results
                    var pSrc = pointsSrc.ConvertAll(Point2fToPoint2d);
                    var pDst = pointsDst.ConvertAll(Point2fToPoint2d);
                    returnData.AddRange(pSrc.Select(x => { return new System.Drawing.Point((int)x.X, (int)x.Y); }));
                    /*foreach (var curReturnData in returnData) {
                        Input.SetMouseLocation(new Vector2(curReturnData.X, curReturnData.Y));
                    }*/
                    /*var outMask = new Mat();
                    // If the original matching result is null, Skip the filtering step
                    if (pSrc.Count > 0 && pDst.Count > 0)
                        Cv2.FindHomography(pSrc, pDst, HomographyMethods.Ransac, mask: outMask);
                    // If passed RANSAC After processing, the matching points are more than 10.,Only filters are used. Otherwise, use the original matching point result(When the matching point is too small, it passes through RANSAC After treatment,It's possible to get the result of 0 matching points.).
                    if (outMask.Rows > 10)
                    {
                        byte[] maskBytes = new byte[outMask.Rows * outMask.Cols];
                        outMask.GetArray(0, 0, maskBytes);
                        Cv2.DrawMatches(matSrc, keyPointsSrc, matTo, keyPointsTo, goodMatches, outMat, matchesMask: maskBytes, flags: DrawMatchesFlags.NotDrawSinglePoints);
                        //Cv2.DrawMatches(matSrc, keyPointsSrc, matReverseTo, reverseKeyPointsTo, goodMatches, outMat, matchesMask: maskBytes, flags: DrawMatchesFlags.NotDrawSinglePoints);
                    }
                    else
                    {
                        Cv2.DrawMatches(matSrc, keyPointsSrc, matTo, keyPointsTo, goodMatches, outMat, flags: DrawMatchesFlags.NotDrawSinglePoints);
                        //Cv2.DrawMatches(matSrc, keyPointsSrc, matReverseTo, reverseKeyPointsTo, goodMatches, outMat, flags: DrawMatchesFlags.NotDrawSinglePoints);
                    }
                    Bitmap outfile = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(outMat);
                    outfile.Save($"test{curNum}.png");*/
                }
            }


            return returnData;
        }

        public static bool WithinTolerance(Color colorA, Color colorB, int tol)
        {

            if (Math.Abs(colorA.R - colorB.R) + Math.Abs(colorA.B - colorB.B) + Math.Abs(colorA.R - colorB.R) < tol)
            {
                return true;
            }
            return false;
        }

        private static bool TraditionalFindBitmap(List<Bitmap> smallBmps, Bitmap bigBmp, int tol, out List<Vector2> locations)
        {
            IEnumerable<int> potentialSelections
                = Enumerable.Range(0, bigBmp.Width * bigBmp.Height);
            //List<Color> bigBitmapColors = new List<Color>();
            List<List<Color>> smallBitmapColors = new List<List<Color>>();
            //BitmapData bmpData =  bigBmp.LockBits(new Rectangle(0, 0, bigBmp.Width, bigBmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var time1 = DateTime.Now;
            List<Color> bigBitmapColors = Imaging.GetColorsFromBmp(bigBmp);
            var time2 = DateTime.Now;
            var time3 = DateTime.Now;
            var timeToComplete1 = time2 - time1;
            var timeToComplete2 = time3 - time2;
            List<int> smallBmpWidths = new List<int>();
            List<int> smallBmpHeights = new List<int>();
            List<int> curSmallPixels = new List<int>();
            foreach (var curSmallBmp in smallBmps)
            {
                smallBitmapColors.Add(Imaging.GetColorsFromBmp(curSmallBmp));
                smallBmpWidths.Add(curSmallBmp.Width);
                smallBmpHeights.Add(curSmallBmp.Height);
                curSmallPixels.Add(0);
            }

            int bigBmpWidth = bigBmp.Width;
            int bigBmpHeight = bigBmp.Height;
            ConcurrentBag<int> curPotentialSelections;
            Vector2 PixelToPixelCoordinate(int pixelLocation, int imageWidth)
            {
                return new Vector2(pixelLocation % imageWidth, pixelLocation / imageWidth);
            }
            locations = new List<Vector2>();
            var returnLocations = new List<int>();
            bool done = false;
            while (potentialSelections.Count() > 0)
            {

                curPotentialSelections = new ConcurrentBag<int>();
                Parallel.ForEach(potentialSelections, curPixel =>
                //foreach (var curPixel in potentialSelections)
                {
                    for (int curSmallBmpIterator = 0; curSmallBmpIterator < smallBitmapColors.Count; curSmallBmpIterator++)
                    {
                        var curSmallPixel = curSmallPixels[curSmallBmpIterator];
                        var smallBmpWidth = smallBmpWidths[curSmallBmpIterator];
                        var smallBmpHeight = smallBmpHeights[curSmallBmpIterator];
                        int curBigPixelX = curPixel % bigBmpWidth;
                        int curBigPixelY = curPixel / bigBmpWidth;
                        int curSmallPixelX = curSmallPixel % smallBmpWidth;
                        int curSmallPixelY = curSmallPixel / smallBmpWidth;
                        if (curPixel < bigBitmapColors.Count()
                            && curSmallPixel < smallBitmapColors[curSmallBmpIterator].Count())
                        {
                            if (WithinTolerance(smallBitmapColors[curSmallBmpIterator][curSmallPixel], bigBitmapColors[curPixel], tol))
                            {
                                if (curSmallPixel == smallBitmapColors[curSmallBmpIterator].Count() - 1)
                                {
                                    done = true;
                                    returnLocations = potentialSelections.Select(x => { return x - (bigBmpWidth * smallBmpHeight); }).ToList();
                                    return;
                                }
                                if (curSmallPixelX == smallBmpWidth - 1)
                                {
                                    curPotentialSelections.Add(curPixel + (bigBmpWidth - smallBmpWidth + 1));
                                    return;
                                }
                                else
                                {
                                    curPotentialSelections.Add(curPixel + 1);
                                    return;
                                }
                            }
                        }
                    }


                });
                if (done)
                {

                    locations = returnLocations.Select(x => { return PixelToPixelCoordinate(x, bigBmpWidth); }).ToList();
                    // correct height, basically reverse the Y
                    //locations = locations.Select(x => { return new Vector2(x.X, Math.Abs(x.Y - bigBmpHeight)); }).ToList();
                    return true;
                }
                potentialSelections = curPotentialSelections;
                for (int curSmallBmpIterator = 0; curSmallBmpIterator < smallBitmapColors.Count; curSmallBmpIterator++)
                {
                    curSmallPixels[curSmallBmpIterator]++;
                }

            }
            if (potentialSelections.Count() == 0)
            {
                return false;
            }
            return true;
        }

        public static bool FindImage(ImageFindTypes imageFindType, Bitmap sourceImage, List<Bitmap> subImageDataList, out List<Vector2> subImageLocations, int tolerance = 180)
        {
            List<System.Drawing.Point> returnData = null;
            switch (imageFindType)
            {
                case ImageFindTypes.ExactImage:
                    returnData = FindPicFromImage(sourceImage, subImageDataList);
                    break;
                case ImageFindTypes.SimilarImage:
                    returnData = MatchPicBySurf(sourceImage, subImageDataList);
                    break;
                case ImageFindTypes.Traditional:
                    return TraditionalFindBitmap(subImageDataList, sourceImage, tolerance, out subImageLocations);
            }
            subImageLocations = returnData.Select(x => new Vector2(x.X, x.Y)).ToList();
            return subImageLocations.Any();
        }
    }
}