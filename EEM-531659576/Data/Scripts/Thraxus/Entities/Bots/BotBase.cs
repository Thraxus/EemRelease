using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.Enums;
using Eem.Thraxus.Common.Extensions;
using Eem.Thraxus.Common.Generics;
using Eem.Thraxus.Common.Utilities.Statics;
using Eem.Thraxus.Extensions;
using Eem.Thraxus.Helpers;
using Eem.Thraxus.Models;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRageMath;
using IMyRadioAntenna = Sandbox.ModAPI.IMyRadioAntenna;
using IMyRemoteControl = Sandbox.ModAPI.IMyRemoteControl;

namespace Eem.Thraxus.Entities.Bots
{
    public abstract class BotBase : BaseEntity
    {
        public abstract void TriggerAlert();

        private IMyFaction _ownerFaction;

        protected bool BotOperable;

        protected bool Closed;
        
        protected readonly BotConfig BotConfig;
        
        public List<IMyRadioAntenna> Antennae { get; protected set; }
        public List<IMyTimerBlock> Timers { get; protected set; }

        public event Action<long, long> TriggerWar;

        private readonly ActionQueue _actionQueue = new ActionQueue();

        protected BotBase(IMyCubeGrid grid, BotConfig botConfig)
        {
            if (grid == null) return;
            Grid = grid;
            SetLogPrefix(grid.DisplayName);
            Antennae = new List<IMyRadioAntenna>();
            BotConfig = botConfig;
        }

        public virtual void Main()
        {
            _actionQueue?.Execute();
        }

        public IMyCubeGrid Grid { get; protected set; }

        public Vector3D GridPosition => Grid.GetPosition();

        public Vector3D GridVelocity => Grid.Physics.LinearVelocity;

        public float GridSpeed => (float)GridVelocity.Length();

        protected float GridRadius => (float)Grid.WorldVolume.Radius;

        public IMyRemoteControl Rc { get; protected set; }

        protected string DroneNameProvider => $"Drone_{Rc.EntityId}";

        protected string DroneName;

        protected bool GridOperable
        {
            get
            {
                try
                {
                    return !Grid.MarkedForClose && !Grid.Closed && Grid.InScene;
                }
                catch (Exception e)
                {
                    WriteGeneral("GridOperable", $"{e}");
                    return false;
                }
            }
        }

        public virtual bool Init(IMyRemoteControl rc)
        {
            Rc = rc;
            if (rc == null) return false;
            DroneName = $"Drone_{Grid.EntityId.ToEntityIdFormat()}";
            rc.IsMainCockpit = true;
            rc.IsWorkingChanged += block => Shutdown();
            WriteGeneral("Init", $"Bot Base Booting... [{DroneName}]");

            Antennae = Grid.GetFatBlocks<IMyRadioAntenna>().ToList();
            Timers = Grid.GetFatBlocks<IMyTimerBlock>().ToList();

            ParseSetup();

            SubscriptionHandler();

            _ownerFaction = Grid.GetOwnerFaction();

            BotOperable = true;

            return true;
        }

        public override void Subscribe()
        {
            Grid.OnBlockAdded += BlockPlacedHandler;
        }

        public override void UnSubscribe()
        {
            Grid.OnBlockAdded -= BlockPlacedHandler;
        }

        // Struct in BotBase: Stack-alloc friendly, caches stable fields (C#6 value type)
        private struct BlockPlacementEvent
        {
            public Vector3I Position;
            public long OwnerId;
            public long BuiltBy; // Assuming long; adjust if int

            public BlockPlacementEvent(IMySlimBlock block)
            {
                Position = block.Position;
                OwnerId = block.OwnerId;
                BuiltBy = block.BuiltBy; // Pre-cache to avoid re-fetch
            }
        }

        protected void BlockPlacedHandler(IMySlimBlock block)
        {
            if (block == null || !GridOperable) return;

            var evt = new BlockPlacementEvent(block);
            _actionQueue.Add(1, () => EvaluatePlacedBlock(evt)); // Struct by value: No capture/closure
        }

        private void EvaluatePlacedBlock(BlockPlacementEvent evt)
        {
            if (!GridOperable) return;

            // Predicate for exact pos/owner match: Allocates delegate (~20B), but API-internal loop short-circuits
            List<IMySlimBlock> tempBlocks = new List<IMySlimBlock>(1); // Tiny, reuse via pool if multi-events
            Grid.GetBlocks(tempBlocks, s => s.Position == evt.Position && s.OwnerId == evt.OwnerId);

            if (tempBlocks.Count == 0) return; // No match (stale/rare)

            var block = tempBlocks[0]; // Single hit guaranteed by predicate

            try
            {
                long myOwner = Grid.BigOwners?.Count > 0 ? Grid.BigOwners[0] : 0L;
                if (evt.OwnerId == myOwner) return; // Use cached

                if (Constants.DebugMode)
                {
                    WriteGeneral("BlockPlacedHandler", $"Unauthorized at {evt.Position}: OwnerId={evt.OwnerId}, BuiltBy={evt.BuiltBy}, HasWarEvent={TriggerWar != null}");
                }

                TriggerWar?.Invoke(Grid.EntityId, evt.OwnerId); // Use cached OwnerId
            }
            catch (Exception e)
            {
                if (Constants.DebugMode) WriteGeneral("BlockPlacedHandler", e.Message);
            }
            finally
            {
                tempBlocks.Clear(); // Reuse next event
            }
        }

        protected HashSet<MyEntity> FindTargets(float distance, bool includeNeutrals = false)
        {
            HashSet<MyEntity> detectTopMostEntitiesInSphere = Statics.DetectTopMostEntitiesInSphere(Rc.GetPosition(), distance).ToHashSet();

            HashSet<MyEntity> filteredTargets = FilterTargets(detectTopMostEntitiesInSphere, includeNeutrals);

            return filteredTargets;
        }

