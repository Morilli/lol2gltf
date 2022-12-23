using System.Collections.Generic;
using SixLabors.ImageSharp;

namespace lol2gltf.Core.ConversionOptions
{
    public interface IBaseSimpleSkinToGltf
    {
        public string SimpleSkinPath { get; set; }

        public Dictionary<string, Image> MaterialTextures { get; set; }

        public string OutputPath { get; set; }
    }
}
