using Sparrow.Display;
using System;
using System.Collections.Generic;

namespace Sparrow.Core
{
    public class ViewController
    {
        public Dictionary<string, Program> Programs { get; private set; }

        public int DrawableWidth { get; set; }

        public int DrawableHeight { get; set; }

        public Context Context { get; set; }

        public DisplayObject Root { get; set; }

        public Stage Stage { get; set; }

        //public Juggler Juggler { get; set; }

        public float ContentScaleFactor { get; set; }

        public RenderSupport RenderSupport { get; set; }

        private Type _rootClass;
        private float _contentScaleFactor = 1.0f; // hardcode for now
        private float _viewScaleFactor = 1.0f;

        public void RegisterProgram(string name, Program program)
        {
            Programs.Add(name, program);
        }

        public ViewController()
        {
            Setup();
        }

        public void UnregisterProgram(string name)
        {
            Programs.Remove(name);
        }

        public void Setup()
        {
            Programs = new Dictionary<string, Program>();

            Stage = new Stage();
            //Juggler = new Juggler();
            Context = new Context();
            // todo set current Context
    
            RenderSupport = new RenderSupport();
            
            SP.CurrentController = this;
        }

        public void Start(Type RootClass )
        {
            if (RootClass != null)
            {
                throw new Exception("Sparrow has already been started");
            }
            _rootClass = RootClass;
        }

        public void CreateRoot()
        {
            if (Root == null)
            {
                // hope iOS wont complain about such dynamic stuff
                Root = (DisplayObject)Activator.CreateInstance(_rootClass);

                if (Root.GetType().IsInstanceOfType(Stage) )
                {
                    throw new Exception("Root extends 'Stage' but is expected to extend 'Sprite' instead");
                }
                else
                {
                    Stage.addChild( Root );
                    /*
                    if (_onRootCreated)
                    {
                        _onRootCreated(_root);
                        SP_RELEASE_AND_NIL(_onRootCreated);
                    }*/
                }
            }
        }

    }
}

