using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;

namespace Myre.Procedural
{
    public abstract class Observer
        :Behaviour
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Observer"/> has changed since it was last checked
        /// </summary>
        /// <value><c>true</c> if changed; otherwise, <c>false</c>.</value>
        public bool Changed
        {
            get;
            protected internal set;
        }
    }
}
