using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.Extensions;
using Eem.Thraxus.Entities.Bots;
using Eem.Thraxus.Entities.Factions.Models;
using Eem.Thraxus.Helpers;

namespace Eem.Thraxus.Controllers
{
    //[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class FactionController : BaseLoggingClass
    {
        //protected override string CompName { get; } = "FactionCore";
        //protected override CompType Type { get; } = CompType.Server;
        //protected override MyUpdateOrder Schedule { get; } = MyUpdateOrder.BeforeSimulation;
        //protected override bool SkipReporting { get; } = true;

        private readonly BotDamageHandler _botDamageHandler;

        private ulong _tickTimer;

        public FactionController(BotDamageHandler botDamageHandler)
        {
            _botDamageHandler = botDamageHandler;
        }

        public RelationshipManager RelationshipManager { get; private set; }

        //public static FactionCore FactionCoreStaticInstance;

        public void Init()
        {
            //FactionCoreStaticInstance = this;
            RelationshipManager = new RelationshipManager();
            RelationshipManager.OnWriteToLog += WriteGeneral;
            RelationshipManager.Init();
            //_botDamageHandler.OnTriggerWar += TriggerWar;
            WriteGeneral("FactionCore", "Online!");
        }

        private void TriggerWar(long assaulted, long assaulter)
        {
            WriteGeneral("TriggerWar", $"Asshats! [{assaulted.ToEntityIdFormat()}] [{assaulter.ToEntityIdFormat()}]");
            RelationshipManager.WarDeclaration(assaulted, assaulter);
        }

        public void Update()
        {
            TickTimer();
        }

        private void TickTimer()
        {
            _tickTimer++;
            if (_tickTimer % Constants.FactionNegativeRelationshipAssessment == 0)
                RelationshipManager.CheckNegativeRelationships();
            if (_tickTimer % Constants.FactionMendingRelationshipAssessment == 0)
                RelationshipManager.CheckMendingRelationships();
        }

        public override void Close()
        {
            RelationshipManager.OnWriteToLog -= WriteGeneral;
            RelationshipManager?.Close();
            //FactionCoreStaticInstance = null;
            WriteGeneral("FactionCore", "I'm out!");
            base.Close();
        }

        //protected override void SuperEarlySetup()
        //{
        //    base.SuperEarlySetup();
        //    FactionCoreStaticInstance = this;
        //}

        //protected override void EarlySetup()
        //{
        //    base.EarlySetup();
        //    RelationshipManager = new RelationshipManager();
        //    RelationshipManager.OnWriteToLog += WriteGeneral;
        //    RelationshipManager.Init();
        //    WriteGeneral("FactionCore", $"RegisterEarly Complete... {UpdateOrder}");
        //}

        //protected override void BeforeSimUpdate()
        //{
        //    base.BeforeSimUpdate();
        //    TickTimer();
        //}

        //protected override void Unload()
        //{
        //    base.Unload();
        //    RelationshipManager.OnWriteToLog -= WriteGeneral;
        //    RelationshipManager?.Close();
        //    FactionCoreStaticInstance = null;
        //    WriteGeneral("FactionCore", $"I'm out!... {UpdateOrder}");
        //}
    }
}