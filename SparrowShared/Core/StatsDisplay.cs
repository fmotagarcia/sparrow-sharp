
using Sparrow.Display;
using Sparrow.Text;
using Sparrow.Styles;
using System.Diagnostics;
using Sparrow.Rendering;
using Sparrow.Utils;
using OpenGL;

namespace Sparrow.Core
{
    /** A small, lightweight box that displays the current framerate, memory consumption and
     *  the number of draw calls per frame. The display is updated automatically once per frame. */
    public class StatsDisplay : Sprite
    {
        private static readonly float UPDATE_INTERVAL = 0.5f;
        private static readonly float B_TO_MB = 1.0f / (1024f * 1024f); // convert from bytes to MB
        
        private Quad _background;
        private TextField _labels;
        private TextField _values;
        
        private int _frameCount = 0;
        private float _totalTime = 0;

        public float Fps = 0;
        public float Memory = 0;
        public float GpuMemory = 0;
        public int DrawCount = 0;
        private int _skipCount = 0;

        /// <summary>
        /// Constant used by a Desktop Nvidia experimental GPU info extension
        /// the returns the dedicated video memory (in kb) of the GPU memory
        /// </summary>
        private int GPU_MEMORY_INFO_DEDICATED_VIDMEM_NVX = 0x9047;
        private int GPU_MEMORY_INFO_CURRENT_AVAILABLE_VIDMEM_NVX = 0x9049;

        /** Creates a new Statistics Box. */
        public StatsDisplay()
        {
            string fontName = BitmapFont.MINI;
            float fontSize = BitmapFont.NATIVE_SIZE;
            uint fontColor  = 0xffffff;
            float width = 90;
            float height = 35;
            string gpuLabel = "\ngpu memory:";
            string labels = "frames/sec:\nstd memory:" + gpuLabel + "\ndraw calls:";

            _labels = new TextField(width, height, labels);
            _labels.Format.SetTo(fontName, fontSize, fontColor, HAlign.Left);
            _labels.Batchable = true;
            _labels.X = 2;

            _values = new TextField(width - 1, height, "");
            _values.Format.SetTo(fontName, fontSize, fontColor, HAlign.Right);
            _values.Batchable = true;

            _background = new Quad(width, height, 0x0);
            _background.Alpha = 0.75f;

            // make sure that rendering takes 2 draw calls
            if (_background.Style.Type != typeof(MeshStyle)) _background.Style = new MeshStyle();
            if (_labels.Style.Type != typeof(MeshStyle)) _labels.Style = new MeshStyle();
            if (_values.Style.Type != typeof(MeshStyle)) _values.Style = new MeshStyle();

            AddChild(_background);
            AddChild(_labels);
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

        /** Updates the displayed values. */
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

        /** Call this once in every frame that can skip rendering because nothing changed. */
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
            if (GLExtensions.DeviceSupportsOpenGLExtension("GL_NVX_gpu_memory_info"))
            {
                // this returns in Kb
                /*int dedicated;
                Gl.GetInteger((GetPName)GPU_MEMORY_INFO_DEDICATED_VIDMEM_NVX, out dedicated);

                int available;
                Gl.GetInteger((GetPName)GPU_MEMORY_INFO_CURRENT_AVAILABLE_VIDMEM_NVX, out available);

                return (dedicated - available) / 1024;*/
            }
            return 0;
        }


}
}

