using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Enums;
using Eem.Thraxus.Extensions;
using Eem.Thraxus.Helpers;
using Eem.Thraxus.Models;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using IMyRemoteControl = Sandbox.ModAPI.IMyRemoteControl;
using IMySmallGatlingGun = Sandbox.ModAPI.IMySmallGatlingGun;
using IMySmallMissileLauncher = Sandbox.ModAPI.IMySmallMissileLauncher;
using IMyTerminalBlock = Sandbox.ModAPI.IMyTerminalBlock;
using InGame = Sandbox.ModAPI.Ingame;


namespace Eem.Thraxus.Entities.Bots
{
    public sealed class BotTypeFighter : BotBase
    {
        public static readonly BotType BotType = BotType.Fighter;

        private FighterSettings _fighterSetup;

        //public BotTypeFighter(IMyCubeGrid grid, BotConfig botConfig, BotDamageHandler botDamageHandler)
        //    : base(grid, botConfig, botDamageHandler) { }

        public BotTypeFighter(IMyCubeGrid grid, BotConfig botConfig)
            : base(grid, botConfig) { }

        //private bool Damaged { get; set; }

        public bool KeenAiLoaded { get; private set; }

        public override bool Init(IMyRemoteControl rc)
        {
            if (!base.Init(rc)) return false;
            WriteGeneral("Init", "Bot Fighter Booting...");
            //Update |= MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME;

            if (_fighterSetup.CallHelpOnDamage) OnDamaged += DamageHandler;

            if (rc != null)
            {
                rc.Name = DroneNameProvider;
                MyAPIGateway.Entities.SetEntityName(rc);
            }

            if (!_fighterSetup.DelayedAiEnable) LoadKeenAi();
            return true;
        }

        public void LoadKeenAi()
        {
            WriteGeneral("LoadKeenAI", "Loading AI...");
            try
            {
                if (KeenAiLoaded) return;
                (Rc as MyRemoteControl)?.SetAutoPilotSpeedLimit(Rc.GetSpeedCap());
                MyVisualScriptLogicProvider.SetDroneBehaviourFull(Rc.Name, _fighterSetup.Preset, maxPlayerDistance: _fighterSetup.SeekDistance, playerPriority: 0);
                if (_fighterSetup.AmbushMode) MyVisualScriptLogicProvider.DroneSetAmbushMode(Rc.Name);
                MyVisualScriptLogicProvider.TargetingSetWhitelist(Rc.Name);
                KeenAiLoaded = true;
            }
            catch (Exception scrap)
            {
                WriteGeneral("LoadKeenAI", scrap.Message);
            }

            WriteGeneral("LoadKeenAI", "AI Loaded!");
        }

        private void DamageHandler(IMySlimBlock block, MyDamageInformation damage)
        {
            IMyPlayer damager;
            ReactOnDamage(damage, out damager);

            if (_fighterSetup.DelayedAiEnable) LoadKeenAi();

            foreach (IMyTimerBlock timer in Term.GetBlocksOfType<IMyTimerBlock>(x => x.IsFunctional && x.Enabled
                                                                                                    && x.CustomName.Contains("Damage")))
                timer.Trigger();

            if (!_fighterSetup.CallHelpOnDamage) return;

            foreach (IMyTimerBlock timer in Term.GetBlocksOfType<IMyTimerBlock>(x => x.IsFunctional && x.Enabled
                                                                                                    && x.CustomName.Contains("Security")))
                timer.Trigger();
        }

        public override void Main()
        {
            if (_fighterSetup.DelayedAiEnable && !KeenAiLoaded)
            {
                DelayedAI_Main();
                return;
            }

            List<InGame.MyDetectedEntityInfo> enemiesInProximity = LookForEnemies(_fighterSetup.SeekDistance);
            WriteGeneral("Main", $"Found [{enemiesInProximity.Count:D2}] Enemies!");

            MyVisualScriptLogicProvider.DroneTargetLoseCurrent(Rc.Name);
            if (enemiesInProximity.Count > 0)
                MyVisualScriptLogicProvider.DroneSetTarget(Rc.Name, GetTopPriorityTarget(enemiesInProximity).GetEntity() as MyEntity);
        }

