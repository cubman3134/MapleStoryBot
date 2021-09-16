using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Diagnostics;
using IronOcr;


namespace Maple.Data
{
    [Serializable]
    public struct SerializableBitmapImageWrapper : ISerializable
    {
        readonly BitmapImage bitmapImage;

        public static implicit operator BitmapImage(SerializableBitmapImageWrapper wrapper)
        {
            return wrapper.BitmapImage;
        }

        public static implicit operator SerializableBitmapImageWrapper(BitmapImage bitmapImage)
        {
            return new SerializableBitmapImageWrapper(BitmapImageToBitmap(bitmapImage));
        }

        private static Bitmap BitmapImageToBitmap(BitmapImage bitmapImageData)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImageData));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        public BitmapImage BitmapImage { get { return bitmapImage; } }

        public Bitmap BitmapData { get { return BitmapImageToBitmap(BitmapImage); } }

        public SerializableBitmapImageWrapper(Bitmap bitmapData)
        {
            var memory = new MemoryStream();
            bitmapData.Save(memory, ImageFormat.Png);
            memory.Position = 0;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            memory = null;
            this.bitmapImage = bitmapImage;
        }

        public SerializableBitmapImageWrapper(SerializationInfo info, StreamingContext context)
        {
            byte[] imageBytes = (byte[])info.GetValue("image", typeof(byte[]));
            if (imageBytes == null)
                bitmapImage = null;
            else
            {
                using (var ms = new MemoryStream(imageBytes))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmapImage = bitmap;
                }
            }
        }

        #region ISerializable Members

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            byte[] imageBytes;
            if (bitmapImage == null)
                imageBytes = null;
            else
                using (var ms = new MemoryStream())
                {
                    BitmapImage.SaveToPng(ms);
                    imageBytes = ms.ToArray();
                }
            info.AddValue("image", imageBytes);
        }

        #endregion
    }

    public static class BitmapHelper
    {
        public static void SaveToPng(this BitmapSource bitmap, Stream stream)
        {
            var encoder = new PngBitmapEncoder();
            SaveUsingEncoder(bitmap, stream, encoder);
        }

        public static void SaveUsingEncoder(this BitmapSource bitmap, Stream stream, BitmapEncoder encoder)
        {
            BitmapFrame frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);
            encoder.Save(stream);
        }

        public static BitmapImage FromUri(string path)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path);
            bitmap.EndInit();
            return bitmap;
        }
    }

    public class PhotoWithTimestamp
    {
        public Bitmap Photo;
        public DateTime TimeStamp;

        public PhotoWithTimestamp(Bitmap photo)
        {
            TimeStamp = DateTime.Now;
            Photo = photo;
        }
    }

    public class PhotoTaker
    {
        private static Thread _photoTakerThread;
        private static int _millisecondRefreshRate;
        private static List<PhotoWithTimestamp> _photoWithTimestampDataList;
        public static void StartTakingImages(int millisecondRefreshRate)
        {
            _millisecondRefreshRate = millisecondRefreshRate;
            _photoWithTimestampDataList = new List<PhotoWithTimestamp>();
            _photoTakerThread = new Thread(PhotoTakerWorker) { Name = "Photo Taker Thread" };
            _photoTakerThread.Start();
        }

        public static void StopTakingImages()
        {
            _photoTakerThread.Abort();
            //_photoTakerThread.Join();
            _photoTakerThread = null;
        }

        public static List<PhotoWithTimestamp> GetPhotosAndDeleteFromMemory()
        {
            List<PhotoWithTimestamp> returnData = _photoWithTimestampDataList;
            //_photoWithTimestampDataList.Clear();
            return returnData;
        }

        private static void PhotoTakerWorker()
        {
            while (true)
            {
                try
                {
                    if (!Imaging.GetCurrentGameScreen(out Bitmap currentBitmap))
                    {
                        continue;
                    }
                    _photoWithTimestampDataList.Add(new PhotoWithTimestamp(currentBitmap));
                    Thread.Sleep(_millisecondRefreshRate);
                }
                catch (Exception ex)
                {
                    return;
                }
            }
        }
    }

    public class Imaging
    {
        public static Rectangle ChatBoxRect = new Rectangle(6, 620, 385, 176);
        public static Rectangle MiniMapRect = new Rectangle(0, 0, 350, 200);
        public static Rectangle MapNameRect = new Rectangle(45, 20, 40, 30);

        public enum ImageFiles
        {
            badguy1,
            map1,
            BlueEspressoMachineLeft,
            BlueEspressoMachineRight,
            BlueRaspberryJellyJuiceLeft,
            BlueRaspberryJellyJuiceRight,
            ChangeChannel,
            ClaimReward,
            ContentGuide,
            OtherPlayerMiniMap,
            OverheadQuest,
            PlayerMiniMap,
            RebootServer,
            RebootServerChannelSelect,
            CharacterSelectionCharacterSlot,
            Rune,
            RuneMiniMap,
            HP,
            TheNextLegendTitle,
            LeftHealthBar,
            HealthBarEmpty,
            runetest5,
            runetest4,
            runetest3,
            runetest2,
            runetest1,
            DemonAvengerName
        }

        //If you get 'dllimport unknown'-, then add 'using System.Runtime.InteropServices;'
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public static System.Windows.Media.ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        public static Bitmap CropImage(Bitmap img, Rectangle cropArea)
        {
            return img.Clone(cropArea, img.PixelFormat);
        }

        public static Bitmap GetImageFromFile(ImageFiles fileName)
        {
            string path = $"..\\..\\Images\\{fileName.ToString()}.png";
            path = Path.Combine(Environment.CurrentDirectory, path);
            return new Bitmap(path);
        }

        private enum ClosestSide
        {
            Unassigned = 0,
            Bottom = 1,
            Top = 2,
            Left = 3,
            Right = 4
        }

        private static ClosestSide GetClosestSide(Vector2 clusterCenter, int imageWidth, int imageHeight)
        {
            Dictionary<ClosestSide, Vector2> sideValues = new Dictionary<ClosestSide, Vector2>()
            {
                { ClosestSide.Bottom, new Vector2(imageWidth / 2, imageHeight) },
                { ClosestSide.Top, new Vector2(imageWidth / 2, 0) },
                { ClosestSide.Left, new Vector2(0, imageHeight / 2) },
                { ClosestSide.Right, new Vector2(imageWidth, imageHeight / 2) }
            };
            double closestValue = double.MaxValue;
            ClosestSide curClosestSide = ClosestSide.Unassigned;
            double curDistance = 0;
            foreach (var curSideValue in sideValues)
            {
                curDistance = MapleMath.PixelCoordinateDistance(curSideValue.Value, clusterCenter);
                if (curDistance < closestValue)
                {
                    closestValue = curDistance;
                    curClosestSide = curSideValue.Key;
                }
            }
            return curClosestSide;
        }

        public static bool SolveRune(Bitmap fullGameImage)
        {
            List<Color> RuneColors = new List<Color>()
            {
                Color.FromArgb(19, 248, 41),  // dark green, at base
                //Color.FromArgb(170, 255, 0),  // lime greenish, in middle
                Color.FromArgb(221, 102, 0)   // orangish, at tip of point
            };
            // this is a lil hacky because it was meant for mobs but uh
            Dictionary<Color, MobCluster> pixelClusters = new Dictionary<Color, MobCluster>();
            foreach (var curRuneColor in RuneColors)
            {
                pixelClusters.Add(curRuneColor, new MobCluster());
            }
            int runeArrowsFound = 0;
            int maxRuneArrows = 4;
            int minRuneColorsHit = 5;
            for (int curXPixel = 638; curXPixel <= 1190; curXPixel += 92)
            {
                Bitmap croppedImage = CropImage(fullGameImage, new Rectangle(curXPixel, 180, 92, 50));
                List<Color> curColorData = GetColorsFromBmp(croppedImage);
                int croppedImageWidth = croppedImage.Width;
                int croppedImageHeight = croppedImage.Height;
                /*Parallel.ForEach(Enumerable.Range(0, croppedImageWidth * croppedImageHeight), curColorIndex => 
                { 
                    foreach (var curRuneColor in RuneColors)
                    {
                        if (WithinTolerance(curColorData[curColorIndex], curRuneColor, 5))
                        {
                            pixelClusters[curRuneColor].AddHit(curColorIndex);
                            break;
                        }
                    }
                });*/
                /*Dictionary<Color, ClosestSide> colorSuggestions = new Dictionary<Color, ClosestSide>();
                foreach (var curColor in RuneColors)
                {
                    colorSuggestions.Add(curColor, ClosestSide.Unassigned);
                }
                foreach (var curRuneColor in RuneColors)
                {
                    if (pixelClusters[curRuneColor].Locations.Count > minRuneColorsHit)
                    {
                        colorSuggestions[curRuneColor] = GetClosestSide(pixelClusters[curRuneColor].Center, croppedImageWidth, croppedImageHeight);
                    }
                }*/
                // check if red and green agree
                
                // if they agree add the input
                if (runeArrowsFound == maxRuneArrows)
                {
                    break;
                }
            }
            if (runeArrowsFound == maxRuneArrows)
            {
                return true;
            }
            return false;
        }

        public static bool GetMobLocation(MapData mapData, out int numMobs, Vector2 playerMinimapLocation, out Vector2 bestMinimapLocation)
        {
            List<MobNames> currentMobNames = MapData.MapNamesToMobNames[mapData.MapName];
            List<Imaging.ImageFiles> currentMobImageFileNames = new List<Imaging.ImageFiles>();
            foreach (var curMobName in currentMobNames)
            {
                currentMobImageFileNames.AddRange(MobData.MobNamesToImagingFiles[curMobName]);
            }
            List<Bitmap> currentMobImages = new List<Bitmap>();
            foreach (var curMobImageFileName in currentMobImageFileNames)
            {
                currentMobImages.Add(Imaging.GetImageFromFile(curMobImageFileName));
            }
            Bitmap playerTitleImage = Imaging.GetImageFromFile(ImageFiles.TheNextLegendTitle); // todo
            double titleToPlayerOffsetX = 70; 
            double titleToPlayerOffsetY = -50;
            Bitmap curScreen;
            List<Vector2> mobLocations;
            while (!Imaging.GetCurrentGameScreen(out curScreen))
            {
                Thread.Sleep(10);
            }
            // todo uhh this could pose a problem actually if multiple players
            if (playerMinimapLocation != null && Imaging.FindBitmap(new List<Bitmap>() { playerTitleImage }, curScreen, out List<Vector2> titleLocations, ImageFindTypes.Traditional) && Imaging.FindBitmap(currentMobImages, curScreen, out mobLocations, ImageFindTypes.SimilarImage) && titleLocations.Count == 1)
            {
                List<MobCluster> mobClusterData = MobCluster.FindMobClustersFromPixelData(mobLocations, curScreen.Width, curScreen.Height);
                MobCluster curMobCluster = mobClusterData.OrderByDescending(x => x.Locations.Count()).Take(1).First();
                var playerScreenLocation = titleLocations[0];
                bestMinimapLocation = MapleMath.MapCoordinatesToMiniMapCoordinates(curMobCluster.Center, new Vector2(playerScreenLocation.X + titleToPlayerOffsetX, playerScreenLocation.Y + titleToPlayerOffsetY), playerMinimapLocation);
                numMobs = mobLocations.Count;
                return true;
            }
            if (playerMinimapLocation != null)
            {

            }
            bestMinimapLocation = null;
            numMobs = 0;
            return false;
        }

        public static List<Color> GetColorsFromBmp(Bitmap bmp)
        {
            List<Color> colorData = new List<Color>(new Color[bmp.Width * bmp.Height]);
            BitmapData bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);

            // not a complete check, but a start on how to use different pixelformats
            int pixWidth = bmp.PixelFormat == PixelFormat.Format24bppRgb ? 3 :
                           bmp.PixelFormat == PixelFormat.Format32bppArgb ? 4 : 4;
            IntPtr scan0 = bmData.Scan0;
            int stride = bmData.Stride;
            int bmpWidth = bmp.Width;
            int bmpHeight = bmp.Height;
            Vector2 PixelToPixelCoordinate(int pixelLocation, int imageWidth)
            {
                return new Vector2(pixelLocation % imageWidth, pixelLocation / imageWidth);
            }
            Parallel.ForEach(Enumerable.Range(0, bmpWidth * bmpHeight), curIndex =>
            {
                int px;
                byte alpha;
                Color color;
                Vector2 pixelCoordinates = PixelToPixelCoordinate(curIndex, bmpWidth);
                unsafe
                {
                    byte* p = (byte*)scan0.ToPointer() + (int)pixelCoordinates.Y * stride;
                    px = (int)pixelCoordinates.X * pixWidth;
                    alpha = (byte)(pixWidth == 4 ? p[px + 3] : 255);
                    color = Color.FromArgb(alpha, p[px + 2], p[px + 1], p[px + 0]);
                }
                colorData[curIndex] = color;
            });
            bmp.UnlockBits(bmData);
            return colorData;
        }

        public static bool GetCurrentGameScreen(out Bitmap gameScreen)
        {
            Rectangle bounds = Screen.GetBounds(System.Drawing.Point.Empty);
            gameScreen = null;
            try
            {
                gameScreen = new Bitmap(bounds.Width, bounds.Height);
                Graphics gr = Graphics.FromImage(gameScreen);
                gr.CopyFromScreen(System.Drawing.Point.Empty, System.Drawing.Point.Empty, bounds.Size);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public class ImageHit
        {
            public int SubImageIdentifyingIndex;
            public List<Vector2> SubImageLocation;
            public ImageHit(int subImageIdentifyingIndex, List<Vector2> subImageLocation)
            {
                SubImageIdentifyingIndex = subImageIdentifyingIndex;
                SubImageLocation = subImageLocation;
            }
        }

        public class SubImage
        {
            public int SubImageIdentifyingIndex;
            public List<Color> Pixels;
            public SubImage(List<Color> pixels, int subImageIdentifyingIndex)
            {
                SubImageIdentifyingIndex = subImageIdentifyingIndex;
                Pixels = pixels;
            }
        }

        public class ImageProcessingPixelData
        {
            public static ConcurrentDictionary<Color, ImageProcessingPixelData> PreProcessedColors;
            public Color CurrentColor;
            public List<ImageHit> ImageHitDataList;


            public ImageProcessingPixelData(Color currentColor, List<ImageHit> imageHitDataList)
            {
                CurrentColor = currentColor;
                ImageHitDataList = imageHitDataList;
            }
        }

        /*public static void FindMatch(Mat modelImage, Mat observedImage, out long matchTime, out VectorOfKeyPoint modelKeyPoints, out VectorOfKeyPoint observedKeyPoints, VectorOfVectorOfDMatch matches, out Mat mask, out Mat homography)
        {
            int k = 2;
            double uniquenessThreshold = 0.8;
            double hessianThresh = 300;

            Stopwatch watch;
            homography = null;

            modelKeyPoints = new VectorOfKeyPoint();
            observedKeyPoints = new VectorOfKeyPoint();
                using (UMat uModelImage = modelImage.ToUMat(AccessType.Read))
                using (UMat uObservedImage = observedImage.ToUMat(AccessType.Read))
                {
                    SURF surfCPU = new SURF(hessianThresh);
                    //extract features from the object image
                    UMat modelDescriptors = new UMat();
                    surfCPU.DetectAndCompute(uModelImage, null, modelKeyPoints, modelDescriptors, false);

                    watch = Stopwatch.StartNew();

                    // extract features from the observed image
                    UMat observedDescriptors = new UMat();
                    surfCPU.DetectAndCompute(uObservedImage, null, observedKeyPoints, observedDescriptors, false);
                    BFMatcher matcher = new BFMatcher(DistanceType.L2);
                    matcher.Add(modelDescriptors);

                    matcher.KnnMatch(observedDescriptors, matches, k, null);
                    mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
                    mask.SetTo(new MCvScalar(255));
                    Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);

                    int nonZeroCount = CvInvoke.CountNonZero(mask);
                    if (nonZeroCount >= 4)
                    {
                        nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints,
                           matches, mask, 1.5, 20);
                        if (nonZeroCount >= 4)
                            homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints,
                               observedKeyPoints, matches, mask, 2);
                    }

                    watch.Stop();
                }
            matchTime = watch.ElapsedMilliseconds;
        }

        public static bool ProcessImage(Bitmap imgSrc, Bitmap imgSub, double threshold = 400)
        {
            Image<Gray, Byte> modelImage = new Image<Gray, byte>("HatersGonnaHate.png");
            Image<Gray, Byte> observedImage = new Image<Gray, byte>("box_in_scene.png");
            Stopwatch watch;
            Emgu.CV.Features2D.s
            HomographyMatrix homography = null;
            SURFDetector surfCPU = new SURFDetector(500, false);

            VectorOfKeyPoint modelKeyPoints;
            VectorOfKeyPoint observedKeyPoints;
            Matrix<int> indices;
            Matrix<float> dist;
            Matrix<byte> mask;
                //extract features from the object image
                modelKeyPoints = surfCPU.DetectKeyPointsRaw(modelImage, null);
                //MKeyPoint[] kpts = modelKeyPoints.ToArray();
                Matrix<float> modelDescriptors = surfCPU.ComputeDescriptorsRaw(modelImage, null, modelKeyPoints);

                watch = Stopwatch.StartNew();

                // extract features from the observed image
                observedKeyPoints = surfCPU.DetectKeyPointsRaw(observedImage, null);
                Matrix<float> observedDescriptors = surfCPU.ComputeDescriptorsRaw(observedImage, null, observedKeyPoints);

                BruteForceMatcher matcher = new BruteForceMatcher(BruteForceMatcher.DistanceType.L2F32);
                matcher.Add(modelDescriptors);
                int k = 2;
                indices = new Matrix<int>(observedDescriptors.Rows, k);
                dist = new Matrix<float>(observedDescriptors.Rows, k);
                matcher.KnnMatch(observedDescriptors, indices, dist, k, null);

                mask = new Matrix<byte>(dist.Rows, 1);

                mask.SetValue(255);

                Features2DTracker.VoteForUniqueness(dist, 0.8, mask);

                int nonZeroCount = CvInvoke.cvCountNonZero(mask);
                if (nonZeroCount >= 4)
                {
                    nonZeroCount = Features2DTracker.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints, indices, mask, 1.5, 20);
                    if (nonZeroCount >= 4)
                        homography = Features2DTracker.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints, observedKeyPoints, indices, mask, 3);
                }

                watch.Stop();

            //Draw the matched keypoints
            Image<Bgr, Byte> result = Features2DTracker.DrawMatches(modelImage, modelKeyPoints, observedImage, observedKeyPoints,
                indices, new Bgr(255, 255, 255), new Bgr(255, 255, 255), mask, Features2DTracker.KeypointDrawType.NOT_DRAW_SINGLE_POINTS);

            #region draw the projected region on the image
            if (homography != null)
            {  //draw a rectangle along the projected model
                Rectangle rect = modelImage.ROI;
                PointF[] pts = new PointF[] {
               new PointF(rect.Left, rect.Bottom),
               new PointF(rect.Right, rect.Bottom),
               new PointF(rect.Right, rect.Top),
               new PointF(rect.Left, rect.Top)};
                homography.ProjectPoints(pts);

                result.DrawPolyline(Array.ConvertAll<PointF, Point>(pts, Point.Round), true, new Bgr(Color.Red), 5);
            }
            #endregion

            ImageViewer.Show(result, String.Format("Matched using {0} in {1} milliseconds", GpuInvoke.HasCuda ? "GPU" : "CPU", watch.ElapsedMilliseconds));
        }


    }

    List<ImageProcessingPixelData> bigImageColors = GetColorsFromBmp(bigBmp).Select(x => { return new ImageProcessingPixelData(x, new List<ImageHit>()); }).ToList();
    List<SubImage> subImageDataList = new List<SubImage>();
    for (int i = 0; i < smallBmps.Count; i++)
    {
        subImageDataList.Add(new SubImage(GetColorsFromBmp(smallBmps[i]), i));
    }
    var uniqueBigColors = bigImageColors.GroupBy(x => x.CurrentColor).First();
    Parallel.ForEach(uniqueBigColors, currentBigImageColor => wwwww
    {
        foreach (var curSubImage in subImageDataList)
        {
            foreach (var curSubImageColor in curSubImage.Pixels)
            {
                if (WithinTolerance(currentBigImageColor.CurrentColor, curSubImageColor, tol))
                {

                }
            }
        }
    });
}*/

        public static bool FindBitmap(List<Bitmap> subImageDataList, Bitmap sourceImage, out List<Vector2> locations, ImageFindTypes imageFindType = ImageFindTypes.ExactImage)
        {
            if (!ImageFinder.FindImage(imageFindType, sourceImage, subImageDataList, out locations))
            {
                return false;
            }
            // correct height, basically reverse the Y
            locations = locations.Select(x => { return new Vector2(x.X, Math.Abs(x.Y - sourceImage.Height)); }).ToList();
            return true;
        }

        /// <summary>
        /// Return a list of strings for each line in the text
        /// </summary>
        /// <param name="imageData"></param>
        /// <returns></returns>
        public static List<string> ReadTextFromImage(Bitmap imageData)
        {
            double confidence = 0;
            OcrResult results = null;
            while (confidence < 90)
            {
                results = new IronTesseract().Read(imageData);
                confidence = results.Confidence;
            }
            
            // Flush all graphics changes to the bitmap
            //g.Flush();
            return results.Lines.Select(x => x.Text).ToList();
        }

        
    }
}
