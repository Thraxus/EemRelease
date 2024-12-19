using System;
using System.Linq;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.Extensions;
using Eem.Thraxus.Common.Generics;
using Eem.Thraxus.Models;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Weapons;
using VRage.Collections;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Eem.Thraxus.Controllers
{
    public class DamageController : BaseLoggingClass
    {
        private readonly CoordinationController _coordinationController;
        private readonly ObjectPool<DamageEvent> _damageEventObjectPool = new ObjectPool<DamageEvent>();
        public readonly MyConcurrentDictionary<long, Action> AlertReporting = new MyConcurrentDictionary<long, Action>();

        private DamageEvent GetDamageEvent(long shipId, long blockId, long playerId)
        {
            DamageEvent damageEvent = _damageEventObjectPool.Get();
            damageEvent.ShipId = shipId;
            damageEvent.BlockId = blockId;
            damageEvent.PlayerId = playerId;

            //WriteGeneral(nameof(GetDamageEvent), $"Serving DamageEvent...");
            return damageEvent;
        }

        private void ReturnDamageEvent(DamageEvent damageEvent)
        {
            //WriteGeneral(nameof(ReturnDamageEvent), $"Returning DamageEvent...[{_damageEventObjectPool}]");
            _damageEventObjectPool.Return(damageEvent);
        }

        // Events
        //public static event Action<long, long, long> TriggerAlert;

        private readonly ActionQueues _actionQueues;
        private readonly ConcurrentCachingHashSet<DamageEvent> _damageEventList = new ConcurrentCachingHashSet<DamageEvent>();

        public DamageController(CoordinationController coordinationController)
        {
            _coordinationController = coordinationController;
            _actionQueues = coordinationController.ActionQueues;
        }

        public void Init()
        {
            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(1, BeforeDamageHandler);
        }
        
        private void AddToDamageQueue(long shipId, long blockId, long playerId)
        {
            AddToDamageQueue(GetDamageEvent(shipId, blockId, playerId));
        }

        private void AddToDamageQueue(DamageEvent damageEvent)
        {
            if (DamageAlreadyInQueue(damageEvent)) return;
            _damageEventList.Add(damageEvent);
            _damageEventList.ApplyAdditions();
            _actionQueues.AfterSimActionQueue.Add(0, () => ProcessDamageQueue(damageEvent));
            WriteGeneral("AddToDamageQueue", $"{damageEvent}");
        }

        private void ProcessDamageQueue(DamageEvent damageEvent)
        {
            //TriggerAlert?.Invoke(damageEvent.ShipId, damageEvent.BlockId, damageEvent.PlayerId);

            Action alertAction;
            AlertReporting.TryGetValue(damageEvent.ShipId, out alertAction);
            alertAction?.Invoke();

            _coordinationController.FactionController.TriggerWar(damageEvent);

            //WriteGeneral(nameof(ProcessDamageQueue), $"Damage Report: [{AlertReporting.ContainsKey(damageEvent.ShipId).ToSingleChar()}] {damageEvent}");
            //foreach (var ar in AlertReporting)
            //{
            //    WriteGeneral(nameof(ProcessDamageQueue), $"Alert Dictionary: [{ar.Key.ToEntityIdFormat()}]");
            //}

            //AlertReporting.ContainsKey(damageEvent.ShipId);
            _damageEventList.Remove(damageEvent);
            _damageEventList.ApplyRemovals();
            ReturnDamageEvent(damageEvent);
        }

        private bool DamageAlreadyInQueue(DamageEvent damage)
        {
            return _damageEventList.Contains(damage);
        }

        // Damage Handlers

        private void BeforeDamageHandler(object target, ref MyDamageInformation info)
        {
            if (info.AttackerId == 0) return;
            if (target == null) return;
            //if (info.IsDeformation) return;
            if (target is IMyFloatingObject) return;

            IMyEntity attackerEntity = MyAPIGateway.Entities.GetEntityById(info.AttackerId);
            if (attackerEntity == null || attackerEntity is IMyMeteor || attackerEntity is IMyThrust)
            { return; }

            var block = target as IMySlimBlock;
            if (block == null) return;
            long blockId = 0;
            IMyCubeBlock fatBlock = block.FatBlock;
            if (fatBlock != null) blockId = fatBlock.EntityId;
            
            MyDamageInformation localDamageInformation = info;
            _actionQueues.BeforeSimActionQueue.Add(1, () =>
            {
                IdentifyDamageDealer(block.CubeGrid.EntityId, blockId, localDamageInformation);
            });
        }

        //private void ProcessPreDamageReporting(DamageEvent damage, MyDamageInformation info)
        //{
        //    if (_preDamageEventList.Contains(damage)) return;
        //    _preDamageEventList.Add(damage);
        //    _preDamageEventList.ApplyAdditions();
        //    IdentifyDamageDealer(damage.ShipId, damage.BlockId, info);
        //}

        //private void CleanPreDamageEvents()
        //{
        //    foreach (DamageEvent damageEvent in _preDamageEventList)
        //    {
        //        if (damageEvent.Tick + 1 < TickCounter)
        //        {
        //            _preDamageEventList.Remove(damageEvent);
        //        }
        //    }
        //    _preDamageEventList.ApplyRemovals();
        //}

        // Supporting Methods
        private void IdentifyDamageDealer(long damagedEntity, long damagedBlock, MyDamageInformation damageInfo)
        {
            // Deformation damage must be allowed here since it handles grid collision damage
            // One idea may be scan loaded mods and grab their damage types for their ammo as well?  We'll see... 
            // Missiles from vanilla launchers track their damage id back to the player, so if unowned or npc owned, they will have no owner - need to entity track missiles, woo! (on entity add)

            try
            {
                IMyEntity attackingEntity;
                if (damageInfo.AttackerId == 0)
                {   // possible instance of a missile getting through to here, need to account for it here or dismiss the damage outright if  no owner can be found
                    return;
                }

                if (!MyAPIGateway.Entities.TryGetEntityById(damageInfo.AttackerId, out attackingEntity)) return;
                FindTheAsshole(damagedEntity, damagedBlock, attackingEntity, damageInfo);
            }
            catch (Exception e)
            {
                WriteGeneral("IdentifyDamageDealer", e.ToString());
            }
        }

        private void FindTheAsshole(long damagedEntity, long damagedBlock, IMyEntity attacker, MyDamageInformation damageInfo)
        {
            if (attacker.GetType() == typeof(MyCubeGrid))
            {
                IdentifyOffendingIdentityFromEntity(damagedEntity, damagedBlock, attacker);
                return;
            }

            var block = attacker as IMyCubeBlock;
            if (block != null)
            {
                var ownerId = ((MyCubeGrid)block.CubeGrid.GetTopMostParent()).BigOwners.Count > 0 ? block.CubeGrid.BigOwners[0] : 0;
                //WriteGeneral("FindTheAsshole", $"Asshole was part of CubeGrid owned by [{ownerId.ToEntityIdFormat()}]");
                if(ownerId > 0)
                {
                    AddToDamageQueue(damagedEntity, damagedBlock, ownerId);
                    return;
                }
                WriteGeneral("FindTheAsshole", $"Asshole was an unowned [{attacker.GetType()}] !!!");
                return;
            }

            var myCharacter = attacker as IMyCharacter;
            if (myCharacter != null)
            {
                AddToDamageQueue(damagedEntity, damagedBlock, myCharacter.EntityId);
                return;
            }

            var myAutomaticRifle = attacker as IMyAutomaticRifleGun;
            if (myAutomaticRifle != null)
            {
                AddToDamageQueue(damagedEntity, damagedBlock, myAutomaticRifle.OwnerIdentityId);
                return;
            }

            var myAngleGrinder = attacker as IMyAngleGrinder;
            if (myAngleGrinder != null)
            {
                AddToDamageQueue(damagedEntity, damagedBlock, myAngleGrinder.OwnerIdentityId);
                return;
            }

            var myHandDrill = attacker as IMyHandDrill;
            if (myHandDrill != null)
            {
                AddToDamageQueue(damagedEntity, damagedBlock, myHandDrill.OwnerIdentityId);
                return;
            }

            

            //IMyThrust myThruster = attacker as IMyThrust;
            //if (myThruster != null)
            //{

            //    long damagedTopMost = MyAPIGateway.Entities.GetEntityById(damagedEntity).GetTopMostParent().EntityId;
            //    if (!BotMarshal.ActiveShipRegistry.Contains(damagedTopMost)) return;
            //    if (!_thrusterDamageTrackers.TryAdd(damagedTopMost, new ThrusterDamageTracker(attacker.EntityId, damageInfo.Amount)))
            //        _thrusterDamageTrackers[damagedTopMost].DamageTaken += damageInfo.Amount;
            //    if (!_thrusterDamageTrackers[damagedTopMost].ThresholdReached) return;
            //    IdentifyOffendingIdentityFromEntity(damagedEntity, damagedBlock, attacker);
            //    return;
            //}

            WriteGeneral("FindTheAsshole", $"Asshole not identified!!!  It was a: {attacker.GetType()}");
        }

        private void IdentifyOffendingIdentityFromEntity(long damagedEntity, long damagedBlock, IMyEntity offendingEntity)
        {
            try
            {
                IMyCubeGrid myCubeGrid = offendingEntity?.GetTopMostParent() as IMyCubeGrid;
                if (myCubeGrid == null) return;
                //if (myCubeGrid.BigOwners.Count == 0)
                //{   // This should only trigger when a player is being a cheeky fucker
                //    IMyPlayer myPlayer;
                //    long tmpId;
                //    if (BotMarshal.PlayerShipControllerHistory.TryGetValue(myCubeGrid.EntityId, out tmpId))
                //    {
                //        myPlayer = MyAPIGateway.Players.GetPlayerById(tmpId);
                //        if (myPlayer != null && !myPlayer.IsBot)
                //        {
                //            AddToDamageQueue(damagedEntity, damagedBlock, myPlayer.IdentityId);
                //            return;
                //        }
                //    }

                //    var detectEntitiesInSphere = (List<MyEntity>)Statics.DetectTopMostEntitiesInSphere(myCubeGrid.GetPosition(), BotSettings.UnownedGridDetectionRange);
                //    foreach (MyEntity myDetectedEntity in detectEntitiesInSphere)
                //    {
                //        myPlayer = MyAPIGateway.Players.GetPlayerById(BotMarshal.PlayerShipControllerHistory[myDetectedEntity.EntityId]);
                //        if (myPlayer == null || myPlayer.IsBot) continue;
                //        AddToDamageQueue(damagedEntity, damagedBlock, myPlayer.IdentityId);
                //    }
                //    return;
                //}

                IMyIdentity myIdentity = myCubeGrid.BigOwners.FirstOrDefault().GetIdentityFromIdentityId();
                if (myIdentity != null)
                {
                    AddToDamageQueue(damagedEntity, damagedBlock, myIdentity.IdentityId);
                    return;
                }

                WriteGeneral("IdentifyOffendingPlayerFromEntity", $"War Target is an elusive shithead! {myCubeGrid.BigOwners.FirstOrDefault()}");
            }
            catch (Exception e)
            {
                WriteGeneral("IdentifyOffendingPlayer", e.ToString());
            }
        }
    }
}