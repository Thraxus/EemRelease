using Eem.Thraxus.Extensions;
using Sandbox.Game.Entities;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Common.Extensions
{
    internal static class Blocks
    {
        public static bool IsOwnedByNobody(this IMyCubeGrid grid)
        {
            return grid.BigOwners.Count == 0 || grid.BigOwners[0] == 0;
        }

        public static bool IsOwnedByNobody(this IMyCubeBlock block)
        {
            return block.OwnerId == 0;
        }

        public static bool IsBuiltByNobody(this IMyCubeBlock block)
        {
            return ((MyCubeBlock)block).BuiltBy == 0;
        }

        public static bool IsPlayerBlock(this IMySlimBlock block, out IMyPlayer builder)
        {
            builder = null;
            long builtBy = block.BuiltBy;
            if (builtBy == 0) return false;
            builder = builtBy.GetPlayerByIdentityId();
            return builder != null && !builder.IsBot;
        }

        public static bool IsPlayerBlock(this IMyCubeBlock block, out IMyPlayer owner)
        {
            owner = null;
            if (block.OwnerId != 0) return block.OwnerId.ValidPlayer();

            long builtBy = ((MyCubeBlock)block).BuiltBy;
            if (builtBy == 0) return false;
            owner = builtBy.GetPlayerByIdentityId();
            return owner != null && !owner.IsBot;
        }
    }
}