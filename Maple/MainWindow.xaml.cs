using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
//using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Threading;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using IronOcr;
using System.IO;
using Maple.Data;
using Maple.ViewModels;
using System.ComponentModel;
using System.Configuration;
using Newtonsoft.Json;

namespace Maple
{



    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        void DataWindow_Closing(object sender, CancelEventArgs e)
        {
            ModelData.IsClosing = true;
        }

        MainWindowViewModel ModelData
        {
            get { return DataContext as MainWindowViewModel; }
        }

        private void CameraUpdate()
        {
            List<Bitmap> enemyImages = new List<Bitmap>() 
            {
                /*Maple.Data.Imaging.GetImageFromFile(Data.Imaging.ImageFiles.BlueEspressoMachineLeft),
                Maple.Data.Imaging.GetImageFromFile(Data.Imaging.ImageFiles.BlueEspressoMachineRight),
                Maple.Data.Imaging.GetImageFromFile(Data.Imaging.ImageFiles.BlueRaspberryJellyJuiceLeft),
                Maple.Data.Imaging.GetImageFromFile(Data.Imaging.ImageFiles.BlueRaspberryJellyJuiceRight)*/
                Maple.Data.Imaging.GetImageFromFile(Data.Imaging.ImageFiles.TheNextLegendTitle)
            };
            //path = Path.Combine(Environment.CurrentDirectory, "..\\..\\Images\\BlueRaspberryJellyJuiceRight.png");
            //Bitmap enemy2 = new Bitmap(path);
            
            var runeData = Maple.Data.Imaging.GetImageFromFile(Data.Imaging.ImageFiles.runetest3);
            // 730, 180, 370, 50

            // Bitmap croppedImage = CropImage(runeData, new Rectangle(730, 180, 92, 50));
            // Bitmap croppedImage = CropImage(runeData, new Rectangle(822, 180, 92, 50));
            // Bitmap croppedImage = CropImage(runeData, new Rectangle(914, 180, 92, 50));
            // Bitmap croppedImage = CropImage(runeData, new Rectangle(1006, 180, 92, 50));
            // Bitmap croppedImage = CropImage(runeData, new Rectangle(1098, 180, 92, 50));
            // Bitmap croppedImage = CropImage(runeData, new Rectangle(1190, 180, 92, 50));

            bool isClosing = false;
            bool isActive = false;
            //Bitmap croppedImage = CropImage(src, new Rectangle(0, 0, 350, 200));
            while (!isClosing)
            {
                try
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        isClosing = ModelData.IsClosing;
                        isActive = ModelData.IsActive;
                    });
                }
                catch (Exception ex)
                {
                    break;
                }
                Thread.Sleep(100);
                if (!isActive)
                {
                    
                    continue;
                }
                if (isClosing)
                {
                    return;
                }
                Bitmap croppedImage;
                /*if (!Data.Imaging.GetCurrentGameScreen(out croppedImage))
                {
                    continue;
                }*/
                croppedImage = RuneTestData;
                //Bitmap croppedImage = Data.Imaging.CropImage(src, new Rectangle(730, 180, 92, 50));
                
                Graphics g = Graphics.FromImage(croppedImage);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                // Create string formatting options (used for alignment)
                StringFormat format = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                var cursorPosition = System.Windows.Forms.Cursor.Position;
                // Draw the text onto the image
                //g.DrawString($"x: {xVal} y: {yVal}\n x: {cursorPosition.X} y: {cursorPosition.Y}", new Font("Arial", 18), Brushes.Red, new RectangleF(0, 0, croppedImage.Width, croppedImage.Height), format);
                /*if (Maple.Data.Imaging.FindBitmap(enemyImages, croppedImage, out List<Vector2> locations, ImageFindTypes.Traditional))
                {
                    //List<MobCluster> mobClusterData = MobCluster.FindMobClustersFromPixelData(locations, croppedImage.Width, croppedImage.Height);
                    //var curMobCluster = mobClusterData.OrderByDescending(x => x.Locations.Count()).Take(1);
                    //g.DrawRectangle(new Pen(Color.Red, 10), new Rectangle(curMobCluster.Center.X - 10, curMobCluster.Center.Y - 10, 4 + 20, 6 + 20));
                    locations = locations.Select(x => { return new Vector2(x.X, Math.Abs(x.Y - croppedImage.Height)); }).ToList();
                    foreach (var curLocation in locations)
                    {
                        //Input.SetMouseLocation(MapleMath.PixelToPixelCoordinate(curLocation, croppedImage.Width));
                        g.DrawRectangle(new Pen(Color.Red, 10), new Rectangle((int)(curLocation.X) + 50, (int)curLocation.Y - 50, 4 + 20, 6 + 20));
                    }
                    
                }*/
                foreach (var curLocation in imageLocations)
                {
                    g.DrawRectangle(new Pen(Color.Red, 10), new Rectangle((int)(curLocation.X) - 10, (int)curLocation.Y - 10, 4 + 20, 6 + 20));
                }
                /*var results = new IronTesseract().Read(croppedImage);*/
                // Flush all graphics changes to the bitmap
                g.Flush();
                try
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        ImageBrushData.ImageSource = Data.Imaging.ImageSourceFromBitmap(croppedImage);
                    });
                }
                catch (Exception ex)
                {
                    continue;
                }


                //bitmap.Save("test.jpg", ImageFormat.Jpeg);
            }
        }

        Thread CameraUpdateThread;
        Bitmap RuneTestData = null;
        List<Vector2> imageLocations = new List<Vector2>();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            CameraUpdateThread = new Thread(CameraUpdate) { Name = "Main Window Camera Update Thread" };
            CameraUpdateThread.Start();
            var runeTestData = Data.Imaging.GetImageFromFile(Data.Imaging.ImageFiles.runetest2);
            //runeTestData = Data.Imaging.CropImage(runeTestData, new Rectangle(100, 0, runeTestData.Width - 200, 400));

            RuneTestData = runeTestData;
            Data.Imaging.SolveRune(runeTestData, out List<Input.SpecialCharacters> chars, out imageLocations);
            Console.WriteLine(string.Join(", ", chars.Select(x => x.ToString())));
            Input.InitializeInputs();
            var mapImage = Data.Imaging.GetImageFromFile(Data.Imaging.ImageFiles.map1);
            var badguyImage = Data.Imaging.GetImageFromFile(Data.Imaging.ImageFiles.badguy1);
            var hpImage = Data.Imaging.GetImageFromFile(Data.Imaging.ImageFiles.ChangeChannel);
            //SURFFeatureExample.Program.MatchPicBySurf(mapImage, new List<Bitmap>() { hpImage, badguyImage });
            //SURFFeatureExample.Program.MatchPicByBF(mapImage, new List<Bitmap>() { badguyImage });
            //Data.Imaging.GetCurrentGameScreen(out Bitmap curGameScreen);
            //var curPoint = SURFFeatureExample.Program.FindPicFromImage(curGameScreen, hpImage);
            //Input.SetMouseLocation(new Vector2(curPoint.X, curPoint.Y));
            //Input.StartInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
            //Input.StopInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);

            //ImageBrushData.ImageSource = ImageSourceFromBitmap(CropImage(src, cropRect));
            /*using (Graphics g = Graphics.FromImage(src))
            {
                g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height), cropRect, GraphicsUnit.Pixel);

            }*/
        }
    }
}
