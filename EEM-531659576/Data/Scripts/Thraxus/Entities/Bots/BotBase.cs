using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.Extensions;
using Eem.Thraxus.Extensions;
using Eem.Thraxus.Helpers;
using Eem.Thraxus.Models;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using IMyGridTerminalSystem = Sandbox.ModAPI.IMyGridTerminalSystem;
using IMyRadioAntenna = Sandbox.ModAPI.IMyRadioAntenna;
using IMyRemoteControl = Sandbox.ModAPI.IMyRemoteControl;
using IMyTerminalBlock = Sandbox.ModAPI.IMyTerminalBlock;
using IMyThrust = Sandbox.ModAPI.IMyThrust;

namespace Eem.Thraxus.Entities.Bots
{
    public abstract class BotBase : BaseLoggingClass
    {
        public delegate void HOnBlockPlaced(IMySlimBlock block);

        public delegate void OnDamageTaken(IMySlimBlock damagedBlock, MyDamageInformation damage);

        public abstract void TriggerAlert();

        //private readonly BotDamageHandler _botDamageHandler;

        protected readonly IMyGridTerminalSystem Term;

        private IMyFaction _ownerFaction;

        protected bool BotOperable;

        protected bool Closed;

        protected List<IMyThrust> SpeedModdedThrusters = new List<IMyThrust>();

        protected readonly BotConfig BotConfig;
        
        public List<IMyRadioAntenna> Antennae { get; protected set; }

        //protected event OnDamageTaken OnDamaged;

        protected event HOnBlockPlaced OnBlockPlaced;

        //protected event Action Alert;

        protected BotBase(IMyCubeGrid grid, BotConfig botConfig)
        {
            if (grid == null) return;
            Grid = grid;
            //_botDamageHandler = botDamageHandler;
            Term = grid.GetTerminalSystem();
            Antennae = new List<IMyRadioAntenna>();
            BotConfig = botConfig;
        }

        public IMyCubeGrid Grid { get; protected set; }

        public Vector3D GridPosition => Grid.GetPosition();

        public Vector3D GridVelocity => Grid.Physics.LinearVelocity;

        public float GridSpeed => (float)GridVelocity.Length();

        protected float GridRadius => (float)Grid.WorldVolume.Radius;

        public IMyRemoteControl Rc { get; protected set; }

        protected string DroneNameProvider => $"Drone_{Rc.EntityId}";

        protected bool HasModdedThrusters => SpeedModdedThrusters.Count > 0;

        //protected string DroneName;

        public string DroneName
        {
            get { return Rc.Name; }
            protected set
            {
                IMyEntity entity = Rc;
                entity.Name = value;
                MyAPIGateway.Entities.SetEntityName(entity);
            }
        }

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

        //public virtual bool Operable
        //{
        //	get
        //	{
        //		try
        //		{
        //			return !Closed && IsInitialized && GridOperable && Rc.IsFunctional && BotOperable;
        //		}
        //		catch (Exception scrap)
        //		{
        //			LogError("Operable", scrap);
        //			return false;
        //		}
        //	}
        //}


        private void TriggerWar(long assaulted, long assaulter)
        {
            WriteGeneral("TriggerWar", $"Asshats! [{assaulted.ToEntityIdFormat()}] [{assaulter.ToEntityIdFormat()}]");
            //_botDamageHandler.TriggerWar(assaulted, assaulter);
        }

        //public BotType ReadBotType(IMyRemoteControl rc)
        //{
        //    try
        //    {
        //        string customData = rc.CustomData.Trim().Replace("\r\n", "\n");
        //        var myCustomData = new List<string>(customData.Split('\n'));

        //        if (customData.IsNullEmptyOrWhiteSpace()) return BotType.None;
        //        if (myCustomData.Count < 2)
        //        {
        //            if (Constants.AllowThrowingErrors) throw new Exception("CustomData is invalid", new Exception("CustomData consists of less than two lines"));
        //            return BotType.Invalid;
        //        }

