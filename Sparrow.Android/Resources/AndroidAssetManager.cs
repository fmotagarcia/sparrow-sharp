using Sparrow.Utils;
using System.IO;

namespace Sparrow.Android
{
    public class AndroidAssetManager : IAssetManager
    {
        #region AssetManager implementation

        public string GetResourcesPath()
        {
            return Path.Combine(System.Environment.CurrentDirectory, "Resources");
        }

        #endregion
    }
}

