using Godot;
using System.Collections.Generic;

namespace Game.Misc.Light
{
    public abstract class GameLight : Node2D
    {
        private static List<GameLight> gameLights = new List<GameLight>();

        public GameLight()
        {
            gameLights.Add(this);
        }
        public abstract void Start();
        public abstract void Stop();
        public static List<GameLight> GetLights()
        {
            return gameLights;
        }
    }
}