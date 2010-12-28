using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;

namespace Myre.Procedural
{
    public abstract class Observer<N>
        :Behaviour
    {
        public abstract float Rate(N node);
    }
}
