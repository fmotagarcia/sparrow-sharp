using System.IO;
using Sparrow.Utils;

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

