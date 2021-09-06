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
            _photoTakerThread = new Thread(PhotoTakerWorker);
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
            LeftHealthBar,
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

        private static bool WithinTolerance(Color colorA, Color colorB, int tol)
        {

            if (Math.Abs(colorA.R - colorB.R) < tol
                && Math.Abs(colorA.B - colorB.B) < tol
                && Math.Abs(colorA.R - colorB.R) < tol)
            {
                return true;
            }
            return false;
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
                Parallel.ForEach(Enumerable.Range(0, croppedImageWidth * croppedImageHeight), curColorIndex => 
                { 
                    foreach (var curRuneColor in RuneColors)
                    {
                        if (WithinTolerance(curColorData[curColorIndex], curRuneColor, 5))
                        {
                            pixelClusters[curRuneColor].AddHit(MapleMath.PixelToPixelCoordinate(curColorIndex, croppedImageWidth));
                            break;
                        }
                    }
                });
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

        public static bool GetMobLocation(MapData mapData, out int numMobs, out Vector2 bestMinimapLocation)
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
            Bitmap curScreen;
            List<int> mobLocations;
            while (!Imaging.GetCurrentGameScreen(out curScreen))
            {
                Thread.Sleep(10);
            }
            if (Imaging.FindBitmap(currentMobImages, curScreen, 20, out mobLocations))
            {
                List<MobCluster> mobClusterData = MobCluster.FindMobClustersFromPixelData(mobLocations, curScreen.Width, curScreen.Height);
                MobCluster curMobCluster = mobClusterData.OrderByDescending(x => x.Locations.Count()).Take(1).First();
                bestMinimapLocation = MapleMath.MapCoordinatesToMiniMapCoordinates(curMobCluster.Center);
                numMobs = mobLocations.Count;
                return true;
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
            Parallel.ForEach(Enumerable.Range(0, bmpWidth * bmpHeight), curIndex =>
            {
                int px;
                byte alpha;
                Color color;
                Vector2 pixelCoordinates = MapleMath.PixelToPixelCoordinate(curIndex, bmpWidth);
                unsafe
                {
                    byte* p = (byte*)scan0.ToPointer() + pixelCoordinates.Y * stride;
                    px = pixelCoordinates.X * pixWidth;
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

        public static bool FindBitmap(List<Bitmap> smallBmps, Bitmap bigBmp, int tol, out List<int> locations)
        {
            IEnumerable<int> potentialSelections
                = Enumerable.Range(0, bigBmp.Width * bigBmp.Height);
            //List<Color> bigBitmapColors = new List<Color>();
            List<List<Color>> smallBitmapColors = new List<List<Color>>();
            //BitmapData bmpData =  bigBmp.LockBits(new Rectangle(0, 0, bigBmp.Width, bigBmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var time1 = DateTime.Now;
            List<Color> bigBitmapColors = GetColorsFromBmp(bigBmp);
            var time2 = DateTime.Now;
            /*foreach (var cur in Enumerable.Range(0, bigBmp.Width * bigBmp.Height))
            {
                bigBitmapColors.Add(bigBmp.GetPixel(cur % bigBmp.Width, cur / bigBmp.Width));
            }*/
            var time3 = DateTime.Now;
            var timeToComplete1 = time2 - time1;
            var timeToComplete2 = time3 - time2;
            List<int> smallBmpWidths = new List<int>();
            List<int> smallBmpHeights = new List<int>();
            List<int> curSmallPixels = new List<int>();
            foreach (var curSmallBmp in smallBmps)
            {
                /*smallBitmapColors.Add(new List<Color>());
                foreach (var cur in Enumerable.Range(0, curSmallBmp.Width * curSmallBmp.Height))
                {
                    
                    smallBitmapColors[smallBitmapColors.Count - 1].Add(curSmallBmp.GetPixel(cur % curSmallBmp.Width, cur / curSmallBmp.Width));
                }*/
                smallBitmapColors.Add(GetColorsFromBmp(curSmallBmp));
                smallBmpWidths.Add(curSmallBmp.Width);
                smallBmpHeights.Add(curSmallBmp.Height);
                curSmallPixels.Add(0);
            }

            int bigBmpWidth = bigBmp.Width;
            ConcurrentBag<int> curPotentialSelections;

            locations = new List<int>();
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
                    locations = returnLocations;
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
