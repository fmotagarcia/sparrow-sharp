
using Sparrow.Display;

namespace Sparrow.Fonts
{
    /// <summary>
    /// A text compositor arranges letters for Sparrow's TextField.
    /// </summary>
    public interface ITextCompositor
    {

        /// <summary>
        /// Draws the given text into a MeshBatch, using the supplied format and options. 
        /// </summary>
        void FillMeshBatch(MeshBatch meshBatch, float width, float height, string text,
                           TextFormat format, TextOptions options = null);

        /// <summary>
        /// Clears the MeshBatch (filled by the same class) and disposes any resources that
        /// are no longer needed.
        /// </summary>
        void ClearMeshBatch(MeshBatch meshBatch);

        /// <summary>
        /// Frees all resources allocated by the compositor.
        /// </summary>
        void Dispose();

    }
}
