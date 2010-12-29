using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities;

namespace Myre.Procedural
{
    /// <summary>
    /// A node encompassing a portion of space within a scene which can be developed into a set of smaller (non overlapping) child nodes
    /// </summary>
    public interface ISceneNode<N>
        where N : ISceneNode<N>
    {
        float GetImportance(int frameNumber);

        void AddToImportance(float delta, int frameNumber);

        /// <summary>
        /// Gets the parent of this node. Or null if this is the root
        /// </summary>
        /// <value>The parent.</value>
        N Parent
        {
            get;
        }

        /// <summary>
        /// Add child entities to scene, Produce child nodes for this node. All the children must be contained within the bounds of this node
        /// </summary>
        void Develop(Scene scene);

        /// <summary>
        /// Remove child nodes and child entities for this node
        /// </summary>
        void Diminish(Scene scene);

        /// <summary>
        /// Gets a value indicating whether this <see cref="ISceneNode"/> has been developed.
        /// </summary>
        /// <value><c>true</c> if developed; otherwise, <c>false</c>.</value>
        bool Developed
        {
            get;
        }

        /// <summary>
        /// Gets the children of this node.
        /// </summary>
        /// <value>The children.</value>
        /// <exception cref="InvalidOperationException">Thrown if this node has not been developed</exception>
        IEnumerable<N> Children
        {
            get;
        }

        /// <summary>
        /// Get the entities created by this node
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this node has not been developed</exception>
        IEnumerable<Entity> Entities
        {
            get;
        }
    }
}
