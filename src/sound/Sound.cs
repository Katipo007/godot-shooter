using Godot;

public class Sound {
    public enum Effects {
        None
    }

    public enum Music {
        None
    }

    public static string MusicFile(Music music) {
        switch (music) {
            case Music.None:
                return null;

            default:
                GD.PushWarning($"No music file configured for {music}");
                return null;
        }
    }

    public static string EffectFile(Effects effect) {
        switch (effect) {
            case Effects.None:
                return null;

            default:
                GD.PushWarning($"No effect file configured for {effect}");
                return null;
        }
    }

    public static void RefreshVolume() {
        float musicVolume = Sound.VolumeMath(GameState.Instance.Settings.MusicVolume);
        GameState.Instance.Jukebox.VolumeDb = musicVolume;
    }

    public static float VolumeMath(float val) {
        val *= GameState.Instance.Settings.masterVolume;
        val *= 100f;

        float remainder = 100f - val; // distance from 100%
        val = 0f - remainder; // volume should be -distance decibels
        return val;
    }

    public static void PlayMusic(Music track) {
        if (track == Music.None)
            return;

        string filename = MusicFile(track);
        AudioStreamOGGVorbis stream = GD.Load(filename) as AudioStreamOGGVorbis;

        GameState.Instance.Jukebox.Stream = stream;
        GameState.Instance.Jukebox.Playing = true;
    }

    public static void PauseSong() {
        if (GameState.Instance.Jukebox == null)
            return;

        GameState.Instance.Jukebox.Playing = false;
    }
}