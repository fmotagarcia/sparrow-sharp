using System;
using System.Collections.Concurrent;

namespace Sparrow.Pool
{
    public delegate T CreateObject<T>() where T : PooledObject;
    public delegate void ReturnObject<PooledObject>(PooledObject pooledObject);
    public class ObjectPool
    {
        private readonly ConcurrentQueue<PooledObject> _objects;
        private readonly CreateObject<PooledObject> _createObject;
        private readonly int _initialSize;
        private readonly int _maxBuffer;

        public ObjectPool(CreateObject<PooledObject> createObject, int initalSize = 0, int maxBuffer = 0)
        {
            if (createObject == null)
            {
                throw new ArgumentNullException("createObject");
            }

            _initialSize = initalSize;
            _maxBuffer = maxBuffer;

            _objects = new ConcurrentQueue<PooledObject>();
            _createObject = createObject;

            for (int i = 0; i < _initialSize; i++)
            {
                PooledObject item = _createObject();
                item.Init(new ReturnObject<PooledObject>(PutObject));

                _objects.Enqueue(item);
            }
        }

        public PooledObject GetObject()
        {
            PooledObject item;
            if (!_objects.TryDequeue(out item))
            {
                item = _createObject();
            }
            item.Init(new ReturnObject<PooledObject>(PutObject));
            return item;
        }

        private void PutObject(PooledObject item)
        {
            if (_maxBuffer == 0 || _maxBuffer > 0 && _objects.Count < _maxBuffer)
            {
                _objects.Enqueue(item);
            }
        }
    }
}
