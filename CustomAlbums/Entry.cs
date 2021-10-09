using ModHelper;

namespace CustomAlbums
{
    class Entry : IMod
    {
        public string Name => "CustomAlbum";

        public string Description => "Custom Album";

        public string Author => "Mo10";

        public string HomePage => "https://github.com/mo10/MuseDashCustomAlbumMod";

        public void DoPatching()
        {
            CustomAlbum.DoPatching();
        }
    }
}
