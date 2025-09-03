using System;
using System.Collections.Concurrent;
using Eem.Thraxus.Common.Interfaces;

namespace Eem.Thraxus.Common.Generics
{
    internal class ObjectPool<T> where T : IReset, new()
    {
        private readonly Func<T> _objectGenerator;
        private readonly ConcurrentStack<T> _objects = new ConcurrentStack<T>();
        public long MaxNewObjects;

        public long TotalObjectsServed;

        public ObjectPool()
        {
        }

        public ObjectPool(Func<T> objectGenerator)
        {
            _objectGenerator = objectGenerator;
        }

        public int Count()
        {
            return _objects.Count;
        }

        public T Get()
        {
            T item;
            TotalObjectsServed++;
            if (_objects.TryPop(out item)) return item;
            MaxNewObjects++;
            return _objectGenerator == null ? new T() : _objectGenerator();
        }

        public void Return(T item)
        {
            if (item == null) return;
            if (!item.IsReset)
                item.Reset();
            _objects.Push(item);
        }

        public void Clear()
        {
            _objects.Clear();
        }

        public override string ToString()
        {
            return $"PoolType: [{typeof(T).Name}] Total Served: [{TotalObjectsServed:D4}] Max Created: [{MaxNewObjects:D4}] Current Pooled: [{Count():D4}]";
        }
    }
}

/*
 * Example Usage:
 * private GenericObjectPool<GrindOperation> _grindOperations = new GenericObjectPool<GrindOperation>(() => new GrindOperation(_userSettings));
 *
 * GrindOperation op =  _grindOperations.Get();
 *
        private void ClearGrindOperationsPool()
        {
            for (int i = _pooledGrindOperations.Count - 1; i >= 0; i--)
            {
                if (_pooledGrindOperations[i].Tick == TickCounter) continue;
                GrindOperation op = _pooledGrindOperations[i];
                _pooledGrindOperations.RemoveAtImmediately(i);
                op.OnWriteToLog -= WriteToLog;
                op.Reset();
                _grindOperations.Return(op);
            }
            _pooledGrindOperations.ApplyRemovals();
        }
 *
 * Make sure to clean up the object before returning it to the pool, else you may carry over obsolete / incorrect info
 *
 */