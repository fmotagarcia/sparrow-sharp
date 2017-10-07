
using Sparrow.Display;
using Sparrow.Text;
using Sparrow.Styles;
using System.Diagnostics;
using Sparrow.Rendering;
using Sparrow.Utils;
using OpenGL;

namespace Sparrow.Core
{
    /// <summary>
    /// A small, lightweight box that displays the current framerate, memory consumption and
    /// the number of draw calls per frame.The display is updated automatically once per frame.
    /// </summary>
    public class StatsDisplay : Sprite
    {
        private static readonly float UPDATE_INTERVAL = 0.5f;
        private static readonly float B_TO_MB = 1.0f / (1024f * 1024f); // convert from bytes to MB
        
        private readonly Quad _background;
        private readonly TextField _values;
        
        private int _frameCount;
        private float _totalTime;

        public float Fps;
        public float Memory;
        public float GpuMemory;
        public int DrawCount = 0;
        private int _skipCount;

        /// <summary>
        /// Creates a new Statistics Box.
        /// </summary>
        public StatsDisplay()
        {
            const string fontName = BitmapFont.MINI;
            float fontSize = BitmapFont.NATIVE_SIZE;
            const uint fontColor = 0xffffff;
            const float width = 90;
            const float height = 35;
            const string gpuLabel = "\ngpu memory:";
            const string labels = "frames/sec:\nstd memory:" + gpuLabel + "\ndraw calls:";

            var labels1 = new TextField(width - 2, height, labels);
            labels1.Format.SetTo(fontName, fontSize, fontColor, HAlign.Left);
            labels1.Batchable = true;
            labels1.X = 2;

            _values = new TextField(width - 1, height);
            _values.Format.SetTo(fontName, fontSize, fontColor, HAlign.Right);
            _values.Batchable = true;

            _background = new Quad(width, height, 0x0);
            _background.Alpha = 0.7f;

            // make sure that rendering takes 2 draw calls
            if (_background.Style.Type != typeof(MeshStyle)) _background.Style = new MeshStyle();
            if (labels1.Style.Type != typeof(MeshStyle)) labels1.Style = new MeshStyle();
            if (_values.Style.Type != typeof(MeshStyle)) _values.Style = new MeshStyle();

            AddChild(_background);
            AddChild(labels1);
            AddChild(_values);
            
            AddedToStage += OnAddedToStage;
            RemovedFromStage += OnRemovedFromStage;
        }

        private void OnAddedToStage(DisplayObject target, DisplayObject currentTarget)
        {
            EnterFrame += OnEnterFrame;
            _totalTime = _frameCount = _skipCount = 0;
            Update();
        }

        private void OnRemovedFromStage(DisplayObject target, DisplayObject currentTarget)
        {
            EnterFrame -= OnEnterFrame;
        }

        private void OnEnterFrame(DisplayObject target, float passedTime)
        {
            _totalTime += (passedTime / 1000);
            _frameCount++;
            
            if (_totalTime > UPDATE_INTERVAL)
            {
                Update();
                _frameCount = _skipCount = 0;
                _totalTime = 0;
            }
        }

        /// <summary>
        /// Updates the displayed values.
        /// </summary>
        public void Update()
        {
            _background.Color = _skipCount > (_frameCount / 2) ? (uint)0x003F00 : 0x0;
            Fps = _totalTime > 0 ? _frameCount / _totalTime : 0;
            Process currentProc = Process.GetCurrentProcess();
            Memory = currentProc.PrivateMemorySize64 * B_TO_MB;
            GpuMemory = GetGPUMemory();

            string fpsText = Fps < 100 ? Fps.ToString("N1") : Fps.ToString("N0");
            string memText = Memory < 100 ? Memory.ToString("N1") : Memory.ToString("N0");
            string gpuMemText = GpuMemory < 100 ? GpuMemory.ToString("N1") : GpuMemory.ToString("N0");
            string drwText = (_totalTime > 0 ? DrawCount-2 : DrawCount).ToString(); // ignore self

            _values.Text = fpsText + "\n" + memText + "\n" +
                (GpuMemory >= 0 ? gpuMemText + "\n" : "") + drwText;
        }

        /// <summary>
        /// Call this once in every frame that can skip rendering because nothing changed.
        /// </summary>
        public void MarkFrameAsSkipped()
        {
            _skipCount += 1;
        }

        public override void Render(Painter painter)
        {
            // By calling 'finishQuadBatch' and 'excludeFromCache', we can make sure that the stats
            // display is always rendered with exactly two draw calls. That is taken into account
            // when showing the drawCount value (see 'ignore self' comment above)
            painter.ExcludeFromCache(this);
            painter.FinishMeshBatch();
            base.Render(painter);
        }

        /// <summary>
        /// Returns the currently used GPU memory in bytes. Might not work in all platforms!
        /// </summary>
        private int GetGPUMemory()
        {
#if __WINDOWS__
            if (GLExtensions.DeviceSupportsOpenGLExtension("GL_NVX_gpu_memory_info"))
            {
                // this returns in Kb, Nvidia only extension
                int dedicated;
                Gl.Get(Gl.GPU_MEMORY_INFO_DEDICATED_VIDMEM_NVX, out dedicated);

                int available;
                Gl.Get(Gl.GPU_MEMORY_INFO_CURRENT_AVAILABLE_VIDMEM_NVX, out available);

                return (dedicated - available) / 1024;
            }
#endif
            return 0;
        }

}
}

