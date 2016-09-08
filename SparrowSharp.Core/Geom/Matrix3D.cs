
using OpenTK;
using SparrowSharp.Pool;

namespace SparrowSharp.Core.Geom
{

    public class Matrix3D : PooledObject
    {
        /// <summary>
        /// The underlying raw Matrix data. 
        /// WARNING: This is a struct, it gets passed by value!
        /// </summary>
        private Matrix4 rawData;

        private static readonly ObjectPool _pool = new ObjectPool(new CreateObject<PooledObject>(Init), 1000);

        public static Matrix3D Create()
        {
            Matrix3D matrix = (Matrix3D)_pool.GetObject();
            matrix.rawData = Matrix4.Identity;
            return matrix;
        }

        public static Matrix3D Create(Matrix4 src)
        {
            Matrix3D matrix = (Matrix3D)_pool.GetObject();
            matrix.rawData = src;
            return matrix;
        }

        private static Matrix3D Init()
        {
            return new Matrix3D();
        }

        public void Identity()
        {
            rawData.Row0 = new Vector4(1, 0, 0, 0);
            rawData.Row1 = new Vector4(0, 1, 0, 0);
            rawData.Row2 = new Vector4(0, 0, 1, 0);
            rawData.Row3 = new Vector4(0, 0, 0, 1);
        }

        public void Prepend(Matrix3D other)
        {
            rawData = rawData * other.rawData;
        }

        public void CopyFrom(Matrix3D src)
        {
            rawData = src.rawData;
        }

        /// <summary>
        /// WARNING: raw data is a struct, it gets passed by value!
        /// </summary>
        public Matrix4 RawData
        {
            get { return rawData; }
        }
    }
}
