using Eem.Thraxus.Common.Interfaces;

namespace Eem.Thraxus.Models
{
    public class DamageEvent : IReset
    {
        public long ShipId;
        public long BlockId;
        public long PlayerId;

        public void Init(long shipId, long blockId, long playerId)
        {
            ShipId = shipId;
            BlockId = blockId;
            PlayerId = playerId;
        }

        public override string ToString()
        {
            return $"{ShipId} | {BlockId} | {PlayerId}";
        }

        public bool IsReset { get; private set; }
        public void Reset()
        {
            IsReset = true;
            ShipId = 0;
            BlockId = 0;
            PlayerId = 0;
        }
    }
}