using System.Collections.Generic;
using Godot;
namespace Game.Database
{
    public static class ImageDB
    {
        private static readonly string DB_PATH = "res://data/ImageDB.xml";
        private static readonly string[] attributes = new string[] {"attacking", "dying", "moving", "swing", "total", "weapon", "body"};
        private static readonly XMLParser xMLParser = new XMLParser();

        public static Dictionary<string, string> GetImageData(string imageName)
        {
            Dictionary<string, string> imageData = new Dictionary<string, string>();
            xMLParser.Open(DB_PATH);
            while (xMLParser.Read() == Error.Ok && imageData.Count == 0)
            {
                if (xMLParser.GetNodeType() == XMLParser.NodeType.Element &&
                xMLParser.GetNamedAttributeValueSafe("name").Equals(imageName))
                {
                    foreach (string attribute in attributes)
                    {
                        imageData.Add(attribute, xMLParser.GetNamedAttributeValueSafe(attribute));
                    }
                }
            }
            return imageData;
        }
    }
}