using System;
using System.Drawing;

namespace Hop
{
    public class Item
    {
        public string Name { get; }
        public string Description { get; }
        public Lazy<Image> Image { get; }
        public object Data { get; }

        public Item(string name, string description, Lazy<Image> image, object data)
        {
            Name = name;
            Description = description;
            Image = image;
            Data = data;
        }
    }
}
