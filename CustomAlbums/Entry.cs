using ModHelper;

namespace CustomAlbums
{
    class Entry : IMod
    {
        public string Name => "CustomAlbums";

        public string Description => "Adds custom charts to Muse Dash.";

        public string Author => "Mo10";

        public string HomePage => "https://github.com/mo10/MuseDashCustomAlbumMod";

        public void DoPatching()
        {
            CustomAlbum.DoPatching();
        }
    }
}
