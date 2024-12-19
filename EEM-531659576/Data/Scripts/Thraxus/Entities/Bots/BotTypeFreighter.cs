using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Common.Extensions;
using Eem.Thraxus.Enums;
using Eem.Thraxus.Extensions;
using Eem.Thraxus.Models;
using Sandbox.Game.Entities;
using SpaceEngineers.Game.ModAPI;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRageMath;
using IMyRemoteControl = Sandbox.ModAPI.IMyRemoteControl;
using InGame = Sandbox.ModAPI.Ingame;


namespace Eem.Thraxus.Entities.Bots
{
    public sealed class BotTypeFreighter : BotBase
    {
        public static readonly BotType BotType = BotType.Freighter;
        private Vector3D _endpoint;

        private FreighterSettings _freighterSetup;

        private Vector3D _startPoint;

        public BotTypeFreighter(IMyCubeGrid grid, BotConfig botConfig)
            : base(grid, botConfig) { }

        private bool IsFleeing { get; set; }

        private bool FleeTimersTriggered { get; set; }

        public override bool Init(IMyRemoteControl rc)
        {
            if (!base.Init(rc)) return false;
            if (Rc == null) return false;
            WriteGeneral("Init", "Bot Freighter Booting...");
            //OnDamaged += DamageHandler;
            //OnBlockPlaced += BlockPlacedHandler;
            //_startPoint = GridPosition;
            SetFlightPath();

            return true;
        }

        private void SetFlightPath()
        {
            WriteGeneral("SetFlightPath", $"GP: {GridPosition}");
            WriteGeneral("SetFlightPath", $"SP: {(float)Rc.GetShipSpeed()}");
            WriteGeneral("SetFlightPath", $"SV: [{Rc.DampenersOverride.ToSingleChar()}] {Rc.GetShipVelocities().LinearVelocity}");
            
            _startPoint = GridPosition;
            
            var speed = (float)Rc.GetShipSpeed();
            
            Vector3D velocity = Rc.GetShipVelocities().LinearVelocity;
            
            if (speed > 5)
                _endpoint = GridPosition + Vector3D.Normalize(velocity) * 30000;
            else
                _endpoint = GridPosition + Rc.WorldMatrix.Forward * 30000;

            (Rc as MyRemoteControl)?.SetAutoPilotSpeedLimit(_freighterSetup.CruiseSpeed);
            (Rc as MyRemoteControl)?.SetCollisionAvoidance(true);
        }
        
        public override void TriggerAlert()
        {
            if (IsFleeing)
            {
                return;
            }
            Flee();
        }

        public override void Main()
        {
            base.Main();
            if (IsFleeing)
            {
                Flee();
                return;
            }
            
            if (!_freighterSetup.FleeOnlyWhenDamaged)
            {
                var targets = FindTargets(_freighterSetup.FleeTriggerDistance, true);
                if (targets.Any())
                {
                    Flee(targets);
                    return;
                }
            }

            if (GridPosition.DistanceTo(_endpoint) < 100) Shutdown();
        }

        private HashSet<IMyTimerBlock> _timers = new HashSet<IMyTimerBlock>();

        private void Flee(HashSet<MyEntity> targets = null)
        {
            IsFleeing = true;
            if (targets == null)
            {
                targets = FindTargets(_freighterSetup.FleeTriggerDistance, true);
            }

            if (targets == null)
            {
                IsFleeing = false;
                return;
            }

            MyEntity closestEntity = GetClosestEntity(targets);
            if (closestEntity != null)
            {
                SetFleePath(closestEntity);
            }

            if (!FleeTimersTriggered)
            {
                TriggerFleeTimers();
            }
        }

        private void SetFleePath(MyEntity closestEntity)
        {
            Vector3D fleePoint = GridPosition.InverseVectorTo(closestEntity.PositionComp.GetPosition(), 100 * 1000);
            Rc.AddWaypoint(fleePoint, "Flee Point");
            Rc.FlightMode = InGame.FlightMode.OneWay;
            Rc.SpeedLimit = DetermineFleeSpeed();
            Rc.SetAutoPilotEnabled(true);
        }

        private void TriggerFleeTimers()
        {
            FleeTimersTriggered = true;

            _timers.Clear();
            _timers = Rc.CubeGrid.GetFatBlocks<IMyTimerBlock>().ToHashSet();

            foreach (var timer in _timers)
            {
                if (timer.Enabled && timer.IsFunctional && (timer.CustomName.Contains("Flee") || timer.CustomData.Contains("Flee")))
                {
                    timer.Trigger();
                }
            }
        }

        private float DetermineFleeSpeed()
        {
            return Math.Min(_freighterSetup.FleeSpeedCap, _freighterSetup.FleeSpeedRatio * Rc.GetSpeedCap());
        }

        protected override void ParseSetup()
        {
            _freighterSetup.FleeOnlyWhenDamaged = BotConfig.FleeOnlyWhenDamaged;
            _freighterSetup.FleeTriggerDistance = BotConfig.FleeTriggerDistance;
            _freighterSetup.FleeSpeedCap = BotConfig.FleeSpeedCap;

            _freighterSetup.Default();
        }

        private struct FreighterSettings
        {
            public bool FleeOnlyWhenDamaged;
            public float FleeTriggerDistance;
            public float FleeSpeedRatio;
            public float FleeSpeedCap;
            public float CruiseSpeed;

            public void Default()
            {
                FleeSpeedRatio = 1.0f;
                CruiseSpeed = 50;
            }
        }
    }
}