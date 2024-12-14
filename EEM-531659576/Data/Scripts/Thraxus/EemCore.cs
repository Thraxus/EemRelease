using Eem.Thraxus.Bots;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.Enums;
using Eem.Thraxus.Common.Generics;
using Eem.Thraxus.Factions;
using Eem.Thraxus.Networking;
using Sandbox.ModAPI;
using VRage.Game.Components;

namespace Eem.Thraxus
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class EemCore : BaseSessionComp
    {
        /* TODO

        1) Create new Faction Types in FactionTypes.sbc for EEM specifically
            -- Done. Was not necessary, so all code commented out.
        2) Decide how the prefabs should be distributed to this new system
        3) Spawn all prefabs in specific saves per category type (i.e. all traders in one save, cargos in another, encounters in another?)
        4) Remove AI init that sets factions from all RCs
            - Can keep behavior or other items as necessary?
        5) Update EEM to use the Common framework from my other mods
            -- Done.  Also updated some bad references in files to obsolete code.
        6) Make bots not suck as a simple first pass, and divorce them from "AiSessionCore"
            - NO FANCY SHIT THIS TIME AROUND ASSHOLE!!!
            - Pull them off GameLogic of the RC into Session Logic
            - Simplify the setup routine
            - Fix the awful threat detection
        7) Rename "AiSessionCore" to "EemCore"
            -- Done.  Also moved Bots to their own area, but did not do much for logic yet.
        8) Stop Factions from caring about any faction but EEM for protections
        9) See if you can remove all reliance on the Client Code
            - Code change done, but still should look deeper into Factions and keeping all stuff to EEM only


        DEV NOTES Dec 10 2024

        Ai works, but it's kinda weird. The bot stops reacting whenever it takes damage, then resumes to care after several seconds.
        This may be due to throttling the DamageHandler.

        Faction assignment for Drones is not correct - may need to revisit using the old way but ensuring whatever method is used for drones does not affect
            cargos so the sbc setup for spawn groups can be leveraged for a more diverse spawn pool

        Messaging does not work.

        EEMUnstableDev has a ton of code for the new bots I was working on.  Might need to leverage that if I can't get the old system to work how I want.
        */

        public static bool DisableAi = false;

        private readonly ActionQueue _genericActionQueue = new ActionQueue();

        private BotAiCore _botAiCore;

        private BotDamageHandler _botDamageHandler;

        private FactionCore _factionCore;
        protected override string CompName { get; } = "EemCore";
        protected override CompType Type { get; } = CompType.Server;
        protected override MyUpdateOrder Schedule { get; } = MyUpdateOrder.BeforeSimulation;
        protected override bool SkipReporting { get; } = true;

        protected override void SuperEarlySetup()
        {
            base.SuperEarlySetup();
            Messaging.Register();
            _botDamageHandler = new BotDamageHandler();
            _factionCore = new FactionCore(_botDamageHandler);
            _botDamageHandler.OnWriteToLog += WriteGeneral;
            //_botDamageHandler.Init();
            _factionCore.OnWriteToLog += WriteGeneral;
            _factionCore.Init();
            if (DisableAi) return;
            _botAiCore = new BotAiCore(_genericActionQueue, _botDamageHandler);
            _botAiCore.OnWriteToLog += WriteGeneral;
            _botAiCore.Init();
        }

        protected override void LateSetup()
        {
            base.LateSetup();
            //_botDamageHandler.OnWriteToLog += WriteGeneral;
            _botDamageHandler.Init(_genericActionQueue);
            //_factionCore.OnWriteToLog += WriteGeneral;
            //_factionCore.Init();
            //if (DisableAi) return;
            //_botAiCore.OnWriteToLog += WriteGeneral;
            //_botAiCore.Init();
        }

        protected override void BeforeSimUpdate()
        {
            base.BeforeSimUpdate();
            _factionCore.Update();
            _genericActionQueue.Execute();
        }

        protected override void BeforeSimUpdate10Ticks()
        {
            base.BeforeSimUpdate10Ticks();
            WriteGeneral("BeforeSimUpdate10Ticks", "Updating...");
            _botAiCore.Update10();
        }

        public override void LoadData()
        {
            base.LoadData();
            const int eemPcuLimit = 500000;
            MyAPIGateway.Session.SessionSettings.EnableIngameScripts = true;
            MyAPIGateway.Session.SessionSettings.EnableDrones = true;
            if (MyAPIGateway.Session.SessionSettings.PiratePCU < eemPcuLimit) MyAPIGateway.Session.SessionSettings.PiratePCU = eemPcuLimit;
            if (MyAPIGateway.Session.SessionSettings.GlobalEncounterPCU < eemPcuLimit) MyAPIGateway.Session.SessionSettings.GlobalEncounterPCU = eemPcuLimit;
            MyAPIGateway.Session.SessionSettings.EncounterDensity = 0.65f;
            MyAPIGateway.Session.SessionSettings.GlobalEncounterTimer = 10;
        }

        protected override void Unload()
        {
            base.Unload();
            Messaging.Unregister();
            _botDamageHandler.OnWriteToLog -= WriteGeneral;
            _botDamageHandler.Close();
            _factionCore.OnWriteToLog -= WriteGeneral;
            _factionCore.Close();
            _botAiCore.OnWriteToLog -= WriteGeneral;
            _botAiCore.Close();
        }
    }
}