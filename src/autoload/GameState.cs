using System;
using System.Collections.Generic;
using System.Text;
using Godot;

/// <summary>
/// Pseudo-singleton, focal point for the state of the game.
/// Should be the root node of the game, if this is null things won't work
/// </summary>
public class GameState : Node {
    /// <summary>
    /// Get the singleton instance
    /// </summary>
    /// <value></value>
    public static GameState Instance { get; private set; }

    private Node _activeMenu;
    public Random Random { get; private set; }
    public AudioStreamPlayer Jukebox { get; private set; }
    public Settings Settings { get; private set; }
    public Player Player { get; private set; }
    public Adventure Adventure { get; private set; }

    public enum GameModes {
        None,
        Adventure
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        // enforce the singleton handler
        if (Instance == null) {
            Instance = this;
        } else {
            this.QueueFree();
        }

        ChangeMenu(MenuFactory.Menus.Main);
        InitJukebox();
        InitSettings();

        if (this.Random == null) {
            this.Random = new System.Random();
        }
    }

    public void PerformTests() {
        Test.Init();
        AdventureTests.RunTests();
    }

    public void Quit() {
        GetTree().Quit();
    }

    public void InitJukebox() {
        if (this.Jukebox == null) {
            this.Jukebox = new AudioStreamPlayer();
            AddChild(this.Jukebox);
        }
    }

    /// <summary>
    /// Convenience method for creating nodes
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Node InstanceScene(string path) {
        byte[] bytes = Encoding.Default.GetBytes(path);
        path = Encoding.UTF8.GetString(bytes);
        PackedScene packedScene = (PackedScene) GD.Load(path);

        if (packedScene == null) {
            GD.PrintErr("Path [" + path + "] is invalid.");
            return null;
        }
        return packedScene.Instance();
    }

    /// <summary>
    /// Removes game nodes/variables
    /// </summary>
    public void ClearGame() {
        if (this.Adventure != null) {
            this.Adventure.QueueFree();
            this.Adventure = null;
        }

        Input.SetMouseMode(Input.MouseMode.Visible);
    }

    public void QuitToMainMenu() {
        ChangeMenu(MenuFactory.Menus.Main);
        ClearGame();
    }

    public void LocalAdventure() {
        ChangeMenu(MenuFactory.Menus.None);
        this.Adventure = new Adventure();
        AddChild(this.Adventure);
    }

    public void ChangeMenu(MenuFactory.Menus menu) {
        if (_activeMenu != null) {
            IMenu menuInstance = _activeMenu as IMenu;

            _activeMenu.QueueFree();
            _activeMenu = null;
        }

        _activeMenu = MenuFactory.Create(menu, this);
    }

    public void HandleEvent(GameEvent gameEvent) {
        if (this.Adventure != null)
            this.Adventure.HandleEvent(gameEvent);
    }

    public void Event(GameEvent gameEvent) {
        HandleEvent(gameEvent);
    }
}