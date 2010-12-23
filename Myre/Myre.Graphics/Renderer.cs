﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Services;
using Microsoft.Xna.Framework.Graphics;
using Myre.Collections;
using Microsoft.Xna.Framework;
using Myre.Debugging.Statistics;
using Myre.Graphics.Lighting;
using Microsoft.Xna.Framework.Content;
using Myre.Graphics.Geometry;
using Myre.Entities;

namespace Myre.Graphics
{
    public class Renderer
        : Service
    {
        struct Resource
        {
            public string Name;
            public RenderTarget2D Target;
        }

        private Dictionary<string, RenderTarget2D> activeResources;
        private RendererMetadata data;
        private RendererSettings settings;
        private Queue<RenderTarget2D> viewResults;

        private GraphicsDevice device;
        private Quad quad;
        //private Effect colourCorrection;
        private SpriteBatch spriteBatch;

        private RenderPlan plan;

        private Scene scene;
        private List<ILightProvider> lights;
        private List<IGeometryProvider> geometry;
        private List<View> views;

        public GraphicsDevice Device
        {
            get { return device; }
        }

        public Scene Scene
        {
            get { return scene; }
        }

        public RendererMetadata Data
        {
            get { return data; }
        }

        public RendererSettings Settings
        {
            get { return settings; }
        }

        public List<View> Views
        {
            get { return views; }
        }

        public List<IGeometryProvider> Geometry
        {
            get { return geometry; }
        }

        public List<ILightProvider> Lights
        {
            get { return lights; }
        }

        public RenderPlan Plan
        {
            get { return plan; }
            set
            {
                plan = value;
                //plan.Apply();
            }
        }

        public Renderer(GraphicsDevice device, ContentManager content, Scene scene)
        {
            this.device = device;
            this.scene = scene;
            this.data = new RendererMetadata();
            this.settings = new RendererSettings(this);
            this.activeResources = new Dictionary<string, RenderTarget2D>();
            this.viewResults = new Queue<RenderTarget2D>();
            this.views = new List<View>();
            this.geometry = new List<IGeometryProvider>();
            this.lights = new List<ILightProvider>();
            this.quad = new Quad(device);
            //this.colourCorrection = content.Load<Effect>("Gamma");
            this.spriteBatch = new SpriteBatch(device);
        }

        public override void Update(float elapsedTime)
        {
            data.Set<float>("timedelta", elapsedTime);
            base.Update(elapsedTime);
        }

        public override void Draw()
        {
#if PROFILE
            Statistic.Get("Graphics.Primitives").Value = 0;
            Statistic.Get("Graphics.Draws").Value = 0;
#endif

            for (int i = 0; i < views.Count; i++)
            {
                views[i].SetMetadata(data);
                var output = Plan.Execute(this);

                activeResources.Clear();

                viewResults.Enqueue(output);
            }

            device.SetRenderTarget(null);

            spriteBatch.Begin();
            //colourCorrection.Parameters["Resolution"].SetValue(data.Get<Vector2>("resolution").Value);
            for (int i = 0; i < views.Count; i++)
            {
                var output = viewResults.Dequeue();
                var viewport = views[i].Viewport;

                //colourCorrection.Parameters["Texture"].SetValue(output);
                //quad.SetPosition(viewport.Bounds);
                //quad.Draw(colourCorrection);
                spriteBatch.Draw(output, viewport.Bounds, Color.White);

                RenderTargetManager.RecycleTarget(output);
            }
            spriteBatch.End();

            base.Draw();
        }

        public void SetResource(string name, RenderTarget2D target)
        {
            activeResources[name] = target;
            data.Set<Texture2D>(name, target);
        }

        internal RenderTarget2D GetResource(string name)
        {
            return activeResources[name];
        }

        public void FreeResource(string name)
        {
            RenderTarget2D target;
            if (activeResources.TryGetValue(name, out target))
            {
                RenderTargetManager.RecycleTarget(target);
                data.Set<Texture2D>(name, null);
                activeResources.Remove(name);
            }
        }
    }
}