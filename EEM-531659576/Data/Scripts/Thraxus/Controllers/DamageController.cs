using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.Generics;
using Eem.Thraxus.Common.Utilities.Statics;
using Eem.Thraxus.Models;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Weapons;
using VRage.Collections;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Eem.Thraxus.Controllers
{
    internal class DamageController : BaseLoggingClass
    {
        private readonly ObjectPool<DamageEvent> _damageEventObjectPool = new ObjectPool<DamageEvent>();

        private DamageEvent GetDamageEvent(long shipId, long blockId, long playerId)
        {
            DamageEvent damageEvent = _damageEventObjectPool.Get();
            damageEvent.ShipId = shipId;
            damageEvent.BlockId = blockId;
            damageEvent.PlayerId = playerId;

            WriteGeneral(nameof(GetDamageEvent), $"Serving DamageEvent...");
            return damageEvent;
        }

        private DamageEvent ReturnDamageEvent(DamageEvent damageEvent)
        {
            WriteGeneral(nameof(ReturnDamageEvent), $"Returning DamageEvent...");
            return damageEvent;
        }


        private ActionQueues _actionQueues;
        private readonly ConcurrentCachingHashSet<DamageEvent> _damageEventList = new ConcurrentCachingHashSet<DamageEvent>();

        public void Init(ActionQueues actionQueues)
        {
            _actionQueues = actionQueues;
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
            _actionQueues.BeforeSimActionQueue.Add(0, () => ProcessDamageQueue(damageEvent));
            WriteGeneral("AddToDamageQueue", $"{damageEvent}");
        }

        private void ProcessDamageQueue(DamageEvent damageEvent)
        {
            TriggerAlert?.Invoke(damageEvent.ShipId, damageEvent.BlockId, damageEvent.PlayerId);
            _damageEventList.Remove(damageEvent);
            _damageEventList.ApplyRemovals();
        }

        private bool DamageAlreadyInQueue(DamageEvent damage)
        {
            return _damageEventList.Contains(damage);
        }

        // Damage Handlers

        private void BeforeDamageHandler(object target, ref MyDamageInformation info)
        {
            //WriteToLog("BeforeDamageHandler", $"{info.AttackerId} | {info.Amount} | {info.Type}", LogType.General);
            if (info.IsDeformation) return;
            IMySlimBlock block = target as IMySlimBlock;
            if (block == null) return;
            long blockId = 0;
            IMyCubeBlock fatBlock = block.FatBlock;
            if (fatBlock != null) blockId = fatBlock.EntityId;
            ProcessPreDamageReporting(new DamageEvent(block.CubeGrid.EntityId, blockId, info.AttackerId, TickCounter), info);
        }

        private void ProcessPreDamageReporting(DamageEvent damage, MyDamageInformation info)
        {
            if (_preDamageEvents.Contains(damage)) return;
            _preDamageEvents.Add(damage);
            _preDamageEvents.ApplyAdditions();
            IdentifyDamageDealer(damage.ShipId, damage.BlockId, info);
        }

        private void CleanPreDamageEvents()
        {
            foreach (DamageEvent damageEvent in _preDamageEvents)
            {
                if (damageEvent.Tick + 1 < TickCounter)
                {
                    _preDamageEvents.Remove(damageEvent);
                }
            }
            _preDamageEvents.ApplyRemovals();
        }

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

            if (attacker is IMyLargeTurretBase)
            {
                IdentifyOffendingIdentityFromEntity(damagedEntity, damagedBlock, attacker);
                return;
            }

            IMyCharacter myCharacter = attacker as IMyCharacter;
            if (myCharacter != null)
            {
                AddToDamageQueue(damagedEntity, damagedBlock, myCharacter.EntityId);
                return;
            }

            IMyAutomaticRifleGun myAutomaticRifle = attacker as IMyAutomaticRifleGun;
            if (myAutomaticRifle != null)
            {
                AddToDamageQueue(damagedEntity, damagedBlock, myAutomaticRifle.OwnerIdentityId);
                return;
            }

            IMyAngleGrinder myAngleGrinder = attacker as IMyAngleGrinder;
            if (myAngleGrinder != null)
            {
                AddToDamageQueue(damagedEntity, damagedBlock, myAngleGrinder.OwnerIdentityId);
                return;
            }

            IMyHandDrill myHandDrill = attacker as IMyHandDrill;
            if (myHandDrill != null)
            {
                AddToDamageQueue(damagedEntity, damagedBlock, myHandDrill.OwnerIdentityId);
                return;
            }

            IMyThrust myThruster = attacker as IMyThrust;
            if (myThruster != null)
            {

                long damagedTopMost = MyAPIGateway.Entities.GetEntityById(damagedEntity).GetTopMostParent().EntityId;
                if (!BotMarshal.ActiveShipRegistry.Contains(damagedTopMost)) return;
                if (!_thrusterDamageTrackers.TryAdd(damagedTopMost, new ThrusterDamageTracker(attacker.EntityId, damageInfo.Amount)))
                    _thrusterDamageTrackers[damagedTopMost].DamageTaken += damageInfo.Amount;
                if (!_thrusterDamageTrackers[damagedTopMost].ThresholdReached) return;
                IdentifyOffendingIdentityFromEntity(damagedEntity, damagedBlock, attacker);
                return;
            }

            WriteGeneral("FindTheAsshole", $"Asshole not identified!!!  It was a: {attacker.GetType()}");
        }

        private void IdentifyOffendingIdentityFromEntity(long damagedEntity, long damagedBlock, IMyEntity offendingEntity)
        {
            try
            {
                IMyCubeGrid myCubeGrid = offendingEntity?.GetTopMostParent() as IMyCubeGrid;
                if (myCubeGrid == null) return;
                if (myCubeGrid.BigOwners.Count == 0)
                {   // This should only trigger when a player is being a cheeky fucker
                    IMyPlayer myPlayer;
                    long tmpId;
                    if (BotMarshal.PlayerShipControllerHistory.TryGetValue(myCubeGrid.EntityId, out tmpId))
                    {
                        myPlayer = MyAPIGateway.Players.GetPlayerById(tmpId);
                        if (myPlayer != null && !myPlayer.IsBot)
                        {
                            AddToDamageQueue(damagedEntity, damagedBlock, myPlayer.IdentityId);
                            return;
                        }
                    }

                    var detectEntitiesInSphere = (List<MyEntity>)Statics.DetectTopMostEntitiesInSphere(myCubeGrid.GetPosition(), BotSettings.UnownedGridDetectionRange);
                    foreach (MyEntity myDetectedEntity in detectEntitiesInSphere)
                    {
                        myPlayer = MyAPIGateway.Players.GetPlayerById(BotMarshal.PlayerShipControllerHistory[myDetectedEntity.EntityId]);
                        if (myPlayer == null || myPlayer.IsBot) continue;
                        AddToDamageQueue(damagedEntity, damagedBlock, myPlayer.IdentityId);
                    }
                    return;
                }

                IMyIdentity myIdentity = Statics.GetIdentityById(myCubeGrid.BigOwners.FirstOrDefault());
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