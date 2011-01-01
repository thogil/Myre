using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Myre.Procedural.Tests
{
    public class OctreeObserver
        :Observer<Octree>
    {
        public Rectangle Rectangle
        {
            get;
            set;
        }

        public OctreeObserver()
        {
        }

        public override float Rate(Octree node)
        {
            Rectangle r = node.Rectangle;

            int x = overlap(node.Rectangle.Left, node.Rectangle.Right, Rectangle.Left, Rectangle.Right);
            int y = overlap(node.Rectangle.Top, node.Rectangle.Bottom, Rectangle.Top, Rectangle.Bottom);

            return x * y;
        }

        private int overlap(int aMin, int aMax, int bMin, int bMax)
        {
            if (aMin > bMax || bMin > aMax)
                return 0;

            return Math.Min(bMax, aMax) - Math.Max(bMin, aMin);
        }
    }
}
