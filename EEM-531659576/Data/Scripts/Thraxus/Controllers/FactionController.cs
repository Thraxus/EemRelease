using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.Extensions;
using Eem.Thraxus.Entities.Bots;
using Eem.Thraxus.Entities.Factions.Models;
using Eem.Thraxus.Extensions;
using Eem.Thraxus.Helpers;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Controllers
{
    //[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class FactionController : BaseLoggingClass
    {
        private readonly CoordinationController _coordinationController;

        //protected override string CompName { get; } = "FactionCore";
        //protected override CompType Type { get; } = CompType.Server;
        //protected override MyUpdateOrder Schedule { get; } = MyUpdateOrder.BeforeSimulation;
        //protected override bool SkipReporting { get; } = true;
        
        private ulong _tickTimer;

        public FactionController(CoordinationController coordinationController)
        {
            _coordinationController = coordinationController;
        }

        public RelationshipManager RelationshipManager { get; private set; }

        public void Init()
        {
            //FactionCoreStaticInstance = this;
            RelationshipManager = new RelationshipManager();
            RelationshipManager.OnWriteToLog += WriteGeneral;
            RelationshipManager.Init();
            //_botDamageHandler.OnTriggerWar += TriggerWar;
            WriteGeneral("FactionCore", "Online!");
        }

        public void TriggerWar(long assaulted, long assaulter)
        {
            var right = MyAPIGateway.Session.Factions.TryGetPlayerFaction(assaulter);
            var left = (MyAPIGateway.Entities.GetEntityById(assaulted).GetTopMostParent() as MyCubeGrid).GetOwnerFaction();
            
            if (left == null || right == null || left == right)
            {
                return;
            }
            WriteGeneral("TriggerWar", $"Asshats! [{left?.Tag ?? "NA"}] [{assaulted.ToEntityIdFormat()}] [{right?.Tag ?? "NA"}] [{assaulter.ToEntityIdFormat()}]");
            RelationshipManager.WarDeclaration(left, right);
        }

        public void Update()
        {
            TickTimer();
        }

        private void TickTimer()
        {
            _tickTimer++;
            if (_tickTimer % Constants.FactionNegativeRelationshipAssessment == 0)
                RelationshipManager?.CheckNegativeRelationships();
            if (_tickTimer % Constants.FactionMendingRelationshipAssessment == 0)
                RelationshipManager?.CheckMendingRelationships();
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