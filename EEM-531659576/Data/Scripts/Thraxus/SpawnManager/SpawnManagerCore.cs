using System.Collections.Generic;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.Enums;
using Sandbox.ModAPI;
using Sandbox.Game.Entities;
using VRage.Game.Components;
using VRage.ModAPI;
using VRageMath;

namespace Eem.Thraxus.SpawnManager
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, priority: int.MinValue + 1)]
	public class SpawnManagerCore : BaseSessionComp
	{
		// Constants

		protected override string CompName { get; } = "SpawnManagerCore";
		protected override CompType Type { get; } = CompType.Server;
		protected override MyUpdateOrder Schedule { get; } = MyUpdateOrder.NoUpdate;

		// Fields
		
		protected override void SuperEarlySetup()
		{
			base.SuperEarlySetup();
			MyAPIGateway.Entities.OnEntityAdd += OnEntityAdd;
			MyAPIGateway.Entities.OnEntityRemove += OnEntityRemoved;
		}

		protected override void Unload()
		{
			base.Unload();
			MyAPIGateway.Entities.OnEntityAdd -= OnEntityAdd;
			MyAPIGateway.Entities.OnEntityRemove -= OnEntityRemoved;
		}

		private void OnEntityAdd(IMyEntity myEntity)
		{
			if (myEntity.GetType() != typeof(MyCubeGrid)) return;
			MyCubeGrid grid = (MyCubeGrid)myEntity;
			WriteToLog("OnEntityAdd", $"Id: {grid.EntityId} | Name: {grid.DisplayName} | Size: {grid.GridSizeEnum} | Blocks: {grid.BlocksCount} | PCU: {grid.BlocksPCU}", LogType.General);
			AttemptStationFix(myEntity);
		}

		private void OnEntityRemoved(IMyEntity myEntity)
		{
			if (myEntity.GetType() != typeof(MyCubeGrid)) return;
			WriteToLog("OnEntityRemoved", $"Id: {myEntity.EntityId} | Name: {myEntity.DisplayName}", LogType.General);
		}

		private static void AttemptStationFix(IMyEntity entity)
		{
			MyCubeGrid thisCubeGrid = (MyCubeGrid)entity;
			if (!EemStations.Contains(thisCubeGrid.DisplayName)) return;
			if (thisCubeGrid.Physics == null || thisCubeGrid.IsStatic) return;
			thisCubeGrid.Physics.LinearVelocity = Vector3.Zero;
			thisCubeGrid.Physics.AngularVelocity = Vector3.Zero;
			thisCubeGrid.ConvertToStatic();
		}

		private static readonly List<string> EemStations = new List<string>
		{
			"Amphion Diatom Waystation",
			"Hydrozoa Waystation",
			"Amphion Polyp Small Outpost",
			"Amphion Reef Barrier Station",
			"Amphion Rotifer Waystation",
			"Station Debris",
			"Dead_Station_2_signal",
			"Encounter Droneyard",
			"Encounter Haunted Section",
			"HEC Debris",
			"Station Defence",
			"Large Grid 4740",
			"Encounter MushStation",
			"Encounter RoidStation",
			"Encounter Stealth pirate station",
			"Hi-Tech Factory",
			"IMDC Defense Platform",
			"KUS Waystation",
			"Small Solar Recharge Station",
			"Object Defense Platform Echo",
			"IMDC 1781 Service Platform",
			"Mahriane 34 Trading Outpost",
			"Mahriane 56 Trading Outpost",
			"Mahriane 8724 Service Platform",
			"XMC 603 Factory",
			"XMC 718 Trading Outpost",
			"XMC 99 Refinery",
			"Trade Route Beacon",
			"Phaeton Trading Outpost",
			"Police Outpost",
			"Raiding Outpost mk.1",
			"Raiding Station - Scourge",
			"Ð‘Ð¾Ð»ÑŒÑˆÐ¾Ð¹ ÐºÐ¾Ñ€Ð°Ð±Ð»ÑŒ 3106",
			"Refueling Station XL Beta",
			"Salvaging Outpost",
			"Navigational Beacon",
			"XMC 4402 Captured",
			"XMC 521 Trade Center_Destroyed",
			"Mining Station Cirva"
		};
	}
}
