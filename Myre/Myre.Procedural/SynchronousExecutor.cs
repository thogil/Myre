using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Collections;
using Myre.Entities;

namespace Myre.Procedural
{
    class SynchronousExecutor<N>
        :Procedural<N>.ITaskExecutor
        where N : ISceneNode<N>
    {
        private List<N> developing = new List<N>();
        private List<N> diminishing = new List<N>();

        public int DevelopCount
        {
            get { return developing.Count; }
        }

        public int DiminishCount
        {
            get { return diminishing.Count; }
        }

        public void QueueDevelop(N node)
        {
            developing.Add(node);
        }

        public bool IsDeveloping(N node)
        {
            return developing.Contains(node);
        }

        public void QueueDiminish(N node)
        {
            diminishing.Add(node);
        }

        public bool IsDiminishing(N node)
        {
            return diminishing.Contains(node);
        }

        public void ApplyQueuedUpdates(Scene scene, Procedural<N> manager)
        {
            foreach (var node in developing)
            {
                node.Develop(scene);
                foreach (var child in node.Children)
                    manager.AddNode(child);
            }
        }
    }
}
