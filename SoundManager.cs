using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Caelum_ReCore
{
    public static class SoundManager
    {
        public static SoundEffectInstance MenuMusicInstance;
        public static SoundEffectInstance RunSoundInstance;
        public static SoundEffectInstance FireballSoundInstance; // New Instance
        public static Song GameMusic;
        public static SoundEffect JumpSound, ButtonClickSound;

        public static void LoadContent(ContentManager content)
        {
            // Load Menu Music
            SoundEffect menuSound = content.Load<SoundEffect>("MenuBG");
            MenuMusicInstance = menuSound.CreateInstance();
            MenuMusicInstance.IsLooped = true;

            // Load Run Sound
            SoundEffect runSound = content.Load<SoundEffect>("rungrass");
            RunSoundInstance = runSound.CreateInstance();
            RunSoundInstance.IsLooped = true;

            // Load Fireball Sound as an Instance for restart control
            SoundEffect fireballSound = content.Load<SoundEffect>("fireballsound");
            FireballSoundInstance = fireballSound.CreateInstance();

            // Load standard sound effects
            JumpSound = content.Load<SoundEffect>("jumpsound");
            ButtonClickSound = content.Load<SoundEffect>("button");

            // Load BGM
            try { GameMusic = content.Load<Song>("BGM"); }
            catch { GameMusic = null; }
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
            if (MenuMusicInstance.State == SoundState.Playing) MenuMusicInstance.Stop();
            if (GameMusic != null && (MediaPlayer.State != MediaState.Playing || MediaPlayer.Queue.ActiveSong != GameMusic))
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(GameMusic);
            }
        }

        public static void PlayRunSound()
        {
            if (RunSoundInstance.State != SoundState.Playing) RunSoundInstance.Play();
        }

        public static void StopRunSound()
        {
            if (RunSoundInstance.State == SoundState.Playing) RunSoundInstance.Stop();
        }

        public static void PlayFireballSound()
        {
            // Stop the instance if already playing, then play it from the start.
            // This prevents sound stacking and ear fatigue when spamming.
            FireballSoundInstance.Stop();
            FireballSoundInstance.Play();
        }

        public static void PlayJumpSound() => JumpSound.Play();

        public static void PlayButtonSound() => ButtonClickSound.Play();
    }
}
