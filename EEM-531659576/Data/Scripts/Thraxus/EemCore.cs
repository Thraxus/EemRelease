using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.Enums;
using Eem.Thraxus.Controllers;
using Eem.Thraxus.Helpers;
using Eem.Thraxus.Networking;
using Sandbox.ModAPI;
using VRage.Game.Components;

namespace Eem.Thraxus
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class EemCore : BaseSessionComp
    {
        private CoordinationController _coordinationController;

        protected override string CompName { get; } = "EemCore";
        protected override CompType Type { get; } = CompType.Server;
        protected override MyUpdateOrder Schedule { get; } = MyUpdateOrder.BeforeSimulation | MyUpdateOrder.AfterSimulation;

        protected override void SuperEarlySetup()
        {
            base.SuperEarlySetup();
            Messaging.Register();
            _coordinationController = new CoordinationController();
            _coordinationController.OnWriteToLog += WriteGeneral;
            _coordinationController.EarlyInit();
        }

        protected override void LateSetup()
        {
            base.LateSetup();
            _coordinationController.LateInit();
        }

        protected override void BeforeSimUpdate()
        {
            base.BeforeSimUpdate();
            _coordinationController.BeforeSimUpdate();
        }

        protected override void AfterSimUpdate()
        {
            base.AfterSimUpdate();
            _coordinationController.AfterSimUpdate();
        }

        protected override void BeforeSimUpdate10Ticks()
        {
            base.BeforeSimUpdate10Ticks();
            _coordinationController.BeforeSimUpdate10();
        }

        public override void LoadData()
        {
            base.LoadData();
            const int eemPcuLimit = 500000;
            MyAPIGateway.Session.SessionSettings.EnableIngameScripts = true;
            MyAPIGateway.Session.SessionSettings.EnableDrones = true;
            MyAPIGateway.Session.SessionSettings.MaxDrones = 25;
            if (MyAPIGateway.Session.SessionSettings.PiratePCU < eemPcuLimit) MyAPIGateway.Session.SessionSettings.PiratePCU = eemPcuLimit;
            if (MyAPIGateway.Session.SessionSettings.GlobalEncounterPCU < eemPcuLimit) MyAPIGateway.Session.SessionSettings.GlobalEncounterPCU = eemPcuLimit;
            MyAPIGateway.Session.SessionSettings.EncounterDensity = 0.65f;
            MyAPIGateway.Session.SessionSettings.GlobalEncounterTimer = 10;
        }

        protected override void Unload()
        {
            Messaging.Unregister();
            _coordinationController.OnWriteToLog -= WriteGeneral;
            base.Unload();
        }
    }
}