        private readonly HashSet<MyEntity> _filteredTargets = new HashSet<MyEntity>();
        private readonly Dictionary<long, MyRelationsBetweenFactions> _relationsCache = new Dictionary<long, MyRelationsBetweenFactions>(32);
        private int _errorCount;
        private int _totalProcessed;
        private long _myFactionId;

        protected HashSet<MyEntity> FilterTargets(HashSet<MyEntity> targets, bool includeNeutrals)
        {
            _filteredTargets.Clear();

            if (targets.Count == 0) return _filteredTargets; // No enumerator

            _myFactionId = Rc?.GetOwnerFaction()?.FactionId ?? 0L;
            _errorCount = 0;
            _totalProcessed = 0;
            _relationsCache.Clear();

            foreach (var target in targets)
            {
                if (target == null) continue; // Hoist null
                _totalProcessed++;

                try
                {
                    var targetGrid = target as MyCubeGrid;
                    if (targetGrid != null)
                    {
                        var targetFactionId = targetGrid.GetOwnerFaction()?.FactionId ?? 0L;
                        if (targetFactionId == _myFactionId) continue;

                        if (includeNeutrals)
                        {
                            _filteredTargets.Add(targetGrid);
                            continue;
                        }

                        MyRelationsBetweenFactions relations;
                        if (!_relationsCache.TryGetValue(targetFactionId, out relations))
                        {
                            relations = MyAPIGateway.Session.Factions.GetRelationBetweenFactions(_myFactionId, targetFactionId);
                            _relationsCache[targetFactionId] = relations;
                        }
                        if (relations != MyRelationsBetweenFactions.Enemies) continue;

                        _filteredTargets.Add(targetGrid);
                        continue;
                    }

                    var targetCharacter = target as IMyCharacter;
                    if (targetCharacter == null) continue;

                    if (includeNeutrals)
                    {
                        _filteredTargets.Add(target);
                        continue;
                    }

                    // Assume Statics.GetRelation is cheap; cache if profiling shows otherwise
                    if (Statics.GetRelationBetweenGridAndCharacterUsingEntity(Rc?.CubeGrid, target) == FactionRelationship.Enemies)
                    {
                        _filteredTargets.Add(target);
                    }
                }
                catch (InvalidOperationException e)
                {
                    _errorCount++;
                    if (Constants.DebugMode) WriteGeneral(nameof(FilterTargets), $"Entity skipped (ID: {target?.EntityId}): {e.Message}"); // Drop ? if hoisted null
                }
                catch (Exception e)
                {
                    _errorCount++;
                    WriteGeneral(nameof(FilterTargets), $"Unexpected error on target {target.EntityId}: {e.Message}");
                    if (_totalProcessed != 0 && _errorCount * 5 <= _totalProcessed) continue; // Int-safe
                    _filteredTargets.Clear();
                    break;
                }
            }

            return _filteredTargets;
        }

        protected MyEntity GetClosestEntity(HashSet<MyEntity> targets)
        {
            switch (targets.Count)
            {
                case 0:
                    return null; // Add: Handle empty (though caller should, safety)
                case 1:
                    return targets.First(); // O(1) fast path unchanged
            }

            MyEntity closest = null;
            float minDistSq = float.MaxValue; // Use squared dist: Avoid sqrt in DistanceTo for 10-20% speed
            Vector3D myPos = GridPosition; // Cache once

            foreach (MyEntity entity in targets) // Foreach is fine here (HashSet enumerator cheap for small n)
            {
                if (entity?.PositionComp == null) continue; // Null-safe, rare

                float distSq = (float)Vector3D.DistanceSquared(myPos, entity.PositionComp.GetPosition());
                if (distSq >= minDistSq) continue;
                minDistSq = distSq;
                closest = entity;
            }

            return closest;
        }

        private readonly HashSet<MyEntity>[] _distanceBuckets =
        {
            new HashSet<MyEntity>(),
            new HashSet<MyEntity>(),
            new HashSet<MyEntity>(),
            new HashSet<MyEntity>(),
            new HashSet<MyEntity>(),
            new HashSet<MyEntity>(),
            new HashSet<MyEntity>()
        };

        private void ClearDistanceSortedEnemies()
        {
            for (int i = 0; i < 7; i++) _distanceBuckets[i].Clear();
        }

        private void AddToDistanceSortedEnemies(MyEntity entity, double distance)
        {
            int bucket = Math.Min(6, (int)(distance / 500));
            _distanceBuckets[bucket].Add(entity);
        }

        protected HashSet<MyEntity>[] GetEnemiesSortedByRange(HashSet<MyEntity> enemies) // Note: Array return
        {
            ClearDistanceSortedEnemies();

            var reference = GridPosition;

            foreach (var enemy in enemies)
            {
                if (!ValidateTarget(enemy)) continue;
                double dist = reference.DistanceTo(enemy.PositionComp.GetPosition());
                AddToDistanceSortedEnemies(enemy, dist);
            }

            return _distanceBuckets;
        }

        private bool ValidateTarget(MyEntity entity)
        {
            if (entity is IMyCharacter) return true;

            foreach (var block in ((MyCubeGrid)entity).GetFatBlocks())
            {
                if (block.IsFunctional &&
                    (block is IMyShipController ||
                     block is IMyPowerProducer ||
                     block is IMySmallGatlingGun ||
                     block is IMySmallMissileLauncher))
                {
                    return true; // Early exit on first match
                }
            }
            return false;
        }

        protected abstract void ParseSetup();

        public virtual void Shutdown()
        {
            Closed = true;
            SubscriptionHandler(true);
            Close();
        }
    }
}