using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Eem.Thraxus.Extensions
{
    public class EntityByDistanceSorter : IComparer<IMyEntity>, IComparer<IMySlimBlock>, IComparer<MyDetectedEntityInfo>
    {
        public EntityByDistanceSorter(Vector3D position)
        {
            Position = position;
        }

        public Vector3D Position { get; set; }

        public int Compare(IMyEntity x, IMyEntity y)
        {
            if (x == null || y == null) return 0;
            double distanceX = Vector3D.DistanceSquared(Position, x.GetPosition());
            double distanceY = Vector3D.DistanceSquared(Position, y.GetPosition());

            if (distanceX < distanceY) return -1;
            return distanceX > distanceY ? 1 : 0;
        }

        public int Compare(IMySlimBlock x, IMySlimBlock y)
        {
            if (x == null || y == null) return 0;
            double distanceX = Vector3D.DistanceSquared(Position, x.CubeGrid.GridIntegerToWorld(x.Position));
            double distanceY = Vector3D.DistanceSquared(Position, y.CubeGrid.GridIntegerToWorld(y.Position));

            if (distanceX < distanceY) return -1;
            return distanceX > distanceY ? 1 : 0;
        }

        public int Compare(MyDetectedEntityInfo x, MyDetectedEntityInfo y)
        {
            double distanceX = Vector3D.DistanceSquared(Position, x.Position);
            double distanceY = Vector3D.DistanceSquared(Position, y.Position);

            if (distanceX < distanceY) return -1;
            return distanceX > distanceY ? 1 : 0;
        }
    }
}