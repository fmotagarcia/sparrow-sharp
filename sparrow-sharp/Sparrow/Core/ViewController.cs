using Android.Util;
using OpenTK;
using OpenTK.Graphics.ES20;
using OpenTK.Platform.Android;
using Sparrow.Display;
using System;
using System.Collections.Generic;
using sparrowsharp;
using Sparrow.Geom;
using OpenTK.Graphics;

namespace Sparrow.Core
{
    public class ViewController : AndroidGameView
    {
        public Dictionary<string, Program> Programs { get; private set; }

        public int DrawableWidth { get; set; }

        public int DrawableHeight { get; set; }

        public Context SPContext { get; set; }

        public DisplayObject Root { get; set; }

        public Stage Stage { get; set; }
        //public Juggler Juggler { get; set; }
        public float ContentScaleFactor { get; set; }

        public RenderSupport RenderSupport { get; set; }

        private Type _rootClass;
        private float _contentScaleFactor = 1.0f;
        // hardcode for now
        private float _viewScaleFactor = 1.0f;

        public ViewController(Android.Content.Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Setup();
        }

        public ViewController(IntPtr handle, Android.Runtime.JniHandleOwnership transfer) : base(handle, transfer)
        {
            Setup();
        }

        public ViewController(Android.Content.Context context) : base(context)
        {
            Setup();
        }

        public void RegisterProgram(string name, Program program)
        {
            Programs.Add(name, program);
        }

        public void UnregisterProgram(string name)
        {
            Programs.Remove(name);
        }

        public void Setup()
        {

        }
        // This gets called when the drawing surface is ready
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ReadjustStageSize(); // TODO check if Width/Height are not 0 here
            CreateRoot();
            // Run the render loop
            Run();
        }
        // This method is called everytime the context needs
        // to be recreated. Use it to set any egl-specific settings
        // prior to context creation
        //
        // In this particular case, we demonstrate how to set
        // the graphics mode and fallback in case the device doesn't
        // support the defaults
        protected override void CreateFrameBuffer()
        {
            // TODO some init calls?
			ContextRenderingApi = GLVersion.ES2;

            // the default GraphicsMode that is set consists of (16, 16, 0, 0, 2, false)
            try
            {
                Log.Verbose("GLCube", "Loading with default settings");

                // if you don't call this, the context won't be created
                base.CreateFrameBuffer();

				Programs = new Dictionary<string, Program>();

				Stage = new Stage();
				Stage.Color = 0x23FF23;
				//Juggler = new Juggler();
				SPContext = new Context(GraphicsContext);
				SP.CurrentController = this;
				SP.Context = SPContext;
				// Context.setCurrentContext() ??
				RenderSupport = new RenderSupport();

                return;
            }
            catch (Exception ex)
            {
                Log.Verbose("GLCube", "{0}", ex);
            }
            // this is a graphics setting that sets everything to the lowest mode possible so
            // the device returns a reliable graphics setting.
            try
            {
                Log.Verbose("GLCube", "Loading with custom Android settings (low mode)");
                GraphicsMode = new AndroidGraphicsMode(0, 0, 0, 0, 0, false); // TODO this is for GL 1.1

                // if you don't call this, the context won't be created
                base.CreateFrameBuffer();
                return;
            }
            catch (Exception ex)
            {
                Log.Verbose("GLCube", "{0}", ex);
            }
            throw new Exception("Can't load egl, aborting");
        }
        // This gets called on each frame render
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
			//MakeCurrent();
            // same as (void)glkView:(GLKView *)view drawInRect:(CGRect)rect ??
            SP.CurrentController = this;  
            //??? neded? Context.SetCurrentContext(_context);
            GL.Disable(All.CullFace);
            GL.Disable(All.DepthTest);
            GL.Disable(All.Blend);

			RenderSupport.NextFrame();
			//RenderSupport.ClearWithColor(0xff02d3, 1.0f);

			Stage.Render(RenderSupport);

			RenderSupport.FinishQuadBatch();

            #if DEBUG
            RenderSupport.CheckForOpenGLError();
            #endif
			//Console.WriteLine("Number of draw calls: " + RenderSupport.NumDrawCalls);
            // you only need to call this if you have delegates
            // registered that you want to have called

