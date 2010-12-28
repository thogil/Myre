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
        where N : ISceneNode
    {
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

        private HashSet<N> dimishing = new HashSet<N>();
        private int diminishQueueLength
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private HashSet<N> developing = new HashSet<N>();
        private int developQueueLength
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Procedural(Scene scene, N root)
        {
            this.nodes = new List<N>();

            this.SceneRoot = root;
            nodes.Add(SceneRoot);
        }

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

            if (nodes.Count < Capacity && developQueueLength == 0)
            {
                for (int i = nodes.Count - 1; i >= 0; i--)
                {
                    var n = nodes[i];
                    if (!n.Developed)
                        QueueDevelop(n);
                }
            }
            else
            {
                for (int i = nodes.Count - 1; i >= 0; i--)
                {
                    bool developed = false;
                    var n = nodes[i];
                    if (!n.Developed || developing.Contains(n))
                    {
                        for (int j = 0; j < i; j++)
                        {
                            var d = nodes[j];
                            if (d.Developed && !dimishing.Contains(d))
                            {
                                QueueDiminish(d);
                                QueueDevelop(n);
                                developed = true;
                            }
                        }
                    }

                    if (!developed)
                        break;
                }
            }

            ApplyQueuedUpdates();

            base.Update(elapsedTime);
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

        private void ApplyQueuedUpdates()
        {
            throw new NotImplementedException();
        }

        private void QueueDevelop(N node)
        {
            developing.Add(node);
            throw new NotImplementedException();
        }

        private void QueueDiminish(N node)
        {
            dimishing.Add(node);
            throw new NotImplementedException();
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
    }
}
