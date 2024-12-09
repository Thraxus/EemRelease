using System;
using System.Collections.Generic;
using Eem.Thraxus.Debug;
using Eem.Thraxus.Helpers;
using Eem.Thraxus.Networking;
using Eem.Thraxus.Utilities;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.ComponentSystem;
using VRage.ModAPI;
using VRage.Utils;

namespace Eem.Thraxus
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
	// ReSharper disable once ClassNeverInstantiated.Global
	public class AiSessionCore : MySessionComponentBase
    {
		/* TODO
		 
		1) Create new Faction Types in FactionTypes.sbc for EEM specifically
		2) Decide how the prefabs should be distributed to this new system
		3) Spawn all prefabs in specific saves per category type (i.e. all traders in one save, cargos in another, encounters in another?)
		4) Remove AI init that sets factions from all RCs 
            - Can keep behavior or other items as necessary? 
		5) Update EEM to use the Common framework from my other mods
		6) Make bots not suck as a simple first pass, and divorce them from "AiSessionCore"
            - NO FANCY SHIT THIS TIME AROUND ASSHOLE!!!
            - Pull them off GameLogic of the RC into Session Logic
            - Simplify the setup routine
            - Fix the awful threat detection
		7) Rename "AiSessionCore" to "EemCore"
		8) Stop Factions from caring about any faction but EEM for protections
		9) See if you can remove all reliance on the Client Code
            - Code change done, but still should look deeper into Factions and keeping all stuff to EEM only

		*/


        public const bool DisableAi = false;

		private static readonly Dictionary<long, BotBase.OnDamageTaken> DamageHandlers = new Dictionary<long, BotBase.OnDamageTaken>();

		public static void AddDamageHandler(long gridId, BotBase.OnDamageTaken handler)
		{
			DamageHandlers.Add(gridId, handler);
		}

		public static void AddDamageHandler(IMyCubeGrid grid, BotBase.OnDamageTaken handler)
		{
			AddDamageHandler(grid.GetTopMostParent().EntityId, handler);
		}

		public static void RemoveDamageHandler(long gridId)
		{
			DamageHandlers.Remove(gridId);
		}

		public static void RemoveDamageHandler(IMyCubeGrid grid)
		{
			RemoveDamageHandler(grid.GetTopMostParent().EntityId);
		}

		public static bool HasDamageHandler(long gridId)
		{
			return DamageHandlers.ContainsKey(gridId);
		}

		public static bool HasDamageHandler(IMyCubeGrid grid)
		{
			return HasDamageHandler(grid.GetTopMostParent().EntityId);
		}

		public static bool LogSetupComplete;
		//public static Log ProfilingLog;
		//public static Log DebugLog;
		public static Log GeneralLog;

		private bool _initialized;
		private bool _debugInitialized;

		public override void UpdateBeforeSimulation()
		{
			MyAPIGateway.Utilities.InvokeOnGameThread(() => SetUpdateOrder(MyUpdateOrder.NoUpdate));
			if (!Constants.IsServer) return;
			if (Constants.DebugMode && !_debugInitialized) DebugInit();
			if (!_initialized) Initialize();
		}

		private void DebugInit()
		{
			_debugInitialized = true;
			//DebugLog = new Log(Constants.DebugLogName);
			InformationExporter.Run();
			MyAPIGateway.Entities.OnEntityAdd += delegate (IMyEntity entity)
			{
				GeneralLog.WriteToLog("Core", $"Entity Added\t{entity.EntityId}\t{entity.DisplayName}");
			};
			MyAPIGateway.Entities.OnEntityRemove += delegate (IMyEntity entity)
			{
				GeneralLog.WriteToLog("Core", $"Entity Removed\t{entity.EntityId}\t{entity.DisplayName}");
			};
		}

		private void Initialize()
		{
            _initialized = true;
            if (DisableAi)
            { return; }

            //if (Constants.DebugMode) DebugLog.WriteToLog("Initialize", $"Debug Active - IsServer: {Constants.IsServer}", true, 20000);
            Messaging.Register();
			MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, DamageRefHandler);
			MyAPIGateway.Session.DamageSystem.RegisterAfterDamageHandler(0, GenericDamageHandler);
			MyAPIGateway.Session.DamageSystem.RegisterDestroyHandler(0, GenericDamageHandler);
			
		}

        private static void InitLogs()
        {
            //if (Constants.EnableProfilingLog) ProfilingLog = new Log(Constants.ProfilingLogName);
            if (Constants.EnableGeneralLog) GeneralLog = new Log(Constants.GeneralLogName);
            LogSetupComplete = true;
            
            GeneralLog.WriteToLog("LateSetup", $"Cargo: {MyAPIGateway.Session.SessionSettings.CargoShipsEnabled}");
            GeneralLog.WriteToLog("LateSetup", $"EnableEncounters: {MyAPIGateway.Session.SessionSettings.EnableEncounters}");
            GeneralLog.WriteToLog("LateSetup", $"EncounterDensity: {MyAPIGateway.Session.SessionSettings.EncounterDensity}");
            GeneralLog.WriteToLog("LateSetup", $"EncounterGeneratorVersion: {MyAPIGateway.Session.SessionSettings.EncounterGeneratorVersion}");
            GeneralLog.WriteToLog("LateSetup", $"GlobalEncounterCap: {MyAPIGateway.Session.SessionSettings.GlobalEncounterCap}");
            GeneralLog.WriteToLog("LateSetup", $"GlobalEncounterEnableRemovalTimer: {MyAPIGateway.Session.SessionSettings.GlobalEncounterEnableRemovalTimer}");
            GeneralLog.WriteToLog("LateSetup", $"GlobalEncounterMaxRemovalTimer: {MyAPIGateway.Session.SessionSettings.GlobalEncounterMaxRemovalTimer}");
            GeneralLog.WriteToLog("LateSetup", $"GlobalEncounterMinRemovalTimer: {MyAPIGateway.Session.SessionSettings.GlobalEncounterMinRemovalTimer}");
            GeneralLog.WriteToLog("LateSetup", $"GlobalEncounterRemovalTimeClock: {MyAPIGateway.Session.SessionSettings.GlobalEncounterRemovalTimeClock}");
            GeneralLog.WriteToLog("LateSetup", $"GlobalEncounterTimer: {MyAPIGateway.Session.SessionSettings.GlobalEncounterTimer}");
            GeneralLog.WriteToLog("LateSetup", $"PlanetaryEncounterDesiredSpawnRange: {MyAPIGateway.Session.SessionSettings.PlanetaryEncounterDesiredSpawnRange}");
            GeneralLog.WriteToLog("LateSetup", $"Drones: {MyAPIGateway.Session.SessionSettings.EnableDrones}");
            GeneralLog.WriteToLog("LateSetup", $"Scripts: {MyAPIGateway.Session.SessionSettings.EnableIngameScripts}");
            GeneralLog.WriteToLog("LateSetup", $"Sync: {MyAPIGateway.Session.SessionSettings.SyncDistance}");
            GeneralLog.WriteToLog("LateSetup", $"View: {MyAPIGateway.Session.SessionSettings.ViewDistance}");
            GeneralLog.WriteToLog("LateSetup", $"BlockLimitsEnabled: {MyAPIGateway.Session.SessionSettings.BlockLimitsEnabled}");
            GeneralLog.WriteToLog("LateSetup", $"Global Encounter PCU: {MyAPIGateway.Session.SessionSettings.GlobalEncounterPCU}");
            GeneralLog.WriteToLog("LateSetup", $"Pirate PCU: {MyAPIGateway.Session.SessionSettings.PiratePCU}");
            GeneralLog.WriteToLog("LateSetup", $"Total PCU: {MyAPIGateway.Session.SessionSettings.TotalPCU}");

            foreach (MyObjectBuilder_Checkpoint.ModItem mod in MyAPIGateway.Session.Mods)
                GeneralLog.WriteToLog("LateSetup", $"Mod: {mod}");
            List<IMyIdentity> identityList = new List<IMyIdentity>();
            MyAPIGateway.Players.GetAllIdentites(identityList);
            foreach (IMyIdentity identity in identityList)
                GeneralLog.WriteToLog("LateSetup", $"Identity: {identity.IdentityId} | {identity.DisplayName} | {identity.IsDead} | {MyAPIGateway.Players.TryGetSteamId(identity.IdentityId)}");
        }

        private static void CloseLogs()
        {
            if (Constants.EnableGeneralLog) GeneralLog?.Close();
        }

        public override void LoadData()
        {
            base.LoadData();
            const int eemPcuLimit = 500000;
            MyAPIGateway.Session.SessionSettings.EnableIngameScripts = true;
            MyAPIGateway.Session.SessionSettings.EnableDrones = true;
            if (MyAPIGateway.Session.SessionSettings.PiratePCU < eemPcuLimit)
            {
                MyAPIGateway.Session.SessionSettings.PiratePCU = eemPcuLimit;
            }
            if (MyAPIGateway.Session.SessionSettings.GlobalEncounterPCU < eemPcuLimit)
            {
                MyAPIGateway.Session.SessionSettings.GlobalEncounterPCU = eemPcuLimit;
            }
            MyAPIGateway.Session.SessionSettings.EncounterDensity = 0.65f;
            MyAPIGateway.Session.SessionSettings.GlobalEncounterTimer = 10;
        }


        ///// <summary>
        ///// Initial setup
        ///// </summary>
        ///// <param name="sessionComponent"></param>
        //public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        //{
        //	base.Init(sessionComponent);
        //}

        public override void BeforeStart()
		{
			InitLogs();
		}

		/// <summary>
		/// Unloads the handlers
		/// </summary>
		protected override void UnloadData()
		{
			base.UnloadData();
			Messaging.Unregister();
			CloseLogs();
		}

		public void DamageRefHandler(object damagedObject, ref MyDamageInformation damage)
		{
			GenericDamageHandler(damagedObject, damage);
		}

		public void GenericDamageHandler(object damagedObject, MyDamageInformation damage)
		{
			try
			{
				if (damage.AttackerId == 0 || !(damagedObject is IMySlimBlock)) return;
				IMySlimBlock damagedBlock = (IMySlimBlock)damagedObject;
				IMyCubeGrid damagedGrid = damagedBlock.CubeGrid;
				long gridId = damagedGrid.GetTopMostParent().EntityId;
				if (!DamageHandlers.ContainsKey(gridId)) return;
				DamageHandlers[gridId].Invoke(damagedBlock, damage);
			}
			catch (Exception scrap)
			{
				LogError("GenericDamageHandler", scrap);
			}
		}

		public static void LogError(string source, Exception scrap, string debugPrefix = "SessionCore.")
		{
			MyLog.Default.WriteLine($"Core Crash - {scrap.StackTrace}");
			DebugHelper.Print("Core Crash - Please reload the game", $"Fatal error in '{debugPrefix + source}': {scrap.Message}. {(scrap.InnerException != null ? scrap.InnerException.Message : "No additional info was given by the game :(")}");
		}

		public static void DebugWrite(string source, string message, bool antiSpam = true)
		{
			if (Constants.DebugMode) DebugHelper.Print($"{source}", $"{message}", antiSpam);
		}
	}
}