            SwapBuffers();
        }

        public void Start(Type RootClass)
        {
            if (_rootClass != null)
            {
                throw new Exception("Sparrow has already been started");
            }
            _rootClass = RootClass;
        }

        public void CreateRoot()
        {
            if (Root == null)
            {
                // FIXME: not worried about this for now let's just make it work
//                // hope iOS wont complain about such dynamic stuff
//                Root = (DisplayObject)Activator.CreateInstance(_rootClass);
//
//                if (Root.GetType().IsInstanceOfType(Stage))
//                {
//                    throw new Exception("Root extends 'Stage' but is expected to extend 'Sprite' instead");
//                }
//                else
//                {
//                    Stage.AddChild(Root);
//                    /*
//                    if (_onRootCreated)
//                    {
//                        _onRootCreated(_root);
//                        SP_RELEASE_AND_NIL(_onRootCreated);
//                    }*/
//                }

                Stage.AddChild(new SampleGame());
            }
        }

        private void ReadjustStageSize()
        {
            Stage.Width = Width * _viewScaleFactor / _contentScaleFactor;
            Stage.Height = Height * _viewScaleFactor / _contentScaleFactor;
        }

		// this is called whenever android raises the SurfaceChanged event
		protected override void OnResize(EventArgs e)
		{
			// the surface change event makes your context
			// not be current, so be sure to make it current again
			MakeCurrent();
			ReadjustStageSize();
		}

		//////////////////////////////////////////////////////////////// TEMP CODE
		/*
        int viewportWidth, viewportHeight;
        int program;
        float[] vertices;

        protected override void OnLoad(EventArgs e)
        {
            // This is completely optional and only needed
            // if you've registered delegates for OnLoad
            base.OnLoad(e);

            viewportHeight = Height; viewportWidth = Width;

            // Vertex and fragment shaders
            string vertexShaderSrc = "attribute vec4 vPosition;    \n" +
                              "void main()                  \n" +
                              "{                            \n" +
                              "   gl_Position = vPosition;  \n" +
                              "}                            \n";

            string fragmentShaderSrc = "precision mediump float;\n" +
                                   "void main()                                  \n" +
                                   "{                                            \n" +
                                   "  gl_FragColor = vec4 (1.0, 0.0, 0.0, 1.0);  \n" +
                                   "}                                            \n";

            int vertexShader = LoadShader(All.VertexShader, vertexShaderSrc);
            int fragmentShader = LoadShader(All.FragmentShader, fragmentShaderSrc);
            program = GL.CreateProgram();
            if (program == 0)
                throw new InvalidOperationException("Unable to create program");

            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);

            GL.BindAttribLocation(program, 0, "vPosition");
            GL.LinkProgram(program);

            int linked = 0;
            GL.GetProgram(program, All.LinkStatus, out linked);
            if (linked == 0)
            {
                // link failed
                int length = 0;
                GL.GetProgram(program, All.InfoLogLength, out length);
                if (length > 0)
                {
                    var log = new StringBuilder(length);
                    GL.GetProgramInfoLog(program, length, out length, log);
                    Log.Debug("GL2", "Couldn't link program: " + log.ToString());
                }

                GL.DeleteProgram(program);
                throw new InvalidOperationException("Unable to link program");
            }

            RenderTriangle();
        }

        int LoadShader(All type, string source)
        {
            int shader = GL.CreateShader(type);
            if (shader == 0)
                throw new InvalidOperationException("Unable to create shader");

            int length = 0;
            GL.ShaderSource(shader, 1, new string[] { source }, (int[])null);
            GL.CompileShader(shader);

			int compiled = 0;
            GL.GetShader(shader, All.CompileStatus, out compiled);
            if (compiled == 0)
            {
                length = 0;
                GL.GetShader(shader, All.InfoLogLength, out length);
                if (length > 0)
                {
                    var log = new StringBuilder(length);
                    GL.GetShaderInfoLog(shader, length, out length, log);
                    Log.Debug("GL2", "Couldn't compile shader: " + log.ToString());
                }

                GL.DeleteShader(shader);
                throw new InvalidOperationException("Unable to compile shader of type : " + type.ToString());
            }

            return shader;
        }

        void RenderTriangle()
        {
            vertices = new float[] {
					0.0f, 0.5f, 0.0f,
					-0.5f, -0.5f, 0.0f,
					0.5f, -0.5f, 0.0f
				};

            GL.ClearColor(0.7f, 0.7f, 0.7f, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.Viewport(0, 0, viewportWidth, viewportHeight);
            GL.UseProgram(program);

            GL.VertexAttribPointer(0, 3, All.Float, false, 0, vertices);
            GL.EnableVertexAttribArray(0);

            GL.DrawArrays(All.Triangles, 0, 3);

            SwapBuffers();
        }

        // this is called whenever android raises the SurfaceChanged event
        protected override void OnResize(EventArgs e)
        {
            viewportHeight = Height;
            viewportWidth = Width;

            // the surface change event makes your context
            // not be current, so be sure to make it current again
            MakeCurrent();
            RenderTriangle();
        }*/

    }
}

