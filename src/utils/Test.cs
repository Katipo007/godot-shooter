using System;
using System.Collections.Generic;
using Godot;

public class Test {
    /// <summary>
    /// Failed test info
    /// </summary>
    public static List<string> Fails { get; private set; }

    public static bool Debugs { get; private set; }

    public static void Init(bool showDebugs = true) {
        Fails = new List<string>();
        Debugs = showDebugs;
    }

    public static void Assert(bool result, string message) {
        if (!result)
            Fail(message, StackTrace());
        else if (Debugs)
            GD.Print("Passed: " + message);
    }

    public static void Fail(string message, string trace) {
        string failedMessage = "Failed: " + message;

        if (Debugs)
            GD.Print(failedMessage + trace);

        Fails.Add(failedMessage);
    }

    public static void PrintFails() {
        GD.Print("##############################################################");
        GD.Print("#                       Error report                         #");
        GD.Print("##############################################################");
        if (Fails.Count == 0) {
            GD.Print("All tests ran successfully!");
        } else {
            GD.Print(Fails.Count + " tests failed.");
        }
        foreach (string fail in Fails) {
            GD.Print(fail);
        }
    }

    public static string StackTrace() {
        return System.Environment.StackTrace;
    }
}