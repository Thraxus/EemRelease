using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Common.Utilities.Statics;
using Eem.Thraxus.Enums;
using Eem.Thraxus.Extensions;
using Eem.Thraxus.Helpers;
using Eem.Thraxus.Models;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRageMath;
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
            if (rc == null) return false;
            WriteGeneral("Init", $"Bot Fighter Booting... [{rc.GetTopMostParent().DisplayName}]");
            //Update |= MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME;

            //if (_fighterSetup.CallHelpOnDamage) OnDamaged += DamageHandler;

            rc.Name = DroneNameProvider;
            //MyAPIGateway.Entities.SetEntityName(rc);

            if (!_fighterSetup.DelayedAiEnable) LoadKeenAi();
            return true;
        }

        public void LoadKeenAi()
        {
            //WriteGeneral("LoadKeenAI", $"Loading AI... [{BotConfig.SeekDistance}]");
            //WriteGeneral("LoadKeenAI", $"Configuration... [{BotConfig.ToStringVerbose()}]");
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

        private bool _onAlert;

        public override void TriggerAlert()
        {
            if (_onAlert)
            {
                return;
            }

            _onAlert = true;
            WriteGeneral(nameof(TriggerAlert), "Triggering Alert!");


            //WriteGeneral(nameof(TriggerAlert), $"Alert Triggered! [{assaulted.ToEntityIdFormat()}] [{blockId.ToEntityIdFormat()}] [{assaulter.ToEntityIdFormat()}]");
            if (_fighterSetup.DelayedAiEnable) LoadKeenAi();

            foreach (IMyTimerBlock timer in Timers)
            {
                if (!timer.IsFunctional || !timer.Enabled || !(timer.CustomName.Contains("Damage") || (timer.CustomName.Contains("Security") && _fighterSetup.CallHelpOnDamage))) continue;
                timer.Trigger();
            }

            //if (!_fighterSetup.CallHelpOnDamage) return;

            //foreach (IMyTimerBlock timer in 
            //         Term.GetBlocksOfType<IMyTimerBlock>(x => 
            //             x.IsFunctional && 
            //             x.Enabled && 
            //             x.CustomName.Contains("Security")))
            //    timer.Trigger();
        }

        //private void DamageHandler(IMySlimBlock block, MyDamageInformation damage)
        //{
        //    IMyPlayer damager;
        //    ReactOnDamage(damage, out damager);

        //    if (_fighterSetup.DelayedAiEnable) LoadKeenAi();

        //    foreach (IMyTimerBlock timer in Term.GetBlocksOfType<IMyTimerBlock>(x => x.IsFunctional && x.Enabled
        //                                                                                            && x.CustomName.Contains("Damage")))
        //        timer.Trigger();

        //    if (!_fighterSetup.CallHelpOnDamage) return;

        //    foreach (IMyTimerBlock timer in Term.GetBlocksOfType<IMyTimerBlock>(x => x.IsFunctional && x.Enabled
        //                                                                                            && x.CustomName.Contains("Security")))
        //        timer.Trigger();
        //}

        private HashSet<MyEntity> _targets = new HashSet<MyEntity>();

        public override void Main()
        {
            base.Main();
            if (_fighterSetup.DelayedAiEnable && !KeenAiLoaded)
            {
                DelayedAI_Main();
                return;
            }
            FindAFucker(_fighterSetup.SeekDistance);
        }

        private void DelayedAI_Main()
        {
            if (!_fighterSetup.DelayedAiEnable || KeenAiLoaded || !(_fighterSetup.AiActivationDistance > 0)) return;
            FindAFucker(_fighterSetup.AiActivationDistance);
        }

        private void Lurk()
        {
            MyVisualScriptLogicProvider.DroneTargetLoseCurrent(Rc.Name);
            MyVisualScriptLogicProvider.DroneWaypointClear(Rc.Name);
            MyVisualScriptLogicProvider.AutopilotClearWaypoints(Rc.Name);
            Rc.ClearWaypoints();
        }

        private void FindAFucker(float distance)
        {
            _targets.Clear();
            _targets = FindTargets(distance);

            if (_targets.Count == 0)
            {
                Lurk();
                return;
            }

            if (_targets.Count > 1)
            {
                FightAFucker(GetMostDangerous(GetEnemiesSortedByRange(_targets)));
                return;
            }

            FightAFucker(_targets.First());
        }

        private int _counter = 0;

        private void FightAFucker(MyEntity fucker)
        {
            //var myRemote = (MyRemoteControl)Rc;
            //myRemote.TargetData = new MyCharacter.MyTargetData
            //{

            //};


            MyVisualScriptLogicProvider.DroneTargetLoseCurrent(Rc.Name);
            MyVisualScriptLogicProvider.DroneSetTarget(Rc.Name, fucker);
            MyVisualScriptLogicProvider.DroneSetSpeedLimit(Rc.Name, Rc.GetSpeedCap());
            //myRemote.ClearWaypoints();
            //myRemote.AddWaypoint(fucker.PositionComp.GetPosition(), "Target");

            if (_counter++ % 5 != 0) return;
            UpdateGps(fucker);

            //Rc.ClearWaypoints();
            //Rc.AddWaypoint(fucker.PositionComp.GetPosition(), "Boom!");
        }

        //public Vector3D CalculateIntercept(MyEntity fucker)
        //{
        //    return CalculateIntercept(Grid.PositionComp.GetPosition(), GridVelocity, fucker.PositionComp.GetPosition(), fucker.Physics.LinearVelocity);
        //}

        public static double MeasureDistance(Vector3D source, Vector3D target)
        {
            Vector3D difference = target - source;
            return difference.Length();
        }

        public static Vector3D SetPositionAwayFromTarget(Vector3D source, Vector3D target, float distance = 300f)
        {
            Vector3D direction = source - target;
            direction.Normalize();
            return target + (direction * distance);
        }
        
        public Vector3D CalculateIntercept(Vector3D position1, Vector3D velocity1, Vector3D position2, Vector3D velocity2)
        {
            Vector3D relativePosition = position2 - position1;
            Vector3D relativeVelocity = velocity2 - velocity1;
            var t = Vector3D.Dot(relativePosition, relativeVelocity) / relativeVelocity.LengthSquared();
            return position1 + velocity1 * t;
        }

        private void UpdateGps(MyEntity fucker)
        {
            if (!KeenAiLoaded || fucker == null) return;

            var myRemote = (MyRemoteControl)Rc;
            myRemote.ClearWaypoints();
            MyVisualScriptLogicProvider.AutopilotClearWaypoints(Rc.Name);

            if (MeasureDistance(GridPosition, fucker.PositionComp.GetPosition()) < 300)
            {
                SetPositionAwayFromTarget(GridPosition, fucker.PositionComp.GetPosition());
                return;
            }
            
            var targetWaypoint = CalculateIntercept(fucker.PositionComp.GetPosition(), fucker.Physics.LinearVelocity, Grid.PositionComp.GetPosition(), GridVelocity);
            myRemote.AddWaypoint(targetWaypoint, "Target");
            Statics.AddGpsLocation("Boom!", targetWaypoint);
        }

        private MyEntity GetMostDangerous(Dictionary<int, HashSet<MyEntity>> enemiesSortedByRange)
        {
            _mostDangerous.DangerIndex = 0;
            _mostDangerous.MyEntity = null;
            
            foreach (var enemyGroup in enemiesSortedByRange)
            {
                GetMostDangerous(enemyGroup);
            }
            
            return _mostDangerous.MyEntity;
        }

        private readonly DangerAssessment _mostDangerous = new DangerAssessment();
        
        private void GetMostDangerous(KeyValuePair<int, HashSet<MyEntity>> group)
        {
            if (group.Value.Count == 0)
            {
                return;
            }

            foreach (var enemy in group.Value)
            {
                double tempDangerIndex = (10 - group.Key) * GetDangerIndex(enemy);
                WriteGeneral("GetMostDangerous", $"DI:[{tempDangerIndex:##.##}] {enemy.DisplayName ?? "Fred"}");
                if (tempDangerIndex < _mostDangerous.DangerIndex) continue;
                _mostDangerous.DangerIndex = tempDangerIndex;
                _mostDangerous.MyEntity = enemy;
            }
        }

        private HashSet<IMyTerminalBlock> _dangerIndexTerminalBlocks = new HashSet<IMyTerminalBlock>();

        private double GetDangerIndex(MyEntity target)
        {
            if (target is IMyCharacter)
            {
                return GridPosition.DistanceTo(target.PositionComp.GetPosition()) < 100 ? 100 : 10;
            }

            float dangerIndex = 0;
            var enemyGrid = target as IMyCubeGrid;
            if (enemyGrid == null)
            {
                return 0;
            }

            _dangerIndexTerminalBlocks.Clear();
            _dangerIndexTerminalBlocks = enemyGrid.GetFatBlocks<IMyTerminalBlock>().ToHashSet();

            dangerIndex += _dangerIndexTerminalBlocks.Count(x => x is IMyLargeMissileTurret) * 300;
            dangerIndex += _dangerIndexTerminalBlocks.Count(x => x is IMyLargeGatlingTurret) * 100;
            dangerIndex += _dangerIndexTerminalBlocks.Count(x => x is IMySmallMissileLauncher) * 400;
            dangerIndex += _dangerIndexTerminalBlocks.Count(x => x is IMySmallGatlingGun) * 250;
            dangerIndex += _dangerIndexTerminalBlocks.Count(x => x is IMyLargeInteriorTurret) * 40;

            if (enemyGrid.GridSizeEnum == MyCubeSize.Large) dangerIndex *= 2.5f;
            return dangerIndex;
        }

        //private float DangerIndex(InGame.MyDetectedEntityInfo enemy)
        //{
        //    if (enemy.Type == InGame.MyDetectedEntityType.CharacterHuman)
        //        return Distance(enemy) < 100 ? 100 : 10;
        //    if (!enemy.IsGrid()) return 0;

        //    float dangerIndex = 0;
        //    IMyCubeGrid enemyGrid = enemy.GetGrid();

        //    var enemySlimBlocks = new List<IMySlimBlock>();
        //    enemyGrid.GetBlocks(enemySlimBlocks, x => x.FatBlock is IMyTerminalBlock);

        //    List<IMyTerminalBlock> enemyBlocks = enemySlimBlocks.Select(x => x.FatBlock as IMyTerminalBlock).ToList();

        //    dangerIndex += enemyBlocks.Count(x => x is IMyLargeMissileTurret) * 300;
        //    dangerIndex += enemyBlocks.Count(x => x is IMyLargeGatlingTurret) * 100;
        //    dangerIndex += enemyBlocks.Count(x => x is IMySmallMissileLauncher) * 400;
        //    dangerIndex += enemyBlocks.Count(x => x is IMySmallGatlingGun) * 250;
        //    dangerIndex += enemyBlocks.Count(x => x is IMyLargeInteriorTurret) * 40;

        //    if (enemy.Type == InGame.MyDetectedEntityType.LargeGrid) dangerIndex *= 2.5f;
        //    return dangerIndex;
        //}

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