using System;
using System.Collections.Generic;
using LeagueToolkit.IO.AnimationFile;

namespace lol2gltf.Core.ConversionOptions
{
    public class SkinnedModelToGltf : IBaseSimpleSkinToGltf
    {
        public string SimpleSkinPath { get; set; }
        public Dictionary<string, ReadOnlyMemory<byte>> MaterialTextures { get; set; }
        public string OutputPath { get; set; }
        public string SkeletonPath { get; set; }
        public List<(string, Animation)> Animations { get; set; }
    }
}
