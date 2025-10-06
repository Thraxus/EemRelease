using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.Enums;
using Eem.Thraxus.Common.Extensions;
using Eem.Thraxus.Common.Generics;
using Eem.Thraxus.Common.Utilities.Statics;
using Eem.Thraxus.Extensions;
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
    public abstract class BotBase : BaseLoggingClass
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
            OverrideLogPrefix(grid.DisplayName);
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
            
            Grid.OnBlockAdded += BlockPlacedHandler;

            _ownerFaction = Grid.GetOwnerFaction();

            BotOperable = true;

            return true;
        }

        protected void BlockPlacedHandler(IMySlimBlock block)
        {
            _actionQueue.Add(1, () => EvaluatePlacedBlock(block));
        }

        protected void EvaluatePlacedBlock(IMySlimBlock block)
        {
            if (block == null) return;
            
            try
            {
                WriteGeneral("BlockPlacedHandler", $"{block.OwnerId} {block.BuiltBy} {TriggerWar == null}");

                if (Grid.BigOwners == null || Grid.BigOwners.Count == 0) return;
                if (block.OwnerId == Grid.BigOwners[0]) return;

                WriteGeneral("BlockPlacedHandler", $"{block.OwnerId} {block.BuiltBy}");

                TriggerWar?.Invoke(Grid.EntityId, block.OwnerId);

            }
            catch (Exception e)
            {
                WriteGeneral("BlockPlacedHandler", $"{e}");
            }
        }

        protected HashSet<MyEntity> FindTargets(float distance, bool includeNeutrals = false)
        {
            HashSet<MyEntity> detectTopMostEntitiesInSphere = Statics.DetectTopMostEntitiesInSphere(Rc.GetPosition(), distance).ToHashSet();

            HashSet<MyEntity> filteredTargets = FilterTargets(detectTopMostEntitiesInSphere, includeNeutrals);

            return filteredTargets;
        }

        private readonly HashSet<MyEntity> _filteredTargets = new HashSet<MyEntity>();

        protected HashSet<MyEntity> FilterTargets(HashSet<MyEntity> targets, bool includeNeutrals)
        {
            _filteredTargets.Clear();

            if (!targets.Any()) return targets;

            try
            {
                foreach (var target in targets)
                {
                    var targetGrid = target as MyCubeGrid;
                    if (targetGrid != null)
                    {
                        IMyFaction targetGridFaction = targetGrid.GetOwnerFaction();
                        if (targetGridFaction != null)
                        {
                            if (Rc.GetOwnerFaction().FactionId == targetGridFaction.FactionId)
                            {
                                continue;
                            }

                            if (includeNeutrals)
                            {
                                _filteredTargets.Add(targetGrid);
                                continue;
                            }

                            MyRelationsBetweenFactions myRelationsBetweenFactions = MyAPIGateway.Session.Factions.GetRelationBetweenFactions(Rc.GetOwnerFaction().FactionId, targetGridFaction.FactionId);

                            if (myRelationsBetweenFactions != MyRelationsBetweenFactions.Enemies) continue;

                            _filteredTargets.Add(targetGrid);
                        }
                        continue;
                    }

                    var targetCharacter = target as IMyCharacter;
                    if (targetCharacter == null) continue;

                    if (includeNeutrals)
                    {
                        _filteredTargets.Add(target);
                        continue;
                    }

                    if (Statics.GetRelationBetweenGridAndCharacterUsingEntity(Rc.CubeGrid, target) == FactionRelationship.Enemies)
                    {
                        _filteredTargets.Add(target);
                    }
                }
            }
            catch (Exception e)
            {
                //WriteGeneral(nameof(FilterTargets),$"This error shouldn't hurt anything, but tell Thraxus if you see it: \n {e}");
                _filteredTargets.Clear();
                return _filteredTargets;
            }

            return _filteredTargets;
        }

        protected MyEntity GetClosestEntity(HashSet<MyEntity> targets)
        {
            if (targets.Count == 1)
            {
                return targets.First();
            }

            MyEntity closestEntity = targets.OrderBy(x => GridPosition.DistanceTo(x.PositionComp.GetPosition())).FirstOrDefault();
            return closestEntity;
        }

        protected Dictionary<int, HashSet<MyEntity>> DistanceSortedEnemies = new Dictionary<int, HashSet<MyEntity>>()
        {
            { 0, new HashSet<MyEntity>() },
            { 1, new HashSet<MyEntity>() },
            { 2, new HashSet<MyEntity>() },
            { 3, new HashSet<MyEntity>() },
            { 4, new HashSet<MyEntity>() },
            { 5, new HashSet<MyEntity>() },
            { 6, new HashSet<MyEntity>() }
        };

        private void ClearDistanceSortedEnemies()
        {
            foreach (var dse in DistanceSortedEnemies)
            {
                dse.Value.Clear();
            }
        }

        private void AddToDistanceSortedEnemies(MyEntity entity, double distance)
        {
            if (distance <= 499)
            {
                DistanceSortedEnemies[0].Add(entity);
                return;
            }
            if (distance <= 999)
            {
                DistanceSortedEnemies[1].Add(entity);
                return;
            }
            if (distance <= 1499)
            {
                DistanceSortedEnemies[2].Add(entity);
                return;
            }
            if (distance <= 1999)
            {
                DistanceSortedEnemies[3].Add(entity);
                return;
            }
            if (distance <= 2499)
            {
                DistanceSortedEnemies[4].Add(entity);
                return;
            }

            if (distance <= 2999)
            {
                DistanceSortedEnemies[5].Add(entity);
                return;
            }

            DistanceSortedEnemies[6].Add(entity);
        }

        protected Dictionary<int, HashSet<MyEntity>> GetEnemiesSortedByRange(HashSet<MyEntity> enemies)
        {
            ClearDistanceSortedEnemies();

            var reference = GridPosition;

            foreach (var enemy in enemies)
            {
                if (!ValidateTarget(enemy)) continue;
                AddToDistanceSortedEnemies(enemy, reference.DistanceTo(enemy.PositionComp.GetPosition()));
            }

            return DistanceSortedEnemies;
        }

        private bool ValidateTarget(MyEntity entity)
        {
            if (entity is IMyCharacter) return true;

            var grid = (IMyCubeGrid)entity;
            var controllers = grid.GetFatBlocks<IMyShipController>().ToHashSet();
            if (controllers.Any())
            {
                if (!ValidateBlockGroup(controllers)) return false;
            }

            // We know we have a controller at this point
            HashSet<IMyPowerProducer> powerProducers = grid.GetFatBlocks<IMyPowerProducer>().ToHashSet();
            if (powerProducers.Any())
            {
                if (!ValidateBlockGroup(powerProducers)) return false;
            }

            // So at this point we have power and control, but do we have weapons?
            HashSet<IMyCubeBlock> bangBangs = grid.GetFatBlocks<IMyCubeBlock>().ToHashSet();
            bangBangs.UnionWith(grid.GetFatBlocks<IMySmallGatlingGun>().ToHashSet());
            bangBangs.UnionWith(grid.GetFatBlocks<IMySmallMissileLauncher>().ToHashSet());
            if (bangBangs.Any())
            {
                if (!ValidateBlockGroup(bangBangs)) return false;
            }

            return true;
        }

        private bool ValidateBlockGroup<T>(HashSet<T> group)
        {
            foreach (var block in group)
            {
                if (((IMyCubeBlock)block).IsFunctional) return true;
            }

            return false;
        }

        protected abstract void ParseSetup();

        public virtual void Shutdown()
        {
            Closed = true;
            Grid.OnBlockAdded -= BlockPlacedHandler;
            Close();
        }
    }
}