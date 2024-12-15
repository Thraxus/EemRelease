using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.Extensions;
using Eem.Thraxus.Enums;
using Eem.Thraxus.Models;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using static VRage.Game.MyObjectBuilder_ControllerSchemaDefinition;

namespace Eem.Thraxus.Entities.Bots
{
    public class BotAi : BaseLoggingClass
    {
        private readonly BotDamageHandler _botDamageHandler;
        private readonly BotConfig _botConfig;

        public BotAi(IMyRemoteControl remote, BotConfig botConfig,  BotDamageHandler botDamageHandler)
        {
            _botDamageHandler = botDamageHandler;
            Rc = remote;
            Grid = Rc.CubeGrid.GetTopMostParent() as IMyCubeGrid;
            _botConfig = botConfig;
        }

        private IMyRemoteControl Rc { get; }

        public IMyCubeGrid Grid { get; }

        private BotBase Ai { get; set; }

        private long BotId;

        public void Init()
        {
            Ai = FabricateBot(Grid, Rc);

            if (Ai == null)
            {
                Close();
                return;
            }

            BotId = Rc.EntityId;
            
            Ai.OnWriteToLog += WriteGeneral;
            Ai.OnClose += close =>
            {
                WriteGeneral("Signing Off", $"[{BotId.ToEntityIdFormat()}]");
                Grid.Close();
                Close();
            };
            Ai.Init(Rc);
        }

        public void Update10()
        {
            Ai.Main();
        }

        public override void Close()
        {
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
                    bot = new BotTypeFighter(grid, _botConfig, _botDamageHandler);
                    break;
                case BotType.Freighter:
                    WriteGeneral("FabricateBot", "New Bot: Freighter");
                    bot = new BotTypeFreighter(grid, _botConfig, _botDamageHandler);
                    break;
                case BotType.Station:
                    WriteGeneral("FabricateBot", "New Bot: Station");
                    bot = new BotTypeStation(grid, _botConfig, _botDamageHandler);
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