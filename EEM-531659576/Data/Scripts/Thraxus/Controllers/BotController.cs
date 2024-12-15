using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.Extensions;
using Eem.Thraxus.Entities.Bots;
using Eem.Thraxus.Models;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Eem.Thraxus.Controllers
{
    internal class BotController : BaseLoggingClass
    {
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

        private readonly List<BotAi> _botAis = new List<BotAi>();
        private readonly BotDamageHandler _botDamageHandler;
        public readonly ActionQueues ActionQueues;


        public BotController(ActionQueues actionQueues, BotDamageHandler botDamageHandler)
        {
            ActionQueues = actionQueues;
            _botDamageHandler = botDamageHandler;
        }

        public void Init()
        {
            MyAPIGateway.Entities.OnEntityAdd += OnEntityAdd;
            MyAPIGateway.Entities.OnEntityRemove += OnEntityRemoved;
        }

        public override void Close()
        {
            MyAPIGateway.Entities.OnEntityAdd -= OnEntityAdd;
            MyAPIGateway.Entities.OnEntityRemove -= OnEntityRemoved;
            base.Close();
        }

        public void Update10()
        {
            foreach (BotAi botAi in _botAis) botAi.Update10();
        }

        private void OnEntityAdd(IMyEntity myEntity)
        {
            if (myEntity.GetType() != typeof(MyCubeGrid)) return;
            var grid = (MyCubeGrid)myEntity;
            if (!grid.IsNpcSpawnedGrid) return;
            if (grid.IsPreview || grid.IsGenerated || grid.Physics == null) return;

            WriteGeneral("OnEntityAdd", $"Id: {grid.EntityId} | Name: {grid.DisplayName} | Size: {grid.GridSizeEnum} | Blocks: {grid.BlocksCount} | PCU: {grid.BlocksPCU} | Npc Spawned: {grid.IsNpcSpawnedGrid.ToSingleChar()}");
            AttemptStationFix(myEntity);

            IMyCubeGrid iMyGrid = grid;

            IMyRemoteControl rc = iMyGrid.GetFatBlocks<IMyRemoteControl>().FirstOrDefault(x => x.CustomData.Contains("[EEM_AI]"));

            if (rc == null) return;
            if (rc.CustomData.Contains("Type:None") || string.IsNullOrEmpty(rc.CustomData)) return;
            var config = new BotConfig(rc.CustomData);

            // add ownership check maybe, or leave it and fuck a player for cheating in the non-buildable remote block, lol
            WriteGeneral("OnEntityAdd", "New Bot AI Initializing...");
            var newBot = new BotAi(rc, config, _botDamageHandler);
            newBot.OnWriteToLog += WriteGeneral;
            newBot.OnClose += close =>
            {
                WriteGeneral("BotClose", "Closing Bot");
                OnWriteToLog -= WriteGeneral;
                _botAis.Remove(newBot);
            };
            newBot.Init();
            _botAis.Add(newBot);
            WriteGeneral("OnEntityAdd", "New Bot AI Initialized!");
        }

        private void OnEntityRemoved(IMyEntity myEntity)
        {
            if (myEntity.GetType() != typeof(MyCubeGrid)) return;
            WriteGeneral("OnEntityRemoved", $"Id: {myEntity.EntityId} | Name: {myEntity.DisplayName}");
        }

        private void AttemptStationFix(IMyEntity entity)
        {
            var thisCubeGrid = (MyCubeGrid)entity;
            if (!EemStations.Contains(thisCubeGrid.DisplayName)) return;
            if (thisCubeGrid.Physics == null || thisCubeGrid.IsStatic) return;
            thisCubeGrid.Physics.LinearVelocity = Vector3.Zero;
            thisCubeGrid.Physics.AngularVelocity = Vector3.Zero;
            thisCubeGrid.ConvertToStatic();
        }
    }
}