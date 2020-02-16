using System.Collections.Generic;
using Godot;
namespace Game.Database
{
    public static class ImageDB
    {
        private static readonly string DB_PATH = "res://data/ImageDB.xml";
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
                    for (int i = 0; i < xMLParser.GetAttributeCount(); i++)
                    {
                        imageData.Add(xMLParser.GetAttributeName(i), xMLParser.GetAttributeValue(i));
                    }   
                }
            }
            return imageData;
        }
    }
}