using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace Hop.FileSystem
{
    class Query
    {
        public string Search { get; set; }
        public List<FileSystemItem> Stack { get; set; }
        public bool Execute { get; set; }
    }

    class FileSystemItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public byte[] Image { get; set; }
        public string FullName { get; set; }
    }

    class Program
    {
        static byte[] ToBytes(Image image)
        {
            using (var stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        static void Main(string[] args)
        {
            var query = JsonConvert.DeserializeObject<Query>(args.SingleOrDefault());
            var root = new DirectoryInfo(query?.Stack?.FirstOrDefault()?.FullName ?? "D:/");

            if (query.Execute)
            {
            }
            else
            {
                var items = root.GetFileSystemInfos($"{query.Search}*")
                    .Select(fs => new FileSystemItem
                    {
                        Name = fs.Name,
                        Description = "",
                        FullName = fs.FullName,
                        Image = ToBytes(ThumbnailGenerator.GetThumbnail(fs.FullName, 64, 64, ThumbnailOptions.None))
                    });
                var json = JsonConvert.SerializeObject(items);
                Console.WriteLine(json);
            }
        }
    }
}
