using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class Mappings
{
    public static Dictionary<string, object> DefaultStats = new Dictionary<string, object>() {
        ["maxHP"] = 100,
        ["curHP"] = 100,
        ["AD"] = 10,
        ["MS"] = 800f,
        ["CR"] = 0f,
        ["AS"] = 1f,
        ["status"] = Status.OK,
    };
    public static Dictionary<ActionStatus, string> ActionStates = new Dictionary<ActionStatus, string>() {
        [ActionStatus.ATTACK] = "Attack",
        [ActionStatus.RUN] = "Run",
        [ActionStatus.HURT] = "Hurt",
        [ActionStatus.JUMP] = "Jump",
        [ActionStatus.FALL] = "Fall",
        [ActionStatus.IDLE] = "Idle",
    };

    public static Dictionary<ActionStatus, string> Bools = new Dictionary<ActionStatus, string> {
        [ActionStatus.RUN] = "run",
        [ActionStatus.JUMP] = "jump",
        [ActionStatus.FALL] = "fall",
        [ActionStatus.IDLE] = "idle",
    };

    public static Dictionary<ActionStatus, string> Triggers = new Dictionary<ActionStatus, string> {
        [ActionStatus.HURT] = "hurt",
        [ActionStatus.ATTACK] = "attack",
        [ActionStatus.DIE] = "death",
    };

    public static Dictionary<ProjectileStatus, string> ProjectileBools = new Dictionary<ProjectileStatus, string>() {
        [ProjectileStatus.MOVE] = "idle",
    };
    public static Dictionary<ProjectileStatus, string> ProjectileTriggers = new Dictionary<ProjectileStatus, string>() {
        [ProjectileStatus.CAST] = "cast",
        [ProjectileStatus.HIT] = "hit"
    };
    public static Dictionary<Button, KeyCode?> DefaultInputMapP1 = new Dictionary<Button, KeyCode?>() {
        [Button.DEFAULT_ATTACK] = KeyCode.K,
        [Button.SKILL_1] = KeyCode.I,
        [Button.SKILL_2] = KeyCode.J,
        [Button.SKILL_3] = KeyCode.L,
        [Button.SKILL_4] = KeyCode.O,
        [Button.JUMP] = KeyCode.Space,
        [Button.NO_ACTION] = null,
    };
    public static Dictionary<Button, KeyCode?> DefaultInputMapP2 = new Dictionary<Button, KeyCode?>() {
        [Button.DEFAULT_ATTACK] = KeyCode.Keypad5,
        [Button.SKILL_1] = KeyCode.Keypad4,
        [Button.SKILL_2] = KeyCode.Keypad8,
        [Button.SKILL_3] = KeyCode.Keypad6,
        [Button.SKILL_4] = KeyCode.Keypad2,
        [Button.JUMP] = KeyCode.Keypad0,
        [Button.NO_ACTION] = null,
    };

    public static Dictionary<string, Type> SkillMap = new Dictionary<string, Type>() {
        ["Dash"] = typeof(Dash),
        ["Rage"] = typeof(Rage),
        ["Heal"] = typeof(Heal),
        ["VampireSlash"] = typeof(VampireSlash),
        ["Fireball"] = typeof(Fireball),
        ["Stomp"] = typeof(Stomp),
        ["Blink"] = typeof(Blink),
        ["SonicWave"] = typeof(SonicWave),
        ["PoisonCloud"] = typeof(SnakeBite)
    };

    public static Dictionary<string, int> SkillIndex = new Dictionary<string, int>() {
        ["Dash"] = 0,
        ["Rage"] = 1,
        ["Heal"] = 2,
        ["VampireSlash"] = 3,
        ["Fireball"] = 4,
        ["Stomp"] = 5,
        ["Blink"] = 6,
        ["SonicWave"] = 7,
        ["PoisonCloud"] = 8
    };

    public static float[] DefaultSkillRow = new float[29] {
        0f, // idx
        0f, // ready
        0f, // mobility
        0f, // button
        0f, // curHP_d
        1f, // curHP_mult
        0f, // maxHP_d
        1f, // maxHP_mult
        0f, // AD_d
        1f, // AD_mult
        0f, // MS_d
        1f, // MS_mult
        0f, // AS_d
        1f, // AS_mult
        0f, // CR_d
        0f, // CR_mult
        0f, // newStatus
        0f, // selfCurHP_d
        1f, // selfCurHP_mult
        0f, // selfMaxHP_d
        1f, // selfMaxHP_mult
        0f, // selfAD_d
        1f, // selfAD_mult
        0f, // selfMS_d
        1f, // selfMS_mult
        0f, // selfAS_d
        1f, // selfAS_mult
        0f, // selfCR_d
        1f // selfCR_mult
    };
}
