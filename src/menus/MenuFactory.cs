using System;
using Godot;

public static class MenuFactory {
    public enum Menus {
        None,
        Main,
        Settings,
        Pause,
        LoadAdventure
    }

    public static Control Create(MenuFactory.Menus menu, Node node) {
        Node result = null;

        switch (menu) {
            case Menus.None:
                // Sound.PauseSong();
                return null;

            case Menus.Pause:
                result = ResourceLoader.Load<PackedScene>("res://src/menus/pause.tscn").Instance() as Control;
                break;

            case Menus.Main:
                result = ResourceLoader.Load<PackedScene>("res://src/menus/main.tscn").Instance() as Control;
                break;

            case Menus.Settings:
                result = ResourceLoader.Load<PackedScene>("res://src/menus/settings.tscn").Instance() as Control;
                break;

            case Menus.LoadAdventure:
                result = ResourceLoader.Load<PackedScene>("res://src/menus/load_adventure.tscn").Instance() as Control;
                break;

            default:
                throw new ArgumentException($"Factory for menu {menu.ToString()} is not yet implemented!");
        }

        node.AddChild(result);

        return result as Control;
    }
}