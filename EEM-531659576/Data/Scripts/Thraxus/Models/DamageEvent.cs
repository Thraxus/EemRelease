using System;
using Eem.Thraxus.Common.Interfaces;

namespace Eem.Thraxus.Models
{
    public class DamageEvent : IResetWithEvent<DamageEvent>
    {
        public long ShipId;
        public long BlockId;
        public long PlayerId;
        public long Tick;

        public void Init(long shipId, long blockId, long playerId, long tick)
        {
            ShipId = shipId;
            BlockId = blockId;
            PlayerId = playerId;
            Tick = tick;
        }

        public override string ToString()
        {
            return $"{ShipId} | {BlockId} | {PlayerId} | {Tick}";
        }

        public bool IsReset { get; private set; }
        public void Reset()
        {
            IsReset = true;
            ShipId = 0;
            BlockId = 0;
            PlayerId = 0;
            Tick = 0;
        }

        public event Action<DamageEvent> OnReset;
    }
}