using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Common.Extensions
{
    public static class Longs
    {
        public static string ToEntityIdFormat(this long tef)
        {
            return $"{tef:D18}";
        }

        public static IMyFaction GetFactionById(this long factionId)
        {
            return MyAPIGateway.Session.Factions.TryGetFactionById(factionId);
        }

        public static bool IsPlayerFaction(this long faction)
        {
            return !MyAPIGateway.Session.Factions.TryGetFactionById(faction).IsEveryoneNpc();
        }

        public static bool IsNpcFaction(this long faction)
        {
            return MyAPIGateway.Session.Factions.TryGetFactionById(faction).IsEveryoneNpc();
        }


        public static bool ValidPlayer(this long identityId)
        {
            return MyAPIGateway.Players.TryGetSteamId(identityId) != 0;
        }

        public static bool ValidPlayerFaction(this long factionId)
        {
            IMyFaction faction = factionId.GetFactionById();
            if (faction == null) return false;
            return MyAPIGateway.Players.TryGetSteamId(faction.FounderId) != 0;
        }
    }
}