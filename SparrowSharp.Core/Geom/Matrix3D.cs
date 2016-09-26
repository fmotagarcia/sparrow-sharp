
using OpenTK;
using Sparrow.Pool;

namespace Sparrow.Geom
{

    public class Matrix3D : PooledObject
    {
        /// <summary>
        /// The underlying raw Matrix data. 
        /// WARNING: This is a struct, it gets passed by value!
        /// </summary>
        private Matrix4 rawData;

        private static readonly ObjectPool _pool = new ObjectPool(new CreateObject<PooledObject>(Init), 1000);

        private Matrix3D() { }

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

        public void Append(Matrix3D other)
        {
            rawData = rawData * other.rawData;
        }

        public void Prepend(Matrix3D other)
        {
            rawData = other.rawData * rawData;
        }

        public void CopyFrom(Matrix3D src)
        {
            rawData = src.rawData;
        }

        public void Invert()
        {
            rawData.Invert();
        }

        /// <summary>
        /// WARNING: raw data is a struct, it gets passed by value!
        /// </summary>
        public Matrix4 RawData
        {
            get { return rawData; }
        }

        public void CopyRawDataFrom(float[] arr)
        {
            rawData.Row0 = new Vector4(arr[0], arr[1], arr[2], arr[3]);
            rawData.Row1 = new Vector4(arr[4], arr[5], arr[6], arr[7]);
            rawData.Row2 = new Vector4(arr[8], arr[9], arr[10], arr[11]);
            rawData.Row3 = new Vector4(arr[12], arr[13], arr[14], arr[15]);
        }

        public void PrependTranslation(float x, float y, float z)
        {
            rawData.M41 += rawData.M11 * x + rawData.M21 * y + rawData.M31 * z;
            rawData.M42 += rawData.M12 * x + rawData.M22 * y + rawData.M32 * z;
            rawData.M43 += rawData.M13 * x + rawData.M23 * y + rawData.M33 * z;
            rawData.M44 += rawData.M14 * x + rawData.M24 * y + rawData.M34 * z;
        }

        public float M11
        {
            set { rawData.M11 = value; }
            get { return rawData.M11; }
        }

        public float M12
        {
            set { rawData.M12 = value; }
            get { return rawData.M12; }
        }

        public float M13
        {
            set { rawData.M13 = value; }
            get { return rawData.M13; }
        }

        public float M14
        {
            set { rawData.M14 = value; }
            get { return rawData.M14; }
        }


        public float M21
        {
            set { rawData.M21 = value; }
            get { return rawData.M21; }
        }

        public float M22
        {
            set { rawData.M22 = value; }
            get { return rawData.M22; }
        }

        public float M23
        {
            set { rawData.M23 = value; }
            get { return rawData.M23; }
        }

        public float M24
        {
            set { rawData.M24 = value; }
            get { return rawData.M24; }
        }


        public float M31
        {
            set { rawData.M31 = value; }
            get { return rawData.M31; }
        }

        public float M32
        {
            set { rawData.M32 = value; }
            get { return rawData.M32; }
        }

        public float M33
        {
            set { rawData.M33 = value; }
            get { return rawData.M33; }
        }

        public float M34
        {
            set { rawData.M34 = value; }
            get { return rawData.M34; }
        }


        public float M41
        {
            set { rawData.M41 = value; }
            get { return rawData.M41; }
        }

        public float M42
        {
            set { rawData.M42 = value; }
            get { return rawData.M42; }
        }

        public float M43
        {
            set { rawData.M43 = value; }
            get { return rawData.M43; }
        }

        public float M44
        {
            set { rawData.M44 = value; }
            get { return rawData.M44; }
        }

        /** Uses a matrix to transform 3D coordinates into a different space. If you pass a
         *  'resultVector', the result will be stored in this vector3D instead of creating a
         *  new object. */
        public float[] TransformCoords3D(float x, float y, float z)
        {
            float[] outV = new float[4];
            outV[0] = x * rawData.M11 + y * rawData.M21 + z * rawData.M31 + rawData.M41; // x
            outV[1] = x * rawData.M12 + y * rawData.M22 + z * rawData.M32 + rawData.M42; // y
            outV[2] = x * rawData.M13 + y * rawData.M23 + z * rawData.M33 + rawData.M43; // z
            outV[3] = x * rawData.M14 + y * rawData.M24 + z * rawData.M34 + rawData.M44; // w
            return outV;
        }

}
}