        //        if (myCustomData[0].Trim() != "[EEM_AI]")
        //        {
        //            if (Constants.AllowThrowingErrors) throw new Exception("CustomData is invalid", new Exception($"AI tag invalid: '{myCustomData[0]}'"));
        //            return BotType.Invalid;
        //        }

        //        string[] botTypeFromCustomData = myCustomData[1].Split(':');
        //        if (botTypeFromCustomData[0].Trim() != "Type")
        //        {
        //            if (Constants.AllowThrowingErrors) throw new Exception("CustomData is invalid", new Exception($"Type tag invalid: '{botTypeFromCustomData[0]}'"));
        //            return BotType.Invalid;
        //        }

        //        var botType = BotType.Invalid;

        //        switch (botTypeFromCustomData[1].Trim())
        //        {
        //            case "Fighter":
        //                botType = BotType.Fighter;
        //                break;
        //            case "Freighter":
        //                botType = BotType.Freighter;
        //                break;
        //            case "Carrier":
        //                botType = BotType.Carrier;
        //                break;
        //            case "Station":
        //                botType = BotType.Station;
        //                break;
        //        }

        //        return botType;
        //    }
        //    catch (Exception e)
        //    {
        //        WriteGeneral("ReadBotType", $"{e}");
        //        return BotType.Invalid;
        //    }
        //}

        public virtual bool Init(IMyRemoteControl rc)
        {
            Rc = rc ?? Term.GetBlocksOfType<IMyRemoteControl>(x => x.IsFunctional).FirstOrDefault();
            if (rc == null) return false;
            DroneName = $"Drone_{Grid.EntityId.ToEntityIdFormat()}";

            WriteGeneral("Init", $"Bot Base Booting... [{DroneName}]");

            Antennae = Term.GetBlocksOfType<IMyRadioAntenna>(x => x.IsFunctional);
            ParseSetup();

            //bool hasSetup = ParseSetup();
            //if (!hasSetup) return false;

            //_botDamageHandler.AddDamageHandler(Grid, (block, damage) => { OnDamaged?.Invoke(block, damage); });

            Grid.OnBlockAdded += block => { OnBlockPlaced?.Invoke(block); };

            _ownerFaction = Grid.GetOwnerFaction();

            BotOperable = true;

            return true;
        }

        //public virtual void RecompilePBs()
        //{
        //	foreach (IMyProgrammableBlock pb in Term.GetBlocksOfType<IMyProgrammableBlock>())
        //	{
        //		MyAPIGateway.Utilities.InvokeOnGameThread(() => { pb.Recompile(); });
        //	}
        //}

