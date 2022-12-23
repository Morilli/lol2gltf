using System.Collections.ObjectModel;
using LeagueToolkit.IO.SimpleSkinFile;

namespace lol2gltf.UI.MVVM.ViewModels
{
    public class SimpleSkinViewModel
    {
        public int VertexCount { get; private set; }
        public int FaceCount { get; private set; }
        public ObservableCollection<SimpleSkinSubmeshViewModel> Submeshes { get; private set; } = new ObservableCollection<SimpleSkinSubmeshViewModel>();

        public SimpleSkin SimpleSkin { get; }

        public SimpleSkinViewModel(SimpleSkin simpleSkin)
        {
            this.SimpleSkin = simpleSkin;

            foreach (SimpleSkinSubmesh submesh in simpleSkin.Submeshes)
            {
                this.Submeshes.Add(new SimpleSkinSubmeshViewModel(submesh));

                this.VertexCount += submesh.Vertices.Count;
                this.FaceCount += submesh.Indices.Count / 3;
            }
        }
    }
}
