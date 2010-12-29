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
        :Service, IBehaviourManager<Observer<N>>
        where N : ISceneNode<N>
    {
        #region fields
        private NComparer comparer = new NComparer();
        private List<N> nodes;
        public IEnumerable<N> Nodes
        {
            get
            {
                return nodes;
            }
        }

        private int capacity;
        public int Capacity
        {
            get
            {
                return capacity;
            }
            set
            {
                capacity = value;
            }
        }

        public readonly N SceneRoot;

        private List<Observer<N>> observers = new List<Observer<N>>();
        public IEnumerable<Observer<N>> Observers
        {
            get
            {
                return observers;
            }
        }

        public readonly Scene Scene;

        private ITaskExecutor executor;
        #endregion

        #region constructors
        public Procedural(Scene scene, N root)
            :this(scene, root, new SynchronousExecutor<N>())
        {
        }

        public Procedural(Scene scene, N root, ITaskExecutor executor)
        {
            this.executor = executor;
            this.nodes = new List<N>();
            this.Scene = scene;

            this.SceneRoot = root;
            nodes.Add(SceneRoot);
        }
        #endregion

        public void AddNode(N node)
        {
            nodes.Add(node);
        }

        public void RemoveNode(N Node)
        {
            nodes.Remove(Node);
        }

        public override void Update(float elapsedTime)
        {
            comparer.FrameNumber++;

            SortNodes();

            if (nodes.Count < Capacity)
                UnderCapacity();
            else if (nodes.Count > Capacity)
                OverCapacity();

            executor.ApplyQueuedUpdates(Scene, this);

            base.Update(elapsedTime);
        }

        private void OverCapacity()
        {
            if (executor.DiminishCount == 0)
            {
                for (int i = 0; i < nodes.Count && (executor.DiminishCount + nodes.Count) > Capacity; i++)
                {
                    var n = nodes[i];
                    if (n.Developed && !executor.IsDiminishing(n))
                        executor.QueueDiminish(n);
                }
            }
        }

        private void UnderCapacity()
        {
            if (executor.DevelopCount == 0)
            {
                for (int i = nodes.Count - 1; i >= 0 && (executor.DevelopCount + nodes.Count) < Capacity; i--)
                {
                    var n = nodes[i];
                    if (!n.Developed && !executor.IsDeveloping(n))
                        executor.QueueDevelop(n);
                }
            }
        }

        private void SortNodes()
        {
            foreach (var observer in observers)
            {
                foreach (var node in nodes)
                {
                    float rating = observer.Rate(node);
                    node.AddToImportance(rating, comparer.FrameNumber);
                }
            }

            nodes.Sort(comparer);
        }

        #region behaviour manager
        public void Add(Observer<N> behaviour)
        {
            observers.Add(behaviour);
        }

        public bool Remove(Observer<N> behaviour)
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

        public interface ITaskExecutor
        {
            int DevelopCount
            {
                get;
            }

            int DiminishCount
            {
                get;
            }

            void QueueDevelop(N node);

            bool IsDeveloping(N node);

            void QueueDiminish(N node);

            bool IsDiminishing(N node);

            void ApplyQueuedUpdates(Scene scene, Procedural<N> manager);
        }
    }
}
