using Maple.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Maple.ViewModels
{
    public class MapCreatorViewModel : ViewModelBase
    {
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
            IsRecordPlatformEdgeEnabled = !IsRecordPlatformEdgeEnabled;
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