        protected void ReactOnDamage(MyDamageInformation damage, out IMyPlayer damager)
        {
            damager = null;
            try
            {
                ////AiSessionCore.DebugLog?.WriteToLog("ReactOnDamage", $"damage.AttackerId:\t{damage.AttackerId}");

                //// Inconsequential damage sources, ignore them
                //if (damage.IsDeformation || damage.IsMeteor() || damage.IsThruster())
                //    return;

                ////if (damage.IsDoneByPlayer(out damager) && damager != null)
                ////	if (damager.GetFaction() == null) return;

                //if (damage.IsDoneByPlayer(out damager))
                //{
                //    if (damager.GetFaction() == null) return;
                //    DeclareWar(damager.GetFaction());
                //    return;
                //}

                //// Issue here is damager == null at this point.  Damager is IMyPlayer
                //// Since damager == null, DeclareWar fails.  
                //// Need to check for damager == null here, if so, get pilot and declare war on them if they are in a faction

                //var possibleAttackingPlayers = new List<IMyPlayer>();
                //MyAPIGateway.Players.GetPlayers(possibleAttackingPlayers,
                //    player =>
                //        player.Controller.ControlledEntity.Entity != null &&
                //        !player.IsBot &&
                //        player.Character != null &&
                //        player.Controller.ControlledEntity.Entity is IMyShipController);
                //foreach (IMyPlayer possibleAttackingPlayer in possibleAttackingPlayers)
                //{
                //    if (((IMyShipController)possibleAttackingPlayer.Controller.ControlledEntity.Entity).SlimBlock.CubeGrid !=
                //        MyAPIGateway.Entities.GetEntityById(damage.AttackerId)) continue;
                //    damager = possibleAttackingPlayer;
                //    IMyFaction attackingFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(possibleAttackingPlayer.IdentityId);
                //    if (attackingFaction == null)
                //        continue;
                //    DeclareWar(attackingFaction);
                //}

                //HashSet<IMyEntity> possibleAttackingPlayers2 = new HashSet<IMyEntity>();
                //MyAPIGateway.Entities.GetEntities(possibleAttackingPlayers2, entity => entity is IMyShipController);
                //AiSessionCore.DebugLog?.WriteToLog("ReactOnDamage", $"EntListSize:\t{possibleAttackingPlayers2.Count}");

                //foreach (IMyPlayer possibleAttackingPlayer in possibleAttackingPlayers)
                //	AiSessionCore.DebugLog?.WriteToLog("ReactOnDamage", $"player:\t{possibleAttackingPlayer?.IdentityId}\t{possibleAttackingPlayer?.DisplayName}");

                //foreach (IMyEntity possibleAttackingPlayer in possibleAttackingPlayers2)
                //	AiSessionCore.DebugLog?.WriteToLog("ReactOnDamage", $"Entity:\t{possibleAttackingPlayer?.EntityId}\t{possibleAttackingPlayer?.DisplayName}");

                //AiSessionCore.DebugLog?.WriteToLog("ReactOnDamage", $"damage:\t{damage}");
                //AiSessionCore.DebugLog?.WriteToLog("ReactOnDamage", $"damage.Amount:\t{damage.Amount}");
                //AiSessionCore.DebugLog?.WriteToLog("ReactOnDamage", $"damage.AttackerId:\t{damage.AttackerId}");
                //AiSessionCore.DebugLog?.WriteToLog("ReactOnDamage", $"damage.Type:\t{damage.Type}");
                //AiSessionCore.DebugLog?.WriteToLog("ReactOnDamage", $"damager.IsNull:\t{damager == null}");
                //AiSessionCore.DebugLog?.WriteToLog("ReactOnDamage", $"damager.GetFaction()\t{damager?.GetFaction()}");
                //AiSessionCore.DebugLog?.WriteToLog("ReactOnDamage", $"damager.GetFactionIsNull()\t{damager?.GetFaction() == null}");
            }
            catch (Exception scrap)
            {
                WriteGeneral("ReactOnDamage", scrap.Message);
            }
        }

        // Get pilot of a ship
        // var playerEnt = MyAPIGateway.Session.ControlledObject?.Entity as MyEntity;
        // if (playerEnt?.Parent != null) playerEnt = playerEnt.Parent;

        protected void BlockPlacedHandler(IMySlimBlock block)
        {
            if (block == null) return;

            try
            {
                IMyPlayer builder;
                if (!block.IsPlayerBlock(out builder)) return;
                IMyFaction faction = builder.GetFaction();
                if (faction != null)
                    DeclareWar(faction);
            }
            catch (Exception e)
            {
                WriteGeneral("BlockPlacedHandler", $"{e}");
            }
        }

        private void DeclareWar(IMyFaction playerFaction)
        {
            //if (MyAPIGateway.Session.IsServer) return;
            try
            {
                if (playerFaction == null) return;
                if (_ownerFaction == null)
                    _ownerFaction = Grid.GetOwnerFaction();
                TriggerWar(_ownerFaction.FactionId, playerFaction.FactionId);
            }
            catch (Exception e)
            {
                WriteGeneral("DeclareWar", $"Exception!\t{e}");
            }
        }


        //protected virtual void RegisterHostileAction(IMyPlayer player, TimeSpan truceDelay)
        //{
        //	try
        //	{
        //		#region Sanity checks
        //		if (player == null)
        //		{
        //			Grid.DebugWrite("RegisterHostileAction", "Error: Damager is null.");
        //			return;
        //		}

        //		if (_ownerFaction == null)
        //		{
        //			_ownerFaction = Grid.GetOwnerFaction();
        //		}

