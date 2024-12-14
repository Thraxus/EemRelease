using System;
using System.Collections.Generic;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.Extensions;
using Eem.Thraxus.Common.Generics;
using Sandbox.ModAPI;
using VRage.Collections;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Bots
{
    public class BotDamageHandler : BaseLoggingClass
    {
        private readonly MyConcurrentList<long> _currentlyUnderAssault = new MyConcurrentList<long>();

        private readonly Dictionary<long, BotBase.OnDamageTaken> _damageHandlers = new Dictionary<long, BotBase.OnDamageTaken>();
        private ActionQueue _actionQueue;
        public event Action<long, long> OnTriggerWar;


        public virtual void TriggerWar(long assaulted, long assaulter)
        {
            WriteGeneral("TriggerWar", $"Asshats! [{assaulted.ToEntityIdFormat()}] [{assaulter.ToEntityIdFormat()}]");
            OnTriggerWar?.Invoke(assaulted, assaulter);
        }

        public void Init(ActionQueue actionQueue)
        {
            _actionQueue = actionQueue;
            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, DamageRefHandler);
            MyAPIGateway.Session.DamageSystem.RegisterAfterDamageHandler(0, GenericDamageHandler);
            MyAPIGateway.Session.DamageSystem.RegisterDestroyHandler(0, GenericDamageHandler);
        }

        public void AddDamageHandler(long gridId, BotBase.OnDamageTaken handler)
        {
            WriteGeneral("AddDamageHandler", $"Adding: {gridId.ToEntityIdFormat()}");
            _damageHandlers.Add(gridId, handler);
        }

        public void AddDamageHandler(IMyCubeGrid grid, BotBase.OnDamageTaken handler)
        {
            WriteGeneral("AddDamageHandler", $"Adding: {grid.EntityId.ToEntityIdFormat()}");
            AddDamageHandler(grid.GetTopMostParent().EntityId, handler);
        }

        public void RemoveDamageHandler(long gridId)
        {
            _damageHandlers.Remove(gridId);
        }

        public void RemoveDamageHandler(IMyCubeGrid grid)
        {
            RemoveDamageHandler(grid.GetTopMostParent().EntityId);
        }

        public bool HasDamageHandler(long gridId)
        {
            return _damageHandlers.ContainsKey(gridId);
        }

        public bool HasDamageHandler(IMyCubeGrid grid)
        {
            return HasDamageHandler(grid.GetTopMostParent().EntityId);
        }

        public void DamageRefHandler(object damagedObject, ref MyDamageInformation damage)
        {
            GenericDamageHandler(damagedObject, damage);
        }

        public void GenericDamageHandler(object damagedObject, MyDamageInformation damage)
        {
            if (damage.AttackerId == 0) return;
            if (damagedObject == null) return;
            if (damage.IsDeformation) return;
            if (damagedObject is IMyFloatingObject) return;

            //WriteGeneral(nameof(GenericDamageHandler), $"Damage!!! {damage.AttackerId:D25} {((IMySlimBlock)damagedObject).CubeGrid.GetTopMostParent().EntityId:D25} {_damageHandlers.ContainsKey(((IMySlimBlock)damagedObject).CubeGrid.GetTopMostParent().EntityId)}");
            try
            {
                var block = (IMySlimBlock)damagedObject;
                IMyCubeGrid damagedGrid = block.CubeGrid;
                long gridId = damagedGrid.GetTopMostParent().EntityId;

                if (_currentlyUnderAssault.Contains(gridId)) return;

                _currentlyUnderAssault.Add(gridId);
                _actionQueue.Add(10, () => _currentlyUnderAssault.Remove(gridId));

                BotBase.OnDamageTaken handler;
                if (!_damageHandlers.TryGetValue(gridId, out handler)) return;
                handler?.Invoke(block, damage);
            }
            catch (Exception e)
            {
                WriteGeneral("GenericDamageHandler", $"{e}");
            }
        }
    }
}