        private InGame.MyDetectedEntityInfo GetTopPriorityTarget(List<InGame.MyDetectedEntityInfo> targets)
        {
            if (targets == null || targets.Count == 0) return new InGame.MyDetectedEntityInfo();
            if (targets.Count == 1) return targets.First();

            List<InGame.MyDetectedEntityInfo> mostDangerous;

            if (targets.Any(x => Distance(x) <= 200 && RelSpeed(x) <= 40, out mostDangerous))
                return mostDangerous.OrderBy(Distance).First();

            List<InGame.MyDetectedEntityInfo> targetsClose = targets.Where(x => Distance(x) <= 1200).ToList();

            if (targetsClose.Count > 0) return targetsClose.OrderBy(x => DangerIndex(x) / x.GetMassT()).First();

            List<InGame.MyDetectedEntityInfo> targetsFar = targets.Where(x => Distance(x) > 1200).ToList();

            return targetsFar.Count > 0 ? targetsFar.OrderBy(x => DangerIndex(x) / x.GetMassT()).First() : new InGame.MyDetectedEntityInfo();
        }

        private float DangerIndex(InGame.MyDetectedEntityInfo enemy)
        {
            if (enemy.Type == InGame.MyDetectedEntityType.CharacterHuman)
                return Distance(enemy) < 100 ? 100 : 10;
            if (!enemy.IsGrid()) return 0;

            float dangerIndex = 0;
            IMyCubeGrid enemyGrid = enemy.GetGrid();

            var enemySlimBlocks = new List<IMySlimBlock>();
            enemyGrid.GetBlocks(enemySlimBlocks, x => x.FatBlock is IMyTerminalBlock);

            List<IMyTerminalBlock> enemyBlocks = enemySlimBlocks.Select(x => x.FatBlock as IMyTerminalBlock).ToList();

            dangerIndex += enemyBlocks.Count(x => x is IMyLargeMissileTurret) * 300;
            dangerIndex += enemyBlocks.Count(x => x is IMyLargeGatlingTurret) * 100;
            dangerIndex += enemyBlocks.Count(x => x is IMySmallMissileLauncher) * 400;
            dangerIndex += enemyBlocks.Count(x => x is IMySmallGatlingGun) * 250;
            dangerIndex += enemyBlocks.Count(x => x is IMyLargeInteriorTurret) * 40;

            if (enemy.Type == InGame.MyDetectedEntityType.LargeGrid) dangerIndex *= 2.5f;
            return dangerIndex;
        }

        private void DelayedAI_Main()
        {
            if (!_fighterSetup.DelayedAiEnable || KeenAiLoaded || !(_fighterSetup.AiActivationDistance > 0)) return;

            List<InGame.MyDetectedEntityInfo> enemiesInProximity = LookForEnemies(_fighterSetup.AiActivationDistance);
            if (enemiesInProximity == null)
            {
                WriteGeneral("DelayedAI_Main()", "WEIRD: EnemiesInProximity == null");
                return;
            }

            if (enemiesInProximity.Count > 0) LoadKeenAi();
        }

        protected override void ParseSetup()
        {
            _fighterSetup.Preset = BotConfig.PresetString;
            _fighterSetup.DelayedAiEnable = BotConfig.DelayedAi;
            _fighterSetup.AmbushMode = BotConfig.AmbushMode;
            _fighterSetup.SeekDistance = BotConfig.SeekDistance;
            _fighterSetup.AiActivationDistance = BotConfig.ActivationDistance;

            _fighterSetup.Default();
        }

        private struct FighterSettings
        {
            public float AiActivationDistance;
            public bool AmbushMode;
            public bool CallHelpOnDamage;
            public bool DelayedAiEnable;
            public string Preset;
            public float SeekDistance;

             //<summary>
             //    Fills out the empty values in struct with default values, and leaves filled values untouched.
             //</summary>
            public void Default(bool randomizeCallHelp = true)
            {
                // This correlates back to DroneBehaviors_Eem.sbc as the SubtypeId, so needs to remain a string for now.
                if (Preset == null) Preset = "DefaultDirect";
                if (Math.Abs(SeekDistance - 0) < 0) SeekDistance = 10000;
                if (CallHelpOnDamage == false || randomizeCallHelp) RandomizeCallHelp();
            }

            private void RandomizeCallHelp()
            {
                // Giving this a default 10% chance to not call for help - there is no actual prefab config for this as the setup suggested there was.
                CallHelpOnDamage = Constants.Random.Next(0, 100) < 91;
            }

            public override string ToString()
            {
                return $"Preset='{Preset}|CallHelp={CallHelpOnDamage}|SeekDistance={SeekDistance}";
            }
        }
    }
}