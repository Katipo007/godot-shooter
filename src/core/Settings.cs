using System;
using Godot;
using SC = System.Collections.Generic;
using GC = Godot.Collections;

public class Settings
{
    public GC.Dictionary<string, GC.Dictionary<string, object>> Config { get; private set; }

    public static readonly string Filepath = "user://settings.json";

    public void Load()
    {
        File file = new File();

        // create new settings file if none exists
        if (!file.FileExists(Filepath))
        {
            GD.Print("Creating settings file from default...");
            File defaultFile = new File();
            defaultFile.Open("res://data/default_settings.json", File.ModeFlags.Read);
            var defaultContents = defaultFile.GetAsText();
            defaultFile.Close();

            file.Open(Filepath, File.ModeFlags.Write);
            file.StoreString(defaultContents);
            file.Close();
        }

        // load the file
        this.Config = new GC.Dictionary<string, GC.Dictionary<string, object>>(Utils.GetJsonFile(Filepath));

        // check for loading errors
        if (this.Config == null)
        {
            GD.PushError("I think you messed up the JSON formatting of your settings file, please check it and try again.");
            GameState.Instance.Quit();
        }

        // TODO: Check for and remove/add any missing/extra keys
    }

    /// <summary>
    /// Update a setting
    /// </summary>
    /// <param name="section"></param>
    /// <param name="item"></param>
    /// <param name="value"></param>
    /// <returns>whether the section/key existed and was overwritten</returns>
    public bool ChangeItem(string section, string item, object value)
    {
        if (Config.ContainsKey(section))
        {
            if (Config[section].ContainsKey(item))
            {
                Config[section][item] = value;
                return true;
            }
            else
            {
                GD.PushWarning($"Item '{item}' not found in section '{section}' of the settings");
                return false;
            }
        }
        else
        {
            GD.PushWarning($"Section '{section}' does not exist in the settings");
            return false;
        }
    }

    /// <summary>
    /// Get a value with an optional default paramater if the section or item does not exist
    /// </summary>
    /// <param name="section"></param>
    /// <param name="item"></param>
    /// <param name="value"></param>
    /// <param name="@default"></param>
    /// <returns></returns>
    public object Get(string section, string item, object value, object @default = null)
    {
        if (Config.ContainsKey(section))
        {
            if (Config[section].ContainsKey(item))
            {
                return Config[section][item];
            }
            else
            {
                return default;
            }
        }
        else
        {
            GD.PushWarning($"Section '{section}' does not exist in the settings");
            return default;
        }
    }

    public void Save()
    {
        Utils.PutJsonFile((GC.Dictionary) this.Config, Filepath, true);
    }

    public GC.Dictionary<string, object> this [string i]
    {
        get { return Config[i]; }
    }
}
