using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Caelum_ReCore
{
    public static class SoundManager
    {
        public static SoundEffectInstance MenuMusicInstance;
        public static Song GameMusic;

        public static void LoadContent(ContentManager content)
        {
            // Load wav file correctly as SoundEffect
            SoundEffect menuSound = content.Load<SoundEffect>("MenuBG");
            MenuMusicInstance = menuSound.CreateInstance();
            MenuMusicInstance.IsLooped = true;

            try
            {
                GameMusic = content.Load<Song>("BGM");
            }
            catch
            {
                GameMusic = null; // Prevents crash if file is missing
            }
        }

        public static void PlayMenuMusic()
        {
            if (MenuMusicInstance.State != SoundState.Playing)
            {
                MediaPlayer.Stop();
                MenuMusicInstance.Play();
            }
        }

        public static void PlayGameMusic()
        {
            if (MenuMusicInstance.State == SoundState.Playing)
                MenuMusicInstance.Stop();

            if (GameMusic != null && (MediaPlayer.State != MediaState.Playing || MediaPlayer.Queue.ActiveSong != GameMusic))
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(GameMusic);
            }
        }
    }
}
