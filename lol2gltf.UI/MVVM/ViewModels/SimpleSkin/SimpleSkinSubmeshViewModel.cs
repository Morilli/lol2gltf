﻿using LeagueToolkit.IO.SimpleSkinFile;
using Microsoft.WindowsAPICodePack.Dialogs;
using SixLabors.ImageSharp;

namespace lol2gltf.UI.MVVM.ViewModels
{
    public class SimpleSkinSubmeshViewModel : PropertyNotifier
    {
        public string Name { get; private set; }
        public int VertexCount { get; private set; }
        public int FaceCount { get; private set; }

        public FileSelectionViewModel TextureFileSelection
        {
            get => this._textureFileSelection;
            set
            {
                this._textureFileSelection = value;
                NotifyPropertyChanged();
            }
        }
        public Image Texture { get; private set; }

        private FileSelectionViewModel _textureFileSelection;

        public SimpleSkinSubmeshViewModel(SimpleSkinSubmesh submesh)
        {
            this.Name = submesh.Name;
            this.VertexCount = submesh.Vertices.Count;
            this.FaceCount = submesh.Indices.Count / 3;

            this.TextureFileSelection = new FileSelectionViewModel(
                "Select a DDS texture",
                OnSelectedTextureChanged,
                new CommonFileDialogFilter("DDS", "*.dds"));
        }

        private void OnSelectedTextureChanged(string filePath)
        {
            this.Texture = Image.Load(filePath);
        }
    }
}
