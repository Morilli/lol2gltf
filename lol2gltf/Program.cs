using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using LeagueToolkit.Core.Mesh;
using LeagueToolkit.Core.Primitives;
using LeagueToolkit.IO.MapGeometryFile;
using LeagueToolkit.IO.SimpleSkinFile;
using LeagueToolkit.IO.SkeletonFile;
using lol2gltf.Core.ConversionOptions;
using SharpGLTF.Schema2;
using SixLabors.ImageSharp;
using Image = SixLabors.ImageSharp.Image;
using LeagueAnimation = LeagueToolkit.IO.AnimationFile.Animation;
using LeagueConverter = lol2gltf.Core.Converter;

namespace lol2gltf
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<
                SimpleSkinToGltfOptions,
                SkinnedModelToGltfOptions,
                DumpSimpleSkinInfoOptions,
                CreateSimpleSkinFromLegacyOptions,
                ConvertMapGeometryToGltfOptions>(args)
                .MapResult(
                    (SimpleSkinToGltfOptions opts) => ConvertSimpleSkin(opts),
                    (SkinnedModelToGltfOptions opts) => ConvertSkinnedModel(opts),
                    (DumpSimpleSkinInfoOptions opts) => DumpSimpleSkinInfo(opts),
                    (CreateSimpleSkinFromLegacyOptions opts) => CreateSimpleSkinFromLegacy(opts),
                    (ConvertMapGeometryToGltfOptions opts) => ConvertMapGeometryToGltf(opts),
                    errors => 1
                );
        }

        // ------------- COMMANDS ------------- \\

        private static int ConvertSimpleSkin(SimpleSkinToGltfOptions opts)
        {
            try
            {
                SimpleSkinToGltf simpleSkinToGltf = new SimpleSkinToGltf()
                {
                    OutputPath = opts.OutputPath,
                    SimpleSkinPath = opts.SimpleSkinPath,
                    MaterialTextures = CreateMaterialTextureMap(opts.MaterialTextures)
                };

                LeagueConverter.ConvertSimpleSkin(simpleSkinToGltf);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Failed to convert Simple Skin to glTF");
                Console.WriteLine(exception);
            }

            return 1;
        }

        private static int ConvertSkinnedModel(SkinnedModelToGltfOptions opts)
        {
            try
            {
                Skeleton skeleton = ReadSkeleton(opts.SkeletonPath);

                List<(string, LeagueAnimation)> animations = null;
                if (!string.IsNullOrEmpty(opts.AnimationsFolder))
                {
                    string[] animationFiles = Directory.GetFiles(opts.AnimationsFolder, "*.anm");
                    animations = ReadAnimations(animationFiles);
                }
                else
                {
                    animations = ReadAnimations(opts.AnimationPaths);
                }

                // Check animation compatibility
                foreach (var animation in animations)
                {
                    if (!animation.Item2.IsCompatibleWithSkeleton(skeleton))
                    {
                        Console.WriteLine("Warning: Found an animation that's potentially not compatible with the provided skeleton - " + animation.Item1);
                    }
                }

                SkinnedModelToGltf skinnedModelToGltf = new SkinnedModelToGltf()
                {
                    OutputPath = opts.OutputPath,
                    Animations = animations,
                    MaterialTextures = CreateMaterialTextureMap(opts.MaterialTextures),
                    SimpleSkinPath = opts.SimpleSkinPath,
                    SkeletonPath = opts.SkeletonPath
                };

                LeagueConverter.ConvertSkinnedModel(skinnedModelToGltf);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Failed to convert Skinned Model to glTF");
                Console.WriteLine(exception);
            }

            return 1;
        }

        private static int CreateSimpleSkinFromLegacy(CreateSimpleSkinFromLegacyOptions opts)
        {
            try
            {
                CreateSimpleSkinFromLegacy createSimpleSkinFromLegacy = new CreateSimpleSkinFromLegacy()
                {
                    SimpleSkinPath = opts.SimpleSkinPath,
                    StaticObjectPath = opts.StaticObjectPath,
                    WeightFilePath = opts.WeightFilePath
                };

                LeagueConverter.CreateSimpleSkinFromLegacy(createSimpleSkinFromLegacy);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Failed to convert Simple Skin to glTF");
                Console.WriteLine(exception);
            }

            return 1;
        }

        private static int ConvertMapGeometryToGltf(ConvertMapGeometryToGltfOptions opts)
        {
            try
            {
                ConvertMapGeometryToGltf convertMapGeometryToGltf = new ConvertMapGeometryToGltf()
                {
                    MapGeometryPath = opts.MapGeometryPath,
                    OutputPath = opts.OutputPath
                };

                LeagueConverter.ConvertMapGeometryToGltf(convertMapGeometryToGltf);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Failed to convert Map Geometry to glTF");
                Console.WriteLine(exception);
            }

            return 1;
        }

        private static int ConvertGltfToSimpleSkin(ConvertGltfToSimpleSkinOptions opts)
        {
            ModelRoot gltf = ReadGltf(opts.GltfPath);
            (SkinnedMesh simpleSkin, Skeleton skeleton) = gltf.ToRiggedMesh();

            simpleSkin.WriteSimpleSkin(opts.SimpleSkinPath);
            skeleton.Write(opts.SkeletonPath);

            return 1;
        }

        // ------------- BACKING FUNCTIONS ------------- \\

        private static SkinnedMesh ReadSimpleSkin(string location)
        {
            try
            {
                return SkinnedMesh.ReadFromSimpleSkin(location);
            }
            catch (Exception exception)
            {
                throw new Exception("Error: Failed to read specified SKN file", exception);
            }
        }

        private static Skeleton ReadSkeleton(string location)
        {
            try
            {
                return new Skeleton(location);
            }
            catch (Exception exception)
            {
                throw new Exception("Error: Failed to read specified SKL file", exception);
            }
        }

        private static List<(string, LeagueAnimation)> ReadAnimations(IEnumerable<string> animationPaths)
        {
            var animations = new List<(string, LeagueAnimation)>();

            foreach (string animationPath in animationPaths)
            {
                string animationName = Path.GetFileNameWithoutExtension(animationPath);
                LeagueAnimation animation = new LeagueAnimation(animationPath);

                animations.Add((animationName, animation));
            }

            return animations;
        }

        private static MapGeometry ReadMapGeometry(string location)
        {
            try
            {
                return new MapGeometry(location);
            }
            catch (Exception exception)
            {
                throw new Exception("Error: Failed to read map geometry file", exception);
            }
        }

        private static ModelRoot ReadGltf(string location)
        {
            try
            {
                return ModelRoot.Load(Path.GetFullPath(location));
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to load glTF file", exception);
            }
        }

        private static Dictionary<string, ReadOnlyMemory<byte>> CreateMaterialTextureMap(IEnumerable<string> materialTextures)
        {
            var materialTextureMap = new Dictionary<string, ReadOnlyMemory<byte>>();

            foreach (string materialTexture in materialTextures)
            {
                if (!materialTexture.Contains(':'))
                {
                    throw new Exception("Material Texture does not contain a separator (:) - " + materialTexture);
                }

                string[] materialTextureSplit = materialTexture.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                string material = materialTextureSplit[0];
                string texturePath;
                if (materialTextureSplit.Length == 2) // Material : Relative path
                {
                    texturePath = materialTextureSplit[1];
                }
                else if (materialTextureSplit.Length == 3) // Material : Absolute path C:/.......
                {
                    texturePath = materialTextureSplit[1] + ':' + materialTextureSplit[2];
                }
                else
                {
                    throw new Exception("Invalid format for material texture: " + materialTexture);
                }

                byte[] textureImage;
                try
                {
                    var textureStream = new MemoryStream();
                    using (Image image = Image.Load(texturePath))
                    {
                        image.SaveAsPng(textureStream);
                    }

                    textureImage = textureStream.GetBuffer();
                }
                catch (Exception exception)
                {
                    throw new Exception($"Error: Failed to load texture {texturePath}: ", exception);
                }

                if (materialTextureMap.ContainsKey(material))
                {
                    throw new Exception($"Material <{material}> has already been added");
                }

                materialTextureMap.Add(material, textureImage);
            }

            return materialTextureMap;
        }

        private static int DumpSimpleSkinInfo(DumpSimpleSkinInfoOptions opts)
        {
            SkinnedMesh simpleSkin = ReadSimpleSkin(opts.SimpleSkinPath);
            if (simpleSkin != null)
            {
                DumpSimpleSkinInfo(simpleSkin);
            }

            return 1;
        }

        private static void DumpSimpleSkinInfo(SkinnedMesh simpleSkin)
        {
            Console.WriteLine("----------SIMPLE SKIN INFO----------");

            Box boundingBox = simpleSkin.AABB;
            Console.WriteLine("Bounding Box:");
            Console.WriteLine("\t Min: " + boundingBox.Min.ToString());
            Console.WriteLine("\t Max: " + boundingBox.Max.ToString());

            Console.WriteLine("Submesh Count: " + simpleSkin.Ranges.Count);

            foreach (SkinnedMeshRange submesh in simpleSkin.Ranges)
            {
                Console.WriteLine("--- SUBMESH ---");
                Console.WriteLine("Material: " + submesh.Material);
                Console.WriteLine("Vertex Count: " + submesh.VertexCount);
                Console.WriteLine("Index Count: " + submesh.IndexCount);
                Console.WriteLine("Face Count: " + submesh.IndexCount / 3);
                Console.WriteLine();
            }
        }
    }
}
