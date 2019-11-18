using System.Collections.Generic;
using Godot;
namespace Game.Misc.Light
{
    public abstract class GameLight : Node2D
    {
        private static List<GameLight> gameLights = new List<GameLight>();
        public GameLight()
        {
            gameLights.Add(this);
        }
        public static List<GameLight> GetLights()
        {
            return gameLights;
        }
        public abstract void Start();
        public abstract void Stop();
    }
}