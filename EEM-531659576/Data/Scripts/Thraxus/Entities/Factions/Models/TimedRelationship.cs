using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Entities.Factions.Models
{
    public class TimedRelationship : IEquatable<TimedRelationship>
    {
        public TimedRelationship(IMyFaction aiFaction, IMyFaction playerFaction, long cooldownTime)
        {
            NpcFaction = aiFaction;
            PlayerFaction = playerFaction;
            CooldownTime = cooldownTime;
        }

        public IMyFaction NpcFaction { get; }

        public IMyFaction PlayerFaction { get; }

        public long CooldownTime { get; set; }

        public bool Equals(TimedRelationship other)
        {
            return other != null &&
                   EqualityComparer<IMyFaction>.Default.Equals(NpcFaction, other.NpcFaction) &&
                   EqualityComparer<IMyFaction>.Default.Equals(PlayerFaction, other.PlayerFaction);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"NpcFaction:\t{NpcFaction.FactionId}\t{NpcFaction.Tag}\tNpcFaction:\t{PlayerFaction.FactionId}\t{PlayerFaction.Tag}\tCooldownTime:\t{CooldownTime}";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TimedRelationship);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((NpcFaction != null ? NpcFaction.GetHashCode() : 0) * 397) ^ (PlayerFaction != null ? PlayerFaction.GetHashCode() : 0);
            }
        }
    }
}