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

namespace Myre.Procedural
{
    /// <summary>
    /// A service which automatically creates and destroys entities in a scene
    /// </summary>
    public class Procedural
        :Service, IBehaviourManager<Observer>
    {
        private List<Observer> observers = new List<Observer>();

        public Procedural(Scene scene)
        {

        }

        public void AddObserver()
        {

        }

        public override void Update(float elapsedTime)
        {
            foreach (var observer in observers)
            {
                if (observer.Changed)
                {
                    throw new NotImplementedException("Begin updating the scene to reflect this change");
                }
            }

            base.Update(elapsedTime);
        }

        public void Add(Observer behaviour)
        {
            observers.Add(behaviour);
        }

        public bool Remove(Observer behaviour)
        {
            return observers.Remove(behaviour);
        }
    }
}
