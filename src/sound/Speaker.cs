using System;
using Godot;

public class Speaker : AudioStreamPlayer3D {
    public void LoadEffect(Sound.Effects effect) {
        string soundFile = Sound.EffectFile(effect);
        this.Stream = GD.Load(soundFile) as AudioStreamSample;
    }

    public void PlayEffect(Sound.Effects effect) {
        this.Stop();
        LoadEffect(effect);
        this.Play();
    }

    public static Speaker Instance() {
        Speaker result = GameState.InstanceScene("res://src/sound/Speaker.tscn") as Speaker;
        return result;
    }
}