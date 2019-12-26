using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// définit les différents messages qui peuvent être envoyé
public enum type_message
{
    cooldown_start_jump,
    cooldown_start_boost,
    cooldown_start_hammer,
    cooldown_start_other,
    restart_hud_timers,
    cinematic
}

// définit une classe abstraite de message pour le pattern observateur obervé (simplifié)
public class BasicMessage
{

    public type_message type;

    public BasicMessage(type_message type)
    {
        this.type = type;
    }
}

// définit un message portant un message de type quelconque
public class Message<Data> : BasicMessage
{
    public Data infos;

    public Message(type_message type, Data data) : base(type)
    {
        this.infos = data;
    }
}
