using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Myre.Entities;
using Ninject;
using Myre.Entities.Ninject;

namespace Myre.Procedural.Tests
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Scene scene;
        IKernel kernel;
        Octree root;

        OctreeObserver observer;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            kernel = new StandardKernel(new EntityNinjectModule());
            scene = new Scene(kernel);

            root = new Octree(new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), null);

            //todo: find some automatic way to do this
            kernel.Bind<Procedural<Octree>>().ToConstant(new Procedural<Octree>(scene, root, 24));

            var service = scene.GetService<Procedural<Octree>>();
            var manager = scene.GetManager<Procedural<Octree>>();

            bool same = object.ReferenceEquals(service, manager);
            if (!same)
                throw new InvalidOperationException("Manager and service should be the same!");

            EntityDescription d = new EntityDescription(kernel);
            d.AddBehaviour<OctreeObserver>("observer");
            Entity e = d.Create();
            observer = e.GetBehaviour<OctreeObserver>("observer");
            scene.Add(e);

            observer.Rectangle = new Rectangle(0, 0, 150, 150);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            Window.Title = scene.GetService<Procedural<Octree>>().Nodes.Count().ToString();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            scene.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            KeyboardState k = Keyboard.GetState();
            if (k.IsKeyDown(Keys.Left))
                observer.Rectangle = new Rectangle(observer.Rectangle.X - 5, observer.Rectangle.Y, observer.Rectangle.Width, observer.Rectangle.Height);
            else if (k.IsKeyDown(Keys.Right))
                observer.Rectangle = new Rectangle(observer.Rectangle.X + 5, observer.Rectangle.Y, observer.Rectangle.Width, observer.Rectangle.Height);
            if (k.IsKeyDown(Keys.Up))
                observer.Rectangle = new Rectangle(observer.Rectangle.X, observer.Rectangle.Y - 5, observer.Rectangle.Width, observer.Rectangle.Height);
            else if (k.IsKeyDown(Keys.Down))
                observer.Rectangle = new Rectangle(observer.Rectangle.X, observer.Rectangle.Y + 5, observer.Rectangle.Width, observer.Rectangle.Height);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            foreach (var item in EnumerateLeaves(root))
                spriteBatch.Draw(Content.Load<Texture2D>("WhitePixel"), item.Rectangle, item.Colour);

            spriteBatch.Draw(Content.Load<Texture2D>("WhitePixel"), observer.Rectangle, new Color(200, 200, 200, 150));
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private IEnumerable<Octree> EnumerateLeaves(Octree node)
        {
            if (node.Developed)
            {
                foreach (var child in node.Children)
                    foreach (var item in EnumerateLeaves(child))
                        yield return item;
            }
            else
            {
                yield return node;
            }
        }
    }
}
