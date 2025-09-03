using System;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.Extensions;
using Eem.Thraxus.Controllers;
using Eem.Thraxus.Enums;
using Eem.Thraxus.Extensions;
using Eem.Thraxus.Models;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Entities.Bots
{
    public class BotAi : BaseLoggingClass
    {
        private readonly BotConfig _botConfig;
        private readonly CoordinationController _coordinationController;

        public BotAi(IMyRemoteControl remote, BotConfig botConfig, CoordinationController coordinationController)
        {
            Rc = remote;
            Grid = Rc.CubeGrid.GetTopMostParent() as IMyCubeGrid;
            _botConfig = botConfig;
            _coordinationController = coordinationController;
        }

        public event Action<long, long> TriggerWar;

        private IMyRemoteControl Rc { get; }

        public IMyCubeGrid Grid { get; }

        private BotBase Ai { get; set; }

        private long BotId;

        public void Init()
        {
            WriteGeneral("Init", "Fabricating New Bot...");
            Ai = FabricateBot(Grid, Rc);

            if (Ai == null)
            {
                Close();
                return;
            }

            BotId = Rc.EntityId;
            WriteGeneral("Init", $"Bot Id: [{BotId.ToEntityIdFormat()}]");

            Ai.OnWriteToLog += WriteGeneral;
            Ai.TriggerWar += (assaulted, assaulter) => _coordinationController.FactionController.TriggerWar(assaulted, assaulter);

            _coordinationController.DamageController.AlertReporting.Add(Rc.GetTopMostParent().EntityId, Ai.TriggerAlert);

            Ai.OnClose += close =>
            {
                _coordinationController.DamageController.AlertReporting.Remove(Rc.GetTopMostParent().EntityId);
                WriteGeneral("Signing Off", $"[{BotId.ToEntityIdFormat()}]");
                Close();
                //Grid.Close();
            };
            WriteGeneral("Init", $"Initializing Ai for: [{BotId.ToEntityIdFormat()}]");
            Ai.Init(Rc);
            Rc.IsMainCockpit = true;
            _coordinationController.ActionQueues.AfterSimActionQueue.Add(1, EnsureOwnership);
        }

        private void EnsureOwnership()
        {
            IMyFaction properOwner = _botConfig.Faction.FactionTypeToFaction();
            if (((MyCubeGrid)Grid.GetTopMostParent()).GetOwnerFaction().Tag == properOwner.Tag) return;
            WriteGeneral("Init", $"Trigger Faction Change: [{((MyCubeGrid)Grid.GetTopMostParent()).GetOwnerFaction().Tag}] [{properOwner.Tag}]");
            ((MyCubeGrid)Grid.GetTopMostParent()).ChangeGridOwner(properOwner.FounderId, MyOwnershipShareModeEnum.Faction);
            ((MyCubeGrid)Grid.GetTopMostParent()).ChangeGridOwnership(properOwner.FounderId, MyOwnershipShareModeEnum.Faction);
        }

        public void Update10()
        {
            Ai.Main();
        }

        public override void Close()
        {
            Ai.Close();
            Ai.TriggerWar -= TriggerWar;
            Ai.OnWriteToLog -= WriteGeneral;
            base.Close();
        }

        public BotBase FabricateBot(IMyCubeGrid grid, IMyRemoteControl rc)
        {
            BotBase bot = null;
            switch (_botConfig.BotType)
            {
                case BotType.Fighter:
                    WriteGeneral("FabricateBot", "New Bot: Fighter");
                    //bot = new BotTypeFighter(grid, _botConfig, _botDamageHandler);
                    bot = new BotTypeFighter(grid, _botConfig);
                    break;
                case BotType.Freighter:
                    WriteGeneral("FabricateBot", "New Bot: Freighter");
                    bot = new BotTypeFreighter(grid, _botConfig);
                    break;
                case BotType.Station:
                    WriteGeneral("FabricateBot", "New Bot: Station");
                    bot = new BotTypeStation(grid, _botConfig);
                    break;
                case BotType.None:
                case BotType.Invalid:
                case BotType.Carrier:
                    break;
                default:
                    WriteGeneral("FabricateBot", "Invalid bot type");
                    break;
            }

            return bot;
        }
    }
}