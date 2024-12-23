using System;
using System.Collections.Generic;
using Eem.Thraxus.Enums;
using Eem.Thraxus.Extensions;
using Eem.Thraxus.Helpers;
using Eem.Thraxus.Models;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Entities.Bots
{
    public sealed class BotTypeStation : BotBase
    {
        private const string SecurityOnTimerPrefix = "[Alert_On]";

        private const string SecurityOffTimerPrefix = "[Alert_Off]";

        public static readonly BotType BotType = BotType.Station;

        //private readonly Timer _calmDownTimer = new Timer();

        //private DateTime? _alertTriggerTime;

        public BotTypeStation(IMyCubeGrid grid, BotConfig botConfig)
            : base(grid, botConfig) { }

        //private bool WasDamaged => _alertTriggerTime != null;

        public override bool Init(IMyRemoteControl rc)
        {
            if (!base.Init(rc)) return false;
            WriteGeneral("Init", "Bot Station Booting...");
            //OnDamaged += DamageHandler;
            //OnBlockPlaced += BlockPlacedHandler;

            //_calmDownTimer.AutoReset = false;
            //_calmDownTimer.Elapsed += (trash1, trash2) =>
            //{
            //    _calmDownTimer.Stop();
            //    CalmDown();
            //};

            return true;
        }

        private int _calmDown = 0;

        private void EvaluateCalmDownTimer()
        {
            if (!_onAlert) return;
            WriteGeneral(nameof(EvaluateCalmDownTimer), $"Evaluating Calm Down Timer: {_calmDown:D4}");
            _calmDown -= 10;
            if (_calmDown > 0) return;
            CalmDown();
        }

        private bool _onAlert;

        public override void TriggerAlert()
        {
            if (_onAlert)
            {
                _calmDown = 1000; // make this higher!
                return;
            }
            //WriteGeneral(nameof(TriggerAlert), $"Alert Triggered! [{assaulted.ToEntityIdFormat()}] [{blockId.ToEntityIdFormat()}] [{assaulter.ToEntityIdFormat()}]");
            OnAlert();
        }

        //private void DamageHandler(IMySlimBlock block, MyDamageInformation damage)
        //{
        //    if (block == null) return;
        //    //if (!block.IsDestroyed && damage.IsThruster()) return;
        //    IMyPlayer damager;
        //    ReactOnDamage(damage, out damager);
        //    if (damager != null) OnAlert();
        //}

        private void OnAlert()
        {
            try
            {
                _onAlert = true;
                WriteGeneral("OnAlert", "Alert activated.");
                Default_SwitchTurretsAndRunTimers(true);
                //if (!WasDamaged) Default_SwitchTurretsAndRunTimers(true);
                //_alertTriggerTime = DateTime.Now;
                //_calmDownTimer.Stop();
                //_calmDownTimer.Interval = 100000;
                //_calmDownTimer.Start();
            }
            catch (Exception scrap)
            {
                WriteGeneral("OnAlert", scrap.Message);
            }
        }

        protected override void ParseSetup()
        {
            // Do nothing.
        }

        public override void Main()
        {
            base.Main();
            EvaluateCalmDownTimer();
            //if (WasDamaged && DateTime.Now - _alertTriggerTime > CalmdownTime) CalmDown();
        }

        private void CalmDown()
        {
            try
            {
                WriteGeneral("CalmDown", $"Calm Down activated {_calmDown:D4}");
                //_alertTriggerTime = null;
                Default_SwitchTurretsAndRunTimers(false);
                _onAlert = false;
            }
            catch (Exception scrap)
            {
                WriteGeneral("Calmdown", scrap.Message);
            }
        }

        private void Default_SwitchTurretsAndRunTimers(bool securityState)
        {
            //try
            //{
            //    List<IMyLargeTurretBase> Turrets = Term.GetBlocksOfType<IMyLargeTurretBase>();
            //    foreach (IMyLargeTurretBase Turret in Turrets)
            //    {
            //        Turret.SetSecurity_EEM(SecurityState);
            //    }
            //}
            //catch (Exception Scrap)
            //{
            //    LogError("SwitchTurrets", Scrap, "StationBot.");
            //}
            try
            {
                foreach (IMyTimerBlock timer in Timers)
                {
                    if (!timer.IsWorking ||
                        !timer.CustomName.Contains(securityState ? SecurityOnTimerPrefix : SecurityOffTimerPrefix)) continue;
                    
                    timer.Trigger();
                }

                foreach (IMyRadioAntenna antenna in Antennae)
                {
                    if (!antenna.IsWorking || !antenna.CustomData.Contains("Security:CallForHelp")) continue;
                    antenna.Enabled = securityState;
                }
                //List<IMyTimerBlock> alertTimers = Term.GetBlocksOfType<IMyTimerBlock>
                //(x => (x.IsWorking && x.CustomName.Contains(securityState ? SecurityOnTimerPrefix : SecurityOffTimerPrefix))
                //      || x.CustomData.Contains(securityState ? SecurityOnTimerPrefix : SecurityOffTimerPrefix));

                //foreach (IMyTimerBlock timer in alertTimers) timer.Trigger();
            }
            catch (Exception scrap)
            {
                WriteGeneral("TriggerTimers", scrap.Message);
            }

            //try
            //{
            //    List<IMyRadioAntenna> callerAntennae = Term.GetBlocksOfType<IMyRadioAntenna>
            //        (x => x.IsWorking && x.CustomData.Contains("Security:CallForHelp"));
            //    foreach (IMyRadioAntenna antenna in callerAntennae) antenna.Enabled = securityState;
            //}
            //catch (Exception scrap)
            //{
            //    WriteGeneral("EnableAntennae", scrap.Message);
            //}

            //if (Constants.DebugMode) MyAPIGateway.Utilities.ShowMessage($"{Grid.DisplayName}", $"{(securityState ? "Security Alert!" : "Security calmdown")}");
        }
    }

    // Unfinished
    /*
    public class FactoryManager
    {
        readonly IMyCubeGrid Grid;
        readonly IMyGridTerminalSystem Term;
        List<IMyTerminalBlock> InventoryOwners = new List<IMyTerminalBlock>();
        List<IMyAssembler> Assemblers = new List<IMyAssembler>();
        Dictionary<MyDefinitionId, float> ItemsTotal = new Dictionary<MyDefinitionId, float>();
        Dictionary<MyDefinitionId, float> ItemMinimalQuotas = new Dictionary<MyDefinitionId, float>();

        public FactoryManager(IMyCubeGrid Grid)
        {
            this.Grid = Grid;
            Term = Grid.GetTerminalSystem();
        }

        public void LoadInventoryOwners()
        {
            InventoryOwners = Grid.GetBlocks<IMyTerminalBlock>(x => x.HasInventory);
        }

        void ParseAssemblerQuotas(string Input)
        {
            var CustomData = Input.Trim().Replace("\r\n", "\n").Split('\n');
            foreach (string DataLine in CustomData)
            {
                // Syntax:
                // MinQuota:Type/Subtype:Amount
                if (DataLine.StartsWith("MinQuota"))
                {
                    var Data = DataLine.Split(':');
                    MyDefinitionId Definition;
                    float Quota;
                    if (MyDefinitionId.TryParse(Data[1], out Definition) && float.TryParse(Data[2], out Quota))
                    {
                        if (!ItemMinimalQuotas.ContainsKey(Definition))
                            ItemMinimalQuotas.Add(Definition, Quota);
                        else
                            ItemMinimalQuotas[Definition] += Quota;
                    }
                }
            }
        }

        public void SumItems()
        {
            ItemsTotal.Clear();
            foreach (IMyTerminalBlock InventoryOwner in InventoryOwners)
            {
                foreach (IMyInventory Inventory in InventoryOwner.GetInventories())
                {
                    foreach (IMyInventoryItem Item in Inventory.GetItems())
                    {
                        var Blueprint = Item.GetBlueprint();
                        if (ItemsTotal.ContainsKey(Blueprint))
                            ItemsTotal[Blueprint] += (float)Item.Amount;
                        else
                            ItemsTotal.Add(Blueprint, (float)Item.Amount);
                    }
                }
            }
        }
    }*/
}