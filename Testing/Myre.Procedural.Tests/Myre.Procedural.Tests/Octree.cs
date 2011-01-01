using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Myre.Entities;

namespace Myre.Procedural.Tests
{
    public class Octree
        :ISceneNode<Octree>
    {
        public readonly Rectangle Rectangle;
        public readonly int Depth;

        private int frameNumber = -1;
        private float importance = 0;

        public Color Colour;

        static Random random = new Random();

        public Octree(Rectangle rectangle, Octree parent)
        {
            Colour = new Color(random.Next(255), random.Next(255), random.Next(255));

            this.Rectangle = rectangle;
            this.Depth = parent == null ? 0 : parent.Depth + 1;
            this.Parent = parent;

            Developed = false;
        }

        public float GetImportance(int frameNumber)
        {
            return frameNumber == this.frameNumber ? importance : 0;
        }

        public void AddToImportance(float delta, int frameNumber)
        {
            if (frameNumber != this.frameNumber)
                importance = delta;
            else
                importance += delta;

            this.frameNumber = frameNumber;
        }

        public Octree Parent
        {
            get;
            private set;
        }

        public void Develop(Scene scene)
        {
            if (Developed)
                throw new InvalidOperationException();

            int halfWidth = Rectangle.Width / 2;
            int halfHeight = Rectangle.Height / 2;

            children.Add(new Octree(new Rectangle(Rectangle.X, Rectangle.Y, halfWidth, halfHeight), this));
            children.Add(new Octree(new Rectangle(Rectangle.X + halfWidth, Rectangle.Y, halfWidth, halfHeight), this));
            children.Add(new Octree(new Rectangle(Rectangle.X + halfWidth, Rectangle.Y + halfHeight, halfWidth, halfHeight), this));
            children.Add(new Octree(new Rectangle(Rectangle.X, Rectangle.Y + halfHeight, halfWidth, halfHeight), this));

            Developed = true;
        }

        public void Diminish(Scene scene)
        {
            if (!Developed)
                throw new InvalidOperationException();

            children.Clear();

            Developed = false;
        }

        public bool Developed
        {
            get;
            private set;
        }

        private List<Octree> children = new List<Octree>();

        public IEnumerable<Octree> Children
        {
            get
            {
                if (!Developed)
                    throw new InvalidOperationException();
                return children;
            }
        }

        private List<Entity> entities = new List<Entity>();
        public IEnumerable<Entity> Entities
        {
            get
            {
                if (!Developed)
                    throw new InvalidOperationException();
                return entities;
            }
        }
    }
}
