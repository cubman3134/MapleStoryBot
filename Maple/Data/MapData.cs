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
