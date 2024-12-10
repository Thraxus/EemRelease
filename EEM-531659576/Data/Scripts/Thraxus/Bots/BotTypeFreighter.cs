using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Extensions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using IMyRemoteControl = Sandbox.ModAPI.IMyRemoteControl;
using InGame = Sandbox.ModAPI.Ingame;


namespace Eem.Thraxus.Bots
{
    public sealed class BotTypeFreighter : BotBase
    {
        public static readonly BotType BotType = BotType.Freighter;

        private FreighterSettings _freighterSetup;

        private Vector3D _startPoint;
        private Vector3D _endpoint;

        private struct FreighterSettings
        {
            public bool FleeOnlyWhenDamaged;
            public float FleeTriggerDistance;
            public float FleeSpeedRatio;
            public float FleeSpeedCap;
            public float CruiseSpeed;

            public void Default()
            {
                if (FleeOnlyWhenDamaged == default(bool)) FleeOnlyWhenDamaged = false;
                if (Math.Abs(FleeTriggerDistance - default(float)) < 1) FleeTriggerDistance = 1000;
                if (Math.Abs(FleeSpeedRatio - default(float)) < 1) FleeSpeedRatio = 1.0f;
                if (Math.Abs(FleeSpeedCap - default(float)) < 1) FleeSpeedCap = 300;
            }
        }

        private bool IsFleeing { get; set; }

        private bool FleeTimersTriggered { get; set; }

        public BotTypeFreighter(IMyCubeGrid grid, BotDamageHandler botDamageHandler) : base(grid, botDamageHandler)
        { }

        public override bool Init(IMyRemoteControl rc = null)
        {
            WriteGeneral("Init", "Bot Booting...");
            if (!base.Init(rc)) return false;
            OnDamaged += DamageHandler;
            OnBlockPlaced += BlockPlacedHandler;
            _startPoint = GridPosition;
            SetFlightPath();

            return true;
        }

        private void SetFlightPath()
        {
            _startPoint = GridPosition;
            var speed = (float)Rc.GetShipSpeed();
            Vector3D velocity = Rc.GetShipVelocities().LinearVelocity;
            if (speed > 5)
            {
                _endpoint = GridPosition + (Vector3D.Normalize(velocity) * 30000);
            }
            else
            {
                _endpoint = GridPosition + (Rc.WorldMatrix.Forward * 30000);
            }

            if (Math.Abs(_freighterSetup.CruiseSpeed - default(float)) > 0)
                (Rc as MyRemoteControl)?.SetAutoPilotSpeedLimit(_freighterSetup.CruiseSpeed);
            else
                (Rc as MyRemoteControl)?.SetAutoPilotSpeedLimit(speed > 5 ? speed : 30);

            (Rc as MyRemoteControl)?.SetCollisionAvoidance(true);
        }

        private void DamageHandler(IMySlimBlock block, MyDamageInformation damage)
        {
            try
            {
                IMyPlayer damager;
                ReactOnDamage(damage, out damager);
                if (damager == null) return;
                IsFleeing = true;
                Flee();
            }
            catch (Exception scrap)
            {
                WriteGeneral("DamageHandler", scrap.Message);
            }
        }

        public override void Main()
        {
            if (IsFleeing) Flee();
            else if (!_freighterSetup.FleeOnlyWhenDamaged)
            {
                List<InGame.MyDetectedEntityInfo> enemiesAround = LookForEnemies(_freighterSetup.FleeTriggerDistance, considerNeutralsAsHostiles: true);
                if (enemiesAround.Count <= 0) return;
                IsFleeing = true;
                Flee(enemiesAround);
            }

            if (!IsFleeing) return;

            if (GridPosition.DistanceTo(_endpoint) < 100) Shutdown();
        }

