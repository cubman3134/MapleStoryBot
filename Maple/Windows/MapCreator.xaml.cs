using Maple.Data;
using Maple.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;


namespace Maple.Windows
{


    /// <summary>
    /// Interaction logic for MapCreator.xaml
    /// </summary>
    public partial class MapCreator : Window
    {
        void DataWindow_Closing(object sender, CancelEventArgs e)
        {
            ModelData.IsClosing = true;
        }

        MapCreatorViewModel ModelData { get { return DataContext as MapCreatorViewModel; } }

        private void CameraUpdate()
        {
            List<Bitmap> playerImages = new List<Bitmap>()
            {
                Maple.Data.Imaging.GetImageFromFile(Data.Imaging.ImageFiles.PlayerMiniMap),
            };
            //path = Path.Combine(Environment.CurrentDirectory, "..\\..\\Images\\BlueRaspberryJellyJuiceRight.png");
            //Bitmap enemy2 = new Bitmap(path);
            Rectangle bounds = Screen.GetBounds(System.Drawing.Point.Empty);

            //var runeData = Maple.Data.Imaging.GetImageFromFile(Data.Imaging.ImageFiles.runetest3);
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
                Bitmap src = new Bitmap(bounds.Width, bounds.Height);
                //src = Imaging.ChangeBackgroundToBlack(src, 6);
                //Bitmap croppedImage = src;
                Bitmap croppedImage = Data.Imaging.CropImage(src, Imaging.MiniMapRect);

                //croppedImage = new Bitmap(Imaging.ColorReplace(croppedImage, 200, Color.Black, Color.White));
                //croppedImage = new Bitmap(b.Width, b.Height);
                this.Dispatcher.Invoke(() =>
                {
                    ModelData.CurrentImage = croppedImage;
                });
                
                Graphics gr = Graphics.FromImage(croppedImage);
                gr.CopyFromScreen(System.Drawing.Point.Empty, System.Drawing.Point.Empty, bounds.Size);
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
                if (Maple.Data.Imaging.FindBitmap(playerImages, croppedImage, 10, out List<int> locations))
                {
                    if (locations?.Count > 0)
                    {
                        var curLocation = MapleMath.PixelToPixelCoordinate(locations[0], croppedImage.Width);
                        g.DrawString($"x: {curLocation.X} y: {curLocation.Y}", new Font("Arial", 18), Brushes.Red, new Rectangle(0, 0, croppedImage.Width, croppedImage.Height), format);
                        this.Dispatcher.Invoke(() =>
                        {
                            ModelData.CurrentPosition = curLocation;
                        });
                        //g.DrawRectangle(new Pen(Color.Red, 10), new Rectangle(curMobCluster.Center.X - 10, curMobCluster.Center.Y - 10, 4 + 20, 6 + 20));
                    }
                    

                }
                /*var results = new IronTesseract().Read(croppedImage);*/
                // Flush all graphics changes to the bitmap
                g.Flush();

                this.Dispatcher.Invoke(() =>
                {
                    ImageBrushData.ImageSource = Data.Imaging.ImageSourceFromBitmap(croppedImage);
                });


                //bitmap.Save("test.jpg", ImageFormat.Jpeg);
            }
        }

        Thread CameraUpdateThread;

        public MapCreator()
        {
            InitializeComponent();
            DataContext = new ViewModels.MapCreatorViewModel();
            CameraUpdateThread = new Thread(CameraUpdate);
            CameraUpdateThread.Start();
        }
    }
}
