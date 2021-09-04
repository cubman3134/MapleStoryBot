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
                Maple.Data.Imaging.GetImageFromFile(Data.Imaging.ImageFiles.HP)
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
                this.Dispatcher.Invoke(() =>
                {
                    isClosing = ModelData.IsClosing;
                    isActive = ModelData.IsActive;
                });
                if (!isActive)
                {
                    Thread.Sleep(100);
                    continue;
                }
                if (isClosing)
                {
                    return;
                }
                Bitmap croppedImage;
                if (!Data.Imaging.GetCurrentGameScreen(out croppedImage))
                {
                    continue;
                }
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
                if (Maple.Data.Imaging.FindBitmap(enemyImages, croppedImage, 30, out List<int> locations))
                {
                    List<MobCluster> mobClusterData = MobCluster.FindMobClustersFromPixelData(locations, croppedImage.Width, croppedImage.Height);
                    var curMobCluster = mobClusterData.OrderByDescending(x => x.Locations.Count()).Take(1);
                    //g.DrawRectangle(new Pen(Color.Red, 10), new Rectangle(curMobCluster.Center.X - 10, curMobCluster.Center.Y - 10, 4 + 20, 6 + 20));

                    foreach (var curLocation in locations)
                    {
                        Input.SetMouseLocation(MapleMath.PixelToPixelCoordinate(curLocation, croppedImage.Width));
                        //g.DrawRectangle(new Pen(Color.Red, 10), new Rectangle(curLocation.Center.X - 10, curLocation.Center.Y - 10, 4 + 20, 6 + 20));
                    }
                    
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

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            CameraUpdateThread = new Thread(CameraUpdate);
            CameraUpdateThread.Start();
            Input.InitializeInputs();
            
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
