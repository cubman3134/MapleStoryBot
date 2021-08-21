using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Data
{
    public class MobCluster
    {
        public List<Vector2> Locations;

        public Vector2 Center
        {
            get { return MapleMath.GetAverage(Locations); }
        }

        public MobCluster(Vector2 location)
        {
            Locations = new List<Vector2>() { location };
        }

        public MobCluster()
        {
            Locations = new List<Vector2>();
        }

        public void AddHit(Vector2 newLocation)
        {
            Locations.Add(newLocation);
        }
    }

    public class MobData
    {
        public static double MaxVerticalClusterDistance = 100.0;
        public static double MaxHorizontalClusterDistance = 300.0;
        public static List<MobCluster> FindMobClustersFromPixelData(List<int> mobLocations, int imageWidth, int imageHeight)
        {
            List<MobCluster> mobClusters = new List<MobCluster>();
            foreach (var curMobLocation in mobLocations)
            {
                var curCoordinate = MapleMath.PixelToPixelCoordinate(curMobLocation, imageWidth);
                bool added = false;
                foreach (var curMobCluster in mobClusters)
                {
                    double verticalDistance = Math.Abs(curCoordinate.Y - curMobCluster.Center.Y);
                    double horizontalDistance = Math.Abs(curCoordinate.X - curMobCluster.Center.X);
                    //double distance = MapleMath.PixelCoordinateDistance(curCoordinate, curMobCluster.Center);
                    if (verticalDistance < MaxVerticalClusterDistance 
                        && horizontalDistance < MaxHorizontalClusterDistance)
                    {
                        curMobCluster.AddHit(curCoordinate);
                        added = true;
                        break;
                    }
                }
                if (!added)
                {
                    mobClusters.Add(new MobCluster(curCoordinate));
                }
            }
            return mobClusters;
        }
    }
}
