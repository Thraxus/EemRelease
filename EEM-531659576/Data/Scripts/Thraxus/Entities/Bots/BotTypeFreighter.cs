using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Common.Extensions;
using Eem.Thraxus.Enums;
using Eem.Thraxus.Extensions;
using Eem.Thraxus.Models;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
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
            //WriteGeneral(nameof(TriggerAlert), $"Alert Triggered! [{assaulted.ToEntityIdFormat()}] [{blockId.ToEntityIdFormat()}] [{assaulter.ToEntityIdFormat()}]");
            IsFleeing = true;
            Flee();
        }

        //private void DamageHandler(IMySlimBlock block, MyDamageInformation damage)
        //{
        //    try
        //    {
        //        IMyPlayer damager;
        //        ReactOnDamage(damage, out damager);
        //        if (damager == null) return;
        //        IsFleeing = true;
        //        Flee();
        //    }
        //    catch (Exception scrap)
        //    {
        //        WriteGeneral("DamageHandler", scrap.Message);
        //    }
        //}

        public override void Main()
        {
            if (IsFleeing)
            {
                Flee();
                return;
            }
            
            if (!_freighterSetup.FleeOnlyWhenDamaged)
            {
                List<InGame.MyDetectedEntityInfo> enemiesAround = LookForEnemies(_freighterSetup.FleeTriggerDistance, true);
                if (enemiesAround.Count <= 0) return;
                IsFleeing = true;
                Flee(enemiesAround);
                return;
            }

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

        private void TriggerFleeTimers()
        {
            if (FleeTimersTriggered) return;

            var fleeTimers = new List<IMyTimerBlock>();

            Term.GetBlocksOfType(fleeTimers, x => x.IsFunctional && x.Enabled && (x.CustomName.Contains("Flee") || x.CustomData.Contains("Flee")));

            foreach (IMyTimerBlock timer in fleeTimers) timer.Trigger();

            FleeTimersTriggered = true;
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