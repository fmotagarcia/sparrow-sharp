using System;
using System.Collections.Concurrent;

namespace SparrowSharp.Pool
{
	public delegate T CreateObject<T> () where T : PooledObject;
	public delegate void ReturnObject<PooledObject> (PooledObject pooledObject);
	public class ObjectPool
	{
		private ConcurrentBag<PooledObject> _objects;
		private CreateObject<PooledObject> _createObject;

		public ObjectPool (CreateObject<PooledObject> createObject)
		{
			if (createObject == null) {
				throw new ArgumentNullException ("createObject");
			}
			_objects = new ConcurrentBag<PooledObject> ();
			_createObject = createObject;
		}

		public PooledObject GetObject ()
		{
			PooledObject item;
			if (!_objects.TryTake (out item)) {
				item = _createObject ();
				item.Init (new ReturnObject<PooledObject> (PutObject));
			}

			return item;
		}

		void PutObject (PooledObject item)
		{
			_objects.Add (item);
		}
	}
}
