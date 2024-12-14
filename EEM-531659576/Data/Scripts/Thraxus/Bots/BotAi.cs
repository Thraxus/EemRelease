using Eem.Thraxus.Common.BaseClasses;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Bots
{
    public class BotAi : BaseLoggingClass
    {
        private readonly BotDamageHandler _botDamageHandler;

        public BotAi(MyCubeBlock remote, BotDamageHandler botDamageHandler)
        {
            _botDamageHandler = botDamageHandler;
            Rc = remote as IMyRemoteControl;
            Grid = Rc?.CubeGrid.GetTopMostParent() as IMyCubeGrid;
        }

        private IMyRemoteControl Rc { get; }

        public IMyCubeGrid Grid { get; }

        private BotBase Ai { get; set; }

        public void Init()
        {
            Ai = FabricateBot(Grid, Rc);
            Ai.OnWriteToLog += WriteGeneral;
            Ai.OnClose += close =>
            {
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
            BotType botType = BotBase.ReadBotType(rc);

            BotBase bot = null;
            switch (botType)
            {
                case BotType.Fighter:
                    WriteGeneral("FabricateBot", "New Bot: Fighter");
                    bot = new BotTypeFighter(grid, _botDamageHandler);
                    break;
                case BotType.Freighter:
                    WriteGeneral("FabricateBot", "New Bot: Freighter");
                    bot = new BotTypeFreighter(grid, _botDamageHandler);
                    break;
                case BotType.Station:
                    WriteGeneral("FabricateBot", "New Bot: Station");
                    bot = new BotTypeStation(grid, _botDamageHandler);
                    break;
                case BotType.None:
                    break;
                case BotType.Invalid:
                    break;
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