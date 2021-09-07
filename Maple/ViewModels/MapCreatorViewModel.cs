using Maple.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Maple.ViewModels
{
    public class MapCreatorViewModel : ViewModelBase
    {
        public SerializableBitmapImageWrapper CurrentImage { get; set; }

        MapData _mapInfoData;
        public MapData MapInfoData
        {
            get { return _mapInfoData; }
            set { _mapInfoData = value; }
        }

        MapPiece _mapPieceData;

        public MapPiece MapPieceData
        {
            get { return _mapPieceData; }
            set { _mapPieceData = value; }
        }

        private Vector2 _currentPosition;

        public Vector2 CurrentPosition
        {
            get { return _currentPosition; }
            set { _currentPosition = value; }
        }

        private string _mapNameText;

        public string MapNameText
        {
            get { return _mapNameText; }
            set { _mapNameText = value; NotifyPropertyChanged(); }
        }

        private string _recordRopeText;

        public string RecordRopeText
        {
            get { return _recordRopeText; }
            set { _recordRopeText = value; NotifyPropertyChanged(); }
        }

        private bool _isRecordRopeEnabled;

        public bool IsRecordRopeEnabled
        {
            get { return _isRecordRopeEnabled; }
            set { _isRecordRopeEnabled = value; NotifyPropertyChanged(); }
        }

        /*private double _minimapToPixelRatioX;

        public double MinimapToPixelRatioX
        {
            get { return _minimapToPixelRatioX; }
            set { _minimapToPixelRatioX = value; NotifyPropertyChanged(); }
        }

        private double _minimapToPixelRatioY;

        public double MinimapToPixelRatioY
        {
            get { return _minimapToPixelRatioY; }
            set { _minimapToPixelRatioY = value; NotifyPropertyChanged(); }
        }*/

        private ICommand _recordRopeCommand;
        public ICommand RecordRopeCommand
        {
            get
            {
                return _recordRopeCommand ?? (_recordRopeCommand = new CommandHandler(() => RecordRope(), () => true));
            }
        }

        public void RecordRope()
        {
            if (MapPieceData == null)
            {
                MapPieceData = new MapPiece(MapPieceTypes.Rope);
                MapPieceData.SetBeginningOrEnd(CurrentPosition);
            }
            else
            {
                MapPieceData.SetBeginningOrEnd(CurrentPosition);
                MapInfoData.MapPieceDataList.Add(MapPieceData);
                MapPieceData = null;
            }
            IsExportMapEnabled = !IsExportMapEnabled;
            IsRecordPlatformEdgeEnabled = !IsRecordPlatformEdgeEnabled;
        }


        private string _recordPlatformEdgeText;

        public string RecordPlatformEdgeText
        {
            get { return _recordPlatformEdgeText; }
            set { _recordPlatformEdgeText = value; NotifyPropertyChanged(); }
        }

        private ICommand _recordPlatformEdgeCommand;
        public ICommand RecordPlatformEdgeCommand
        {
            get
            {
                return _recordPlatformEdgeCommand ?? (_recordPlatformEdgeCommand = new CommandHandler(() => RecordPlatformEdge(), () => true));
            }
        }

        public void RecordPlatformEdge()
        {
            if (MapPieceData == null)
            {
                MapPieceData = new MapPiece(MapPieceTypes.Ledge);
                MapPieceData.SetBeginningOrEnd(CurrentPosition);
            }
            else
            {
                MapPieceData.SetBeginningOrEnd(CurrentPosition);
                MapInfoData.MapPieceDataList.Add(MapPieceData);
                MapPieceData = null;
            }
            IsRecordRopeEnabled = !IsRecordRopeEnabled;
            IsExportMapEnabled = !IsExportMapEnabled;
        }

        private ICommand _exportMapCommand;
        public ICommand ExportMapCommand
        {
            get
            {
                return _exportMapCommand ?? (_exportMapCommand = new CommandHandler(() => ExportMap(), () => true));
            }
        }

        public ObservableCollection<string> MapNamesDataList
        {
            get { return new ObservableCollection<string>(Enum.GetNames(typeof(MapNames)).ToList()); }
        }

        public void ExportMap()
        {
            MapInfoData.MapName = (MapNames)Enum.Parse(typeof(MapNames), MapNameText);
            MapInfoData.MapNameImage = new SerializableBitmapImageWrapper(Imaging.CropImage(CurrentImage.BitmapData, Imaging.MapNameRect));
            string fileName = MapInfoData.MapName.ToString();
            if (fileName.Length <= 0)
            {
                return;
            }
            string jsonString = JsonConvert.SerializeObject(MapInfoData, Formatting.Indented);
            string mapExportLocation = System.Configuration.ConfigurationManager.AppSettings["MapExportLocation"];
            string filePath = Path.Combine(mapExportLocation, fileName);
            File.WriteAllText(filePath, jsonString);
            MapInfoData = new MapData();
        }

        private ICommand _recordPixelToMapPixelCommand;
        public ICommand RecordPixelToMapPixelCommand
        {
            get
            {
                return _recordPixelToMapPixelCommand ?? (_recordPixelToMapPixelCommand = new CommandHandler(() => RecordPixelToMapPixel(), () => true)); 
            }
        }

        public enum RecordPixelToMapPixelStatuses
        {
            XFindingInitialPhotoLocation,
            XFindingFirstImageWhereOriginalMinimapLocationChanged,
            XFindingSecondImageWhereOriginalMinimapLocationChanged,
            YFindingInitialPhotoLocation,
            YFindingFirstImageWhereOriginalMinimapLocationChanged,
            YFindingSecondImageWhereOriginalMinimapLocationChanged,
            Finished
        }

        public void RecordPixelToMapPixel() { }

        /*public void RecordPixelToMapPixel()
        {
            Input.ResetMouse();
            Input.ClickMouse();
            Thread.Sleep(50);
            Input.ReleaseMouse();
            List<PhotoWithTimestamp> photoWithTimeDataList = new List<PhotoWithTimestamp>();
            //PhotoTaker.StartTakingImages(10);
            for (int i = 0; i < 200; i++)
            {
                Input.StartInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                Thread.Sleep(10);
                Input.StopInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                Thread.Sleep(600);
                if (Imaging.GetCurrentGameScreen(out Bitmap gameScreen))
                {
                    photoWithTimeDataList.Add(new PhotoWithTimestamp(gameScreen));
                }
            }
            
            var playerImage = Imaging.GetImageFromFile(Imaging.ImageFiles.PlayerMiniMap);
            var playerNameImage = Imaging.GetImageFromFile(Imaging.ImageFiles.DemonAvengerName);
            RecordPixelToMapPixelStatuses currentStatus = RecordPixelToMapPixelStatuses.XFindingInitialPhotoLocation;
            List<int> minimapLocations;
            List<int> nameLocations;
            Vector2 minimapLocationPre = null;
            Vector2 minimapLocationOriginal = null;
            Vector2 minimapLocationChanged = null;
            Vector2 playerLocationOriginal = null;
            Vector2 playerLocationChanged = null;
            
            //foreach (var curPhotoWithTimestamp in photoWithTimeDataList)
            while (currentStatus != RecordPixelToMapPixelStatuses.YFindingInitialPhotoLocation)
            {
                Input.StartInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                Thread.Sleep(10);
                Input.StopInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                Thread.Sleep(600);
                PhotoWithTimestamp curPhotoWithTimestamp = null;
                if (Imaging.GetCurrentGameScreen(out Bitmap gameScreen))
                {
                    curPhotoWithTimestamp = new PhotoWithTimestamp(gameScreen);
                }
                else
                {
                    continue;
                }
                if (Imaging.FindBitmap(new List<Bitmap>() { playerImage }, curPhotoWithTimestamp.Photo, 20, out minimapLocations))
                {
                    switch (currentStatus)
                    {
                        case RecordPixelToMapPixelStatuses.XFindingInitialPhotoLocation:
                            minimapLocationPre = MapleMath.PixelToPixelCoordinate(minimapLocations[0], curPhotoWithTimestamp.Photo.Width);
                            currentStatus = RecordPixelToMapPixelStatuses.XFindingFirstImageWhereOriginalMinimapLocationChanged;
                            break;
                        case RecordPixelToMapPixelStatuses.XFindingFirstImageWhereOriginalMinimapLocationChanged:
                            minimapLocationOriginal = MapleMath.PixelToPixelCoordinate(minimapLocations[0], curPhotoWithTimestamp.Photo.Width);
                            if (minimapLocationOriginal.X == minimapLocationPre.X || minimapLocationOriginal.Y == minimapLocationPre.Y)
                            {
                                continue;
                            }
                            if (Imaging.FindBitmap(new List<Bitmap>() { playerNameImage }, curPhotoWithTimestamp.Photo, 125, out nameLocations))
                            {
                                playerLocationOriginal = MapleMath.PixelToPixelCoordinate(nameLocations[0], curPhotoWithTimestamp.Photo.Width);
                                currentStatus = RecordPixelToMapPixelStatuses.XFindingSecondImageWhereOriginalMinimapLocationChanged;
                            }
                            break;
                        case RecordPixelToMapPixelStatuses.XFindingSecondImageWhereOriginalMinimapLocationChanged:
                            minimapLocationChanged = MapleMath.PixelToPixelCoordinate(minimapLocations[0], curPhotoWithTimestamp.Photo.Width);
                            if (minimapLocationChanged.X == minimapLocationOriginal.X || minimapLocationChanged.Y == minimapLocationOriginal.Y)
                            {
                                continue;
                            }
                            if (Imaging.FindBitmap(new List<Bitmap>() { playerNameImage }, curPhotoWithTimestamp.Photo, 125, out nameLocations))
                            {
                                playerLocationChanged = MapleMath.PixelToPixelCoordinate(nameLocations[0], curPhotoWithTimestamp.Photo.Width);
                                if (playerLocationChanged.X == playerLocationOriginal.X || playerLocationChanged.Y == playerLocationOriginal.Y)
                                {
                                    continue;
                                }
                                currentStatus = RecordPixelToMapPixelStatuses.YFindingInitialPhotoLocation;
                            }
                            break;
                        default:
                            break;
                    }
                }
                if (currentStatus == RecordPixelToMapPixelStatuses.YFindingInitialPhotoLocation)
                {
                    break;
                }
            }
            MinimapToPixelRatioX = ((double)(minimapLocationChanged.X - minimapLocationOriginal.X)) / (playerLocationChanged.X - playerLocationOriginal.X);
            MinimapToPixelRatioY = ((double)(minimapLocationChanged.Y - minimapLocationOriginal.Y)) / (playerLocationChanged.Y - playerLocationOriginal.Y);
            PhotoTaker.StartTakingImages(10);
            Input.StartInput(Input.SpecialCharacters.KEY_LEFT_ALT);
            Thread.Sleep(1000);
            PhotoTaker.StopTakingImages();
            Input.StopInput(Input.SpecialCharacters.KEY_LEFT_ALT);
            Input.StopInput('d');
            // todo clean this up
            photoWithTimeDataList = PhotoTaker.GetPhotosAndDeleteFromMemory();
            foreach (var curPhotoWithTimestamp in photoWithTimeDataList)
            {
                if (Imaging.FindBitmap(new List<Bitmap>() { playerImage }, curPhotoWithTimestamp.Photo, 20, out minimapLocations))
                {
                    switch (currentStatus)
                    {
                        case RecordPixelToMapPixelStatuses.YFindingInitialPhotoLocation:
                            minimapLocationPre = MapleMath.PixelToPixelCoordinate(minimapLocations[0], curPhotoWithTimestamp.Photo.Width);
                            currentStatus = RecordPixelToMapPixelStatuses.XFindingFirstImageWhereOriginalMinimapLocationChanged;
                            break;
                        case RecordPixelToMapPixelStatuses.YFindingFirstImageWhereOriginalMinimapLocationChanged:
                            minimapLocationOriginal = MapleMath.PixelToPixelCoordinate(minimapLocations[0], curPhotoWithTimestamp.Photo.Width);
                            if (minimapLocationOriginal.Y == minimapLocationPre.Y)
                            {
                                continue;
                            }
                            if (Imaging.FindBitmap(new List<Bitmap>() { playerNameImage }, curPhotoWithTimestamp.Photo, 125, out nameLocations))
                            {
                                playerLocationOriginal = MapleMath.PixelToPixelCoordinate(nameLocations[0], curPhotoWithTimestamp.Photo.Width);
                                currentStatus = RecordPixelToMapPixelStatuses.XFindingSecondImageWhereOriginalMinimapLocationChanged;
                            }
                            break;
                        case RecordPixelToMapPixelStatuses.YFindingSecondImageWhereOriginalMinimapLocationChanged:
                            minimapLocationChanged = MapleMath.PixelToPixelCoordinate(minimapLocations[0], curPhotoWithTimestamp.Photo.Width);
                            if (minimapLocationChanged.Y == minimapLocationOriginal.Y)
                            {
                                continue;
                            }
                            if (Imaging.FindBitmap(new List<Bitmap>() { playerNameImage }, curPhotoWithTimestamp.Photo, 125, out nameLocations))
                            {
                                playerLocationChanged = MapleMath.PixelToPixelCoordinate(nameLocations[0], curPhotoWithTimestamp.Photo.Width);
                                currentStatus = RecordPixelToMapPixelStatuses.Finished;
                            }
                            break;
                        default:
                            break;
                    }
                }
                if (currentStatus == RecordPixelToMapPixelStatuses.Finished)
                {
                    break;
                }
            }
            MinimapToPixelRatioY = ((double)(minimapLocationChanged.Y - minimapLocationOriginal.Y)) / (playerLocationChanged.Y - playerLocationOriginal.Y);

        }*/


        private bool _isRecordPlatformEdgeEnabled;
        public bool IsRecordPlatformEdgeEnabled
        {
            get { return _isRecordPlatformEdgeEnabled; }
            set { _isRecordPlatformEdgeEnabled = value; NotifyPropertyChanged(); }
        }

        private bool _isExportMapEnabled;

        public bool IsExportMapEnabled
        {
            get { return _isRecordRopeEnabled; }
            set { _isExportMapEnabled = value; NotifyPropertyChanged(); }
        }

        public MapCreatorViewModel()
        {
            base.InitializeData();
            MapNameText = MapNamesDataList[0];
            RecordPlatformEdgeText = "Record Platform Edge";
            RecordRopeText = "Record Rope Edge";
            MapInfoData = new MapData();
            IsExportMapEnabled = true;
            IsRecordPlatformEdgeEnabled = true;
            IsRecordRopeEnabled = true;
        }

    }
}
