using System;
using System.Collections.Generic;
using Godot;

public class AdventureTests {
    Adventure game;

    public static void RunTests() {
        GD.Print("Running adventure tests");
        AdventureTests test = new AdventureTests();
    }

    public AdventureTests() {
        game = new Adventure();

        // Turning off HUD menu, which is enabled by default as player 1 is created.
        GameState.Instance.ChangeMenu(MenuFactory.Menus.None);

        // Pause world to allow updates to be run arbitrarily, and not in real time.
        game.Pause();

        //
        // TESTS GO HERE
        //

        Test.PrintFails();
    }
}