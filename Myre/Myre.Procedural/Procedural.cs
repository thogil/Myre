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
using Myre.Entities.Services;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Collections;

namespace Myre.Procedural
{
    /// <summary>
    /// A service which automatically creates and destroys entities in a scene
    /// </summary>
    public class Procedural<N>
        :Service, IBehaviourManager<Observer>
        where N : ISceneNode
    {
        private NComparer comparer = new NComparer();
        private MinMaxHeap<N> heap;

        public readonly N SceneRoot;

        private List<Observer> observers = new List<Observer>();
        public IEnumerable<Observer> Observers
        {
            get
            {
                return observers;
            }
        }

        public Procedural(Scene scene, N root)
        {
            this.heap = new MinMaxHeap<N>(comparer);

            this.SceneRoot = root;
            heap.Add(SceneRoot);
        }

        public void AddNode(N node)
        {
            heap.Add(node);
        }

        public void RemoveNode(N Node)
        {
            heap.Remove(Node);
        }

        public override void Update(float elapsedTime)
        {
            comparer.FrameNumber++;

            foreach (var observer in observers)
            {
                if (observer.Changed)
                {
                    observer.Changed = false;

                    throw new NotImplementedException();
                }
            }

            base.Update(elapsedTime);
        }

        #region behaviour manager
        public void Add(Observer behaviour)
        {
            observers.Add(behaviour);
        }

        public bool Remove(Observer behaviour)
        {
            return observers.Remove(behaviour);
        }
        #endregion

        private class NComparer
            :IComparer<N>
        {
            public int FrameNumber;

            public int Compare(N x, N y)
            {
                return x.GetImportance(FrameNumber).CompareTo(y.GetImportance(FrameNumber));
            }
        }
    }
}
