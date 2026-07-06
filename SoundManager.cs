using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace Caelum_ReCore
{
    public static class SoundManager
    {
        public static Song MenuMusic;
        public static Song GameMusic;

        public static void LoadContent(ContentManager content)
        {
            MenuMusic = content.Load<Song>("MenuBG");
            GameMusic = content.Load<Song>("BGM");
        }

        public static void PlayMenuMusic()
        {
            if (MediaPlayer.State != MediaState.Playing || MediaPlayer.Queue.ActiveSong != MenuMusic)
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(MenuMusic);
            }
        }

        public static void PlayGameMusic()
        {
            if (MediaPlayer.State != MediaState.Playing || MediaPlayer.Queue.ActiveSong != GameMusic)
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(GameMusic);
            }
        }
    }
}
