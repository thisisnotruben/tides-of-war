using System.Collections.Generic;
using System;
using Godot;
namespace Game.Database
{
    public static class ImageDB
    {
        public struct ImageNode
        {
            public int total;
            public int moving;
            public int dying;
            public int attacking;
            public string weapon;
            public string swing;
            public string body;
            public string weaponMaterial;
            public bool melee;
        }
        private static Dictionary<string, ImageNode> imageData = new Dictionary<string, ImageNode>();
        private static readonly string DB_PATH = "res://data/image.json";
        
        static ImageDB()
        {
            LoadImageData();
        }
        private static void LoadImageData()
        {
            File file = new File();
            file.Open(DB_PATH, File.ModeFlags.Read);
            JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
            file.Close();
            Godot.Collections.Dictionary rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
            foreach (string imgName in rawDict.Keys)
            {
                Godot.Collections.Dictionary imgDict = (Godot.Collections.Dictionary) rawDict[imgName];
                ImageNode imageNode;
                imageNode.moving = (int) ((Single) imgDict[nameof(ImageNode.moving)]);
                imageNode.dying = (int) ((Single) imgDict[nameof(ImageNode.dying)]);
                imageNode.attacking = (int) ((Single) imgDict[nameof(ImageNode.attacking)]);
                imageNode.total = imageNode.moving + imageNode.dying + imageNode.attacking;
                imageNode.weapon = (string) imgDict[nameof(ImageNode.weapon)];
                imageNode.weaponMaterial = (string) imgDict[nameof(ImageNode.weaponMaterial)];
                imageNode.swing = (string) imgDict[nameof(ImageNode.swing)];
                imageNode.body = (string) imgDict[nameof(ImageNode.body)];
                imageNode.melee = (bool) imgDict[nameof(ImageNode.melee)];
                imageData.Add(imgName, imageNode);
            }
        }
        public static ImageNode GetImageData(string imageName)
        {
            return imageData[imageName];
        }
    }
}