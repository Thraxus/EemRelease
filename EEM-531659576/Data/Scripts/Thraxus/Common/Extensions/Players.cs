using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Eem.Thraxus.Common.Extensions
{
    internal static class Players
    {
        private static readonly List<IMyPlayer> OnlinePlayers = new List<IMyPlayer>();
        
        public static IMyPlayer GetPlayerFromEntity(this IMyEntity entity)
        {
            OnlinePlayers.Clear();
            IMyPlayer returnedPlayer = null;
            MyAPIGateway.Multiplayer.Players.GetPlayers(OnlinePlayers);
            
            foreach (var myPlayer in OnlinePlayers)
            {
                if (myPlayer.Character.EntityId != entity.EntityId) continue;
                returnedPlayer = myPlayer;
                break;
            }

            return returnedPlayer;
        }

        private static readonly List<IMyIdentity> AllIdentities = new List<IMyIdentity>();

        public static IMyIdentity GetIdentityFromIdentityId(this long identityId)
        {
            AllIdentities.Clear();
            IMyIdentity returnedIdentity = null;
            MyAPIGateway.Players.GetAllIdentites(AllIdentities);
            foreach (var identity in AllIdentities)
            {
                if (identity.IdentityId != identityId) continue;
                returnedIdentity = identity;
                break;
            }

            return returnedIdentity;
        }

        public static IMyPlayer GetPlayerByIdentityId(this long identityId)
        {
            OnlinePlayers.Clear();

            IMyPlayer returnedPlayer = null;
            MyAPIGateway.Multiplayer.Players.GetPlayers(OnlinePlayers);

            foreach (var myPlayer in OnlinePlayers)
            {
                if (myPlayer.Character.EntityId != identityId) continue;
                returnedPlayer = myPlayer;
            }

            return returnedPlayer;
        }

        public static IMyPlayer GetPlayerByPlayerId(long playerId)
        {
            OnlinePlayers.Clear();
            IMyPlayer returnedPlayer = null;
            MyAPIGateway.Multiplayer.Players.GetPlayers(OnlinePlayers);

            foreach (var myPlayer in OnlinePlayers)
            {
                if (myPlayer.Character.EntityId != playerId) continue;
                returnedPlayer = myPlayer;
                break;
            }

            return returnedPlayer;
        }
    }
}