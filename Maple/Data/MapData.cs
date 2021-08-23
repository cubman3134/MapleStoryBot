using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Data
{
    public enum MapPieceTypes
    {
        Rope = 0,
        Ledge = 1
    }

    public enum MapNames
    {

    }

    public enum LevelRanges
    {
        Level145 = 145, // kerning tower
        Level150 = 150, // kerning tower
        Level155 = 155, // kerning tower
        Level160 = 160, // kerning tower
        Level170 = 170, // kritias
        Level175 = 175, // kritias
        Level180 = 180, // kritias
        Level190 = 190, // twilight perion
        Level195 = 195, // twilight perion
    }

    public class MapPiece
    {
        public MapPieceTypes MapPieceType { get; set; }
        public Vector2 Beginning { get; set; }
        public Vector2 End { get; set; }

        public MapPiece(MapPieceTypes mapPieceType, Vector2 beginning, Vector2 end)
        {
            MapPieceType = mapPieceType;
            Beginning = beginning;
            End = end;
        }

        public MapPiece(MapPieceTypes mapPieceType)
        {
            MapPieceType = mapPieceType;
        }

        public void SetBeginningOrEnd(Vector2 value)
        {
            if (Beginning == null)
            {
                Beginning = value;
            }
            else if (Beginning.Y > value.Y)
            {
                End = Beginning;
                Beginning = value;
            }
            else if (value.Y > Beginning.Y)
            {
                End = value;
            }
            else if (Beginning.X > value.X)
            {
                End = Beginning;
                Beginning = value;
            }
            else
            {
                End = value;
            }
        }
    }

    public class MapData
    {
        public string MapName;
        public List<MapPiece> MapPieceDataList;

        public MapData()
        {
            MapPieceDataList = new List<MapPiece>();
        }
    }
}
