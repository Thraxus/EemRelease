using Eem.Thraxus.Common;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.Enums;
using Eem.Thraxus.Common.Utilities;
using Eem.Thraxus.Factions.Models;
using Eem.Thraxus.Common.Utilities.FileHandlers;
using Eem.Thraxus.Factions.DataTypes;
using VRage.Game;
using VRage.Game.Components;


namespace Eem.Thraxus.Factions
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
	// ReSharper disable once ClassNeverInstantiated.Global
	public class FactionCore : BaseSessionComp
	{
		protected override string CompName { get; } = "FactionCore";
		protected override CompType Type { get; } = CompType.Server;
		protected override MyUpdateOrder Schedule { get; } = MyUpdateOrder.BeforeSimulation;

		private const string SaveFileName = "Factions.eem";
		
		// Events

		public void RegisterWar(PendingWar pendingWar)
		{
			_relationshipManager.DeclareWar(pendingWar);
			WriteToLog("RegisterWar", $"New war! {pendingWar}", LogType.General);
		}

		// Fields

		private RelationshipManager _relationshipManager;

		public static FactionCore FactionCoreStaticInstance;

		/// <inheritdoc />
		public override void LoadData()
		{
			base.LoadData();
			_relationshipManager = new RelationshipManager(Load.ReadFromBinaryFile<SaveData>(SaveFileName, typeof(SaveData)));
			FactionCoreStaticInstance = this;
		}
		
		// Init Methods

		/// <summary>
		/// Runs every tick before the simulation is updated
		/// </summary>
		public override void UpdateBeforeSimulation()
		{
			base.UpdateBeforeSimulation();
			TickTimer();
		}

		public override MyObjectBuilder_SessionComponent GetObjectBuilder()
		{   // Always return base.GetObjectBuilder() after your code! 
			if (_relationshipManager == null) base.GetObjectBuilder();
			Save.WriteToFile(SaveFileName, _relationshipManager.GetSaveData(), typeof(SaveData));
			WriteToLog("GetObjectBuilder", $"Saved state", LogType.General);
			return base.GetObjectBuilder();
		}

		public override void SaveData()
		{	// Don't save here, save in GetObjectBuilder() -- this fires before the actual save
			base.SaveData();
		}
		
		/// <inheritdoc />
		protected override void LateSetup()
		{
			base.LateSetup();
			_relationshipManager.OnWriteToLog += WriteToLog;
			_relationshipManager.Run();
			//MyAPIGateway.Utilities.InvokeOnGameThread(() => SetUpdateOrder(MyUpdateOrder.NoUpdate));
			WriteToLog("FactionCore", $"Initialized... {UpdateOrder}", LogType.General);
		}

		/// <inheritdoc />
		protected override void Unload()
		{
			if (_relationshipManager != null)
			{
				_relationshipManager.OnWriteToLog -= WriteToLog;
				_relationshipManager.Close();
			}
			FactionCoreStaticInstance = null;
			base.Unload();
		}

		// Core Logic Methods

		/// <summary>
		/// Increments every server tick
		/// </summary>
		private ulong _tickTimer;

		/// <summary>
		/// Processes certain things at set intervals
		/// </summary>
		private void TickTimer()
		{
			_tickTimer++;
			if (_tickTimer % CommonSettings.TicksPerMinute == 0)
				_relationshipManager.ReputationDecay();
		}
	}
}
