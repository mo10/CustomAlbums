using Ionic.Zip;
using CustomAlbums.Data;
using System.IO;

namespace CustomAlbums
{
    public class Album
    {
        public AlbumInfo Info { get; private set; }
        public string Path { get; private set; }
        public bool IsPackaged { get; private set; }
        public Album(string path)
        {
            if (File.Exists($"{path}/info.json"))
            {
                // Load from folder
                this.Info = File.OpenRead($"{path}/info.json").JsonDeserialize<AlbumInfo>();
                this.Path = path;
                this.IsPackaged = false;
                return;
            }
            else
            {
                // Load from package
                using (ZipFile zip = ZipFile.Read(path))
                {
                    if (zip["info.json"] != null)
                    {
                        this.Info = zip["info.json"].OpenReader().JsonDeserialize<AlbumInfo>();
                        this.Path = path;
                        this.IsPackaged = true;
                        return;
                    }
                }
            }
            throw new FileNotFoundException($"info.json not found");
        }
    }
}
