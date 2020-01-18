using System;

public class GameEvent
{
    public string[] args;
    public Types type;

    public enum Types
    {
        /// <summary>
        /// Default
        /// </summary>
        None,

        /// <summary>
        /// Death of an actor
        /// </summary>
        ActorDied,

        /// <summary>
        /// Toggling pause
        /// </summary>
        Pause
    }

    public GameEvent()
    {
        this.type = Types.None;
        this.args = new string[0];
    }

    public static GameEvent PauseEvent()
    {
        GameEvent ge = new GameEvent();
        ge.type = Types.Pause;
        return ge;
    }

    public static GameEvent ActorDiedEvent(string dead, string killer)
    {
        GameEvent ge = new GameEvent();
        ge.type = Types.ActorDied;
        ge.args = new string[2];
        ge.args[0] = dead;
        ge.args[1] = killer;

        return ge;
    }
}