        //		if (_ownerFaction == null || !_ownerFaction.IsNpc())
        //		{
        //			Grid.DebugWrite("RegisterHostileAction", $"Error: {(_ownerFaction == null ? "can't find own faction" : "own faction isn't recognized as NPC.")}");
        //			return;
        //		}
        //		#endregion

        //		IMyFaction hostileFaction = player.GetFaction();
        //		if (hostileFaction == null)
        //		{
        //			Grid.DebugWrite("RegisterHostileAction", "Error: can't find damager's faction");
        //			return;
        //		}

        //		if (hostileFaction == _ownerFaction)
        //		{
        //			_ownerFaction.Kick(player);
        //			return;
        //		}

        //		//AiSessionCore.WarDeclared = 
        //		AiSessionCore.DeclareWar(_ownerFaction, hostileFaction, truceDelay);
        //		//if (!_ownerFaction.IsLawful()) return;
        //		//AiSessionCore.DeclareWar(Diplomacy.Police, hostileFaction, truceDelay);
        //		//AiSessionCore.DeclareWar(Diplomacy.Army, hostileFaction, truceDelay);
        //	}
        //	catch (Exception scrap)
        //	{
        //		LogError("RegisterHostileAction", scrap);
        //	}
        //}

        //protected virtual void RegisterHostileAction(IMyFaction hostileFaction, TimeSpan truceDelay)
        //{
        //	try
        //	{
        //		if (hostileFaction != null)
        //		{
        //			//AiSessionCore.WarDeclared = 
        //			AiSessionCore.DeclareWar(_ownerFaction, hostileFaction, truceDelay);
        //			//if (!_ownerFaction.IsLawful()) return;
        //			//AiSessionCore.DeclareWar(Diplomacy.Police, hostileFaction, truceDelay);
        //			//AiSessionCore.DeclareWar(Diplomacy.Army, hostileFaction, truceDelay);
        //		}
        //		else
        //		{
        //			Grid.DebugWrite("RegisterHostileAction", "Error: can't find damager's faction");
        //		}
        //	}
        //	catch (Exception scrap)
        //	{
        //		LogError("RegisterHostileAction", scrap);
        //	}
        //}

        //TODO Figure out why there is a NULL REFERENCE EXCEPTION from this call on velocity from MyDetectedEntityInfo
        //	velocity = myCubeGrid.Physics.LinearVelocity; +		$exception	{System.NullReferenceException: Object reference not set to an instance of an object.
        //		at Sandbox.Game.Entities.MyDetectedEntityInfoHelper.Create(MyEntity entity, Int64 sensorOwner, Nullable`1 hitPosition)}
        //		System.NullReferenceException

        protected List<MyDetectedEntityInfo> LookAround(float radius, Func<MyDetectedEntityInfo, bool> filter = null)
        {
            var radarData = new List<MyDetectedEntityInfo>();
            var lookaroundSphere = new BoundingSphereD(GridPosition, radius);

            List<IMyEntity> entitiesAround = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref lookaroundSphere);
            entitiesAround.RemoveAll(x => x == Grid || GridPosition.DistanceTo(x.GetPosition()) < GridRadius * 1.5);

            long ownerId;
            if (_ownerFaction != null)
            {
                ownerId = _ownerFaction.FounderId;
                //WriteGeneral("LookAround", "Found owner via faction owner");
            }
            else
            {
                ownerId = Rc.OwnerId;
                WriteGeneral("LookAround", "OWNER FACTION NOT FOUND, found owner via RC owner");
            }

            foreach (IMyEntity detectedEntity in entitiesAround)
            {
                if (detectedEntity is IMyFloatingObject || detectedEntity.Physics == null) continue;
                MyDetectedEntityInfo radarDetectedEntity = MyDetectedEntityInfoHelper.Create(detectedEntity as MyEntity, ownerId);
                if (radarDetectedEntity.Type == MyDetectedEntityType.None || radarDetectedEntity.Type == MyDetectedEntityType.Unknown) continue;
                if (filter == null || filter(radarDetectedEntity)) radarData.Add(radarDetectedEntity);
            }

