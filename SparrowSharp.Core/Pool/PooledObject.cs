using System;

namespace SparrowSharp.Pool
{
    public class PooledObject
    {
        private ReturnObject<PooledObject> _returnObject;

        public void Init(ReturnObject<PooledObject> returnObject)
        {
            _returnObject = returnObject;
        }

        ~PooledObject ()
        {
            if (_returnObject != null)
            {
                _returnObject(this);
                GC.ReRegisterForFinalize(this);
            }
        }
    }
}

