using System;
using Godot;
using SC = System.Collections.Generic;
using GC = Godot.Collections;

/// <summary>
/// Class of helper utility functions
/// </summary>
public static class Utils {
    public static Vector3 TUp(Transform t) {
        return t.basis.y;
    }

    public static Vector3 TForward(Transform t) {
        return -t.basis.z;
    }

    public static Vector3 TRight(Transform t) {
        return t.basis.x;
    }

    public static Vector3 ToDegrees(Vector3 radians) {
        return new Vector3(
            Mathf.Rad2Deg(radians.x),
            Mathf.Rad2Deg(radians.y),
            Mathf.Rad2Deg(radians.z)
        );
    }

    public static Vector3 ToRadians(Vector3 degrees) {
        return new Vector3(
            Mathf.Deg2Rad(degrees.x),
            Mathf.Deg2Rad(degrees.y),
            Mathf.Deg2Rad(degrees.z)
        );
    }

    public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T> {
        if (val.CompareTo(min) < 0) return min;
        else if (val.CompareTo(max) > 0) return max;
        else return val;
    }

    public static void CaptureMouse() {
        Input.SetMouseMode(Input.MouseMode.Captured);
    }

    public static void PutJsonFile(SC.Dictionary<string, object> data, string filepath, bool sort = true) {
        throw new NotImplementedException();
    }

    public static void PutJsonFile(GC.Dictionary data, string filepath, bool sort = true) {

    }

    public static GC.Dictionary GetJsonFile(string filepath) {
        File file = new File();
        Error e = file.Open(filepath, (int) File.ModeFlags.Read);

        if (e != Error.Ok) {
            GD.PrintErr("Failed to open json file [", filepath, "]: ", e);
            return null;
        }

        var text = file.GetAsText();
        file.Close();

        var resultJson = JSON.Parse(text);

        if (resultJson.Error != Error.Ok) {
            GD.PrintErr("Error parsing json file [", filepath, "]: ", resultJson.Error, resultJson.ErrorLine, resultJson.ErrorString);
            return null;
        }

        return resultJson.Result as GC.Dictionary;
    }
}