            //DebugWrite("LookAround", $"Radar entities detected: {String.Join(" | ", RadarData.Select(x => $"{x.Name}"))}");
            return radarData;
        }

        protected List<MyDetectedEntityInfo> LookForEnemies(float radius, bool considerNeutralsAsHostiles = false, Func<MyDetectedEntityInfo, bool> filter = null)
        {
            return !considerNeutralsAsHostiles ? LookAround(radius, x => x.IsHostile() && (filter == null || filter(x))) : LookAround(radius, x => x.IsNonFriendly() && (filter == null || filter(x)));
        }

        /// <summary>
        ///     Returns distance from the grid to an object.
        /// </summary>
        protected float Distance(MyDetectedEntityInfo target)
        {
            return (float)Vector3D.Distance(GridPosition, target.Position);
        }

        /// <summary>
        ///     Returns distance from the grid to an object.
        /// </summary>
        //protected float Distance(IMyEntity target)
        //{
        //	return (float)Vector3D.Distance(GridPosition, target.GetPosition());
        //}

        //protected Vector3 RelVelocity(MyDetectedEntityInfo target)
        //{
        //	return target.Velocity - GridVelocity;
        //}
        protected float RelSpeed(MyDetectedEntityInfo target)
        {
            return (float)(target.Velocity - GridVelocity).Length();
        }

        //protected Vector3 RelVelocity(IMyEntity target)
        //{
        //	return target.Physics.LinearVelocity - GridVelocity;
        //}

        //protected float RelSpeed(IMyEntity target)
        //{
        //	return (float)(target.Physics.LinearVelocity - GridVelocity).Length();
        //}

        //protected virtual List<IMyTerminalBlock> GetHackedBlocks()
        //{
        //	List<IMyTerminalBlock> terminalBlocks = new List<IMyTerminalBlock>();
        //	List<IMyTerminalBlock> hackedBlocks = new List<IMyTerminalBlock>();

        //	Term.GetBlocks(terminalBlocks);

        //	foreach (IMyTerminalBlock block in terminalBlocks)
        //		if (block.IsBeingHacked) hackedBlocks.Add(block);

        //	return hackedBlocks;
        //}

        //protected virtual List<IMySlimBlock> GetDamagedBlocks()
        //{
        //	List<IMySlimBlock> blocks = new List<IMySlimBlock>();
        //	Grid.GetBlocks(blocks, x => x.CurrentDamage > 10);
        //	return blocks;
        //}


        protected void ApplyThrustMultiplier(float thrustMultiplier)
        {
            DeMultiplyThrusters();
            foreach (IMyThrust thruster in Term.GetBlocksOfType<IMyThrust>(x => x.IsOwnedByNpc(false, true)))
            {
                thruster.ThrustMultiplier = thrustMultiplier;
                thruster.OwnershipChanged += Thruster_OnOwnerChanged;
                SpeedModdedThrusters.Add(thruster);
            }
        }

        protected void DeMultiplyThrusters()
        {
            if (!HasModdedThrusters) return;
            foreach (IMyThrust thruster in SpeedModdedThrusters)
                if (Math.Abs(thruster.ThrustMultiplier - 1) > 0)
                    thruster.ThrustMultiplier = 1;
            SpeedModdedThrusters.Clear();
        }

        private void Thruster_OnOwnerChanged(IMyTerminalBlock thruster)
        {
            try
            {
                var myThruster = thruster as IMyThrust;
                if (myThruster == null) return;
                if (!myThruster.IsOwnedByNpc() && Math.Abs(myThruster.ThrustMultiplier - 1) > 0) myThruster.ThrustMultiplier = 1;
            }
            catch (Exception scrap)
            {
                WriteGeneral("Thruster_OnOwnerChanged", $"{thruster.CustomName} OnOwnerChanged failed: {scrap.Message}");
            }
        }

        protected abstract void ParseSetup();

        public abstract void Main();

        public virtual void Shutdown()
        {
            Closed = true;
            if (HasModdedThrusters) DeMultiplyThrusters();
            //_botDamageHandler.RemoveDamageHandler(Grid);
            Close();
        }
    }
}