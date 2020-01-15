using System;
using Godot;
using SC = System.Collections.Generic;
using GC = Godot.Collections;

public class Settings {
    public GC.Dictionary<string, GC.Dictionary<string, object>> Config { get; private set; }

    public static readonly string Filepath = "user://settings.json";

    public void Load() {
        File file = new File();

        // create new settings file if none exists
        if (!file.FileExists(Filepath)) {
            GD.Print("Creating settings file from default...");
            File defaultFile = new File();
            defaultFile.Open("res://data/default_settings.json", (int) File.ModeFlags.Read);
            var defaultContents = defaultFile.GetAsText();
            defaultFile.Close();

            file.Open(Filepath, (int) File.ModeFlags.Write);
            file.StoreString(defaultContents);
            file.Close();
        }

        // load the file
        this.Config = new GC.Dictionary<string, GC.Dictionary<string, object>>(Utils.GetJsonFile(Filepath));

        // check for loading errors
        if (this.Config == null) {
            GD.PushError("I think you messed up the JSON formatting of your settings file, please check it and try again.");
            GameState.Instance.Quit();
        }

        // TODO: Check for and remove/add any missing/extra keys
    }

    public bool ChangeItem(string section, string item, object value) {
        if (Config.ContainsKey(section)) {
            if (Config[section].ContainsKey(item)) {
                Config[section][item] = value;
                return true;
            } else {
                GD.PushWarning($"Item '{item}' not found in section '{section}' of the settings");
                return false;
            }
        } else {
            GD.PushWarning($"Section '{section}' does not exist in the settings");
            return false;
        }
    }

    public void Save() {
        Utils.PutJsonFile((GC.Dictionary) this.Config, Filepath, true);
    }

    public GC.Dictionary<string, object> this [string i] {
        get { return Config[i]; }
    }
}