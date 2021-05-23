using MelonLoader;
using MuseDashCustomAlbumMod;

[assembly: MelonInfo(typeof(Entry), "CustomAlbum", "1.0.0", "Mo10")]
[assembly: MelonGame("PeroPeroGames", "Muse Dash")]

namespace MuseDashCustomAlbumMod
{
    public class Entry : MelonMod
    {
        public override void OnApplicationStart()
        {
            CustomAlbum.LoadDependencies();
            CustomAlbum.DoPatching(HarmonyInstance);
        }
    }
}