        private void Flee(List<InGame.MyDetectedEntityInfo> radarData = null)
        {
            if (!IsFleeing) return;

            if (!FleeTimersTriggered) TriggerFleeTimers();

            if (radarData == null) radarData = LookForEnemies(_freighterSetup.FleeTriggerDistance);
            if (radarData.Count == 0) return;

            InGame.MyDetectedEntityInfo closestEnemy = radarData.OrderBy(x => GridPosition.DistanceTo(x.Position)).FirstOrDefault();

            if (closestEnemy.IsEmpty())
            {
                WriteGeneral("Flee", "Cannot find closest hostile");
                return;
            }

            IMyEntity enemyEntity = MyAPIGateway.Entities.GetEntityById(closestEnemy.EntityId);
            if (enemyEntity == null)
            {
                WriteGeneral("Flee", "Cannot find enemy entity from closest hostile ID");
                return;
            }

            Vector3D fleePoint = GridPosition.InverseVectorTo(closestEnemy.Position, 100 * 1000);
            Rc.AddWaypoint(fleePoint, "Flee Point");
            (Rc as MyRemoteControl)?.ChangeFlightMode(InGame.FlightMode.OneWay);
            (Rc as MyRemoteControl)?.SetAutoPilotSpeedLimit(DetermineFleeSpeed());
            Rc.SetAutoPilotEnabled(true);
        }

        private void JumpAway()
        {
            List<IMyJumpDrive> jumpDrives = Term.GetBlocksOfType<IMyJumpDrive>(collect: x => x.IsWorking);

            if (jumpDrives.Count > 0) jumpDrives.First().Jump(false);
        }

        private void TriggerFleeTimers()
        {
            if (FleeTimersTriggered) return;

            var fleeTimers = new List<IMyTimerBlock>();
            
            Term.GetBlocksOfType(fleeTimers, x => x.IsFunctional && x.Enabled && (x.CustomName.Contains("Flee") || x.CustomData.Contains("Flee")));
            
            foreach (IMyTimerBlock timer in fleeTimers)
            {
                timer.Trigger();
            }

            FleeTimersTriggered = true;
        }

        private float DetermineFleeSpeed()
        {
            return Math.Min(_freighterSetup.FleeSpeedCap, _freighterSetup.FleeSpeedRatio * Rc.GetSpeedCap());
        }

        protected override bool ParseSetup()
        {
            if (ReadBotType(Rc) != BotType) return false;
            List<string> customData = Rc.CustomData.Trim().Replace("\r\n", "\n").Split('\n').ToList();
            foreach (string dataLine in customData)
            {
                if (dataLine.Contains("EEM_AI")) continue;
                if (dataLine.Contains("Type")) continue;
                string[] data = dataLine.Trim().Split(':');
                data[1] = data[1].Trim();
                switch (data[0].Trim())
                {
                    case "Faction":
                        break;
                    case "FleeOnlyWhenDamaged":
                        if (!bool.TryParse(data[1], out _freighterSetup.FleeOnlyWhenDamaged))
                        {
                            WriteGeneral("ParseSetup", "AI setup error: FleeOnlyWhenDamaged cannot be parsed");
                            return false;
                        }
                        break;
                    case "FleeTriggerDistance":
                        if (!float.TryParse(data[1], out _freighterSetup.FleeTriggerDistance))
                        {
                            WriteGeneral("ParseSetup", "AI setup error: FleeTriggerDistance cannot be parsed");
                            return false;
                        }
                        break;
                    case "FleeSpeedRatio":
                        if (!float.TryParse(data[1], out _freighterSetup.FleeSpeedRatio))
                        {
                            WriteGeneral("ParseSetup", "AI setup error: FleeSpeedRatio cannot be parsed");
                            return false;
                        }
                        break;
                    case "FleeSpeedCap":
                        if (!float.TryParse(data[1], out _freighterSetup.FleeSpeedCap))
                        {
                            WriteGeneral("ParseSetup", "AI setup error: FleeSpeedCap cannot be parsed");
                            return false;
                        }
                        break;
                    case "CruiseSpeed":
                        if (!float.TryParse(data[1], out _freighterSetup.CruiseSpeed))
                        {
                            WriteGeneral("ParseSetup", "AI setup error: CruiseSpeed cannot be parsed");
                            return false;
                        }
                        break;
                    default:
                        WriteGeneral("ParseSetup", $"AI setup error: Cannot parse '{dataLine}'");
                        return false;
                }
            }
            _freighterSetup.Default();
            return true;
        }
    }
}