using Maple.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        public Bitmap CurrentImage { get; set; }

        MapData _mapInfoData;
        MapData MapInfoData
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

        public void ExportMap()
        {
            MapInfoData.MapName = MapNameText;
            var allImageText = Imaging.ReadTextFromImage(CurrentImage);
            string fileName = string.Join("_", allImageText);
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
            YFindingImageWhereOriginalMinimapLocationChanged,
        }

        public void RecordPixelToMapPixel()
        {
            Input.StartInput('d');
            PhotoTaker.StartTakingImages(10);
            Thread.Sleep(1000);
            PhotoTaker.StopTakingImages();
            Input.StopInput('d');
            List<PhotoWithTimestamp> photoWithTimeDataList = PhotoTaker.GetPhotosAndDeleteFromMemory();
            var playerImage = Imaging.GetImageFromFile(Imaging.ImageFiles.PlayerMiniMap);
            var playerNameImage = Imaging.GetImageFromFile(Imaging.ImageFiles.DemonAvengerName);
            RecordPixelToMapPixelStatuses currentStatus = RecordPixelToMapPixelStatuses.XFindingInitialPhotoLocation;
            List<int> minimapLocations;
            List<int> nameLocations;
            Vector2 minimapLocationPre = null;
            Vector2 minimapLocationOriginal = null;
            Vector2 minimapLocationChanged = null;
            Vector2 playerLocationOriginal;
            Vector2 playerLocationChanged;
            
            foreach (var curPhotoWithTimestamp in photoWithTimeDataList)
            {
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
                            if (minimapLocationOriginal.X == minimapLocationPre.X)
                            {
                                continue;
                            }
                            if (Imaging.FindBitmap(new List<Bitmap>() { playerNameImage }, curPhotoWithTimestamp.Photo, 50, out nameLocations))
                            {
                                playerLocationOriginal = MapleMath.PixelToPixelCoordinate(nameLocations[0], curPhotoWithTimestamp.Photo.Width);
                                currentStatus = RecordPixelToMapPixelStatuses.XFindingSecondImageWhereOriginalMinimapLocationChanged;
                            }
                            break;
                        case RecordPixelToMapPixelStatuses.XFindingSecondImageWhereOriginalMinimapLocationChanged:
                            minimapLocationChanged = MapleMath.PixelToPixelCoordinate(minimapLocations[0], curPhotoWithTimestamp.Photo.Width);
                            if (minimapLocationChanged.X == minimapLocationOriginal.X)
                            {
                                continue;
                            }
                            if (Imaging.FindBitmap(new List<Bitmap>() { playerNameImage }, curPhotoWithTimestamp.Photo, 50, out nameLocations))
                            {
                                playerLocationChanged = MapleMath.PixelToPixelCoordinate(nameLocations[0], curPhotoWithTimestamp.Photo.Width);
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
            
        }


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
            RecordPlatformEdgeText = "Record Platform Edge";
            RecordRopeText = "Record Rope Edge";
            MapInfoData = new MapData();
            IsExportMapEnabled = true;
            IsRecordPlatformEdgeEnabled = true;
            IsRecordRopeEnabled = true;
        }

    }
}
