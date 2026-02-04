using UnityEngine;

/// <summary>
/// Constantes globales du jeu pour faciliter la maintenance et éviter les "magic numbers"
/// </summary>
public static class GameConstants
{
    // Sword Stats
    public const float BASE_SWORD_VALUE = 10f;
    public const float LEVEL_MULTIPLIER_PER_LEVEL = 0.01f; // 1% par niveau

    // Sell Zone
    public const float DEFAULT_SELL_COUNTDOWN = 30f;

    // Player Movement
    public const float DEFAULT_MOVE_SPEED = 6f;
    public const float DEFAULT_JUMP_HEIGHT = 2f;
    public const float DEFAULT_GRAVITY = -9.81f;
    public const float DEFAULT_ROTATION_SPEED = 10f;

    // Camera
    public const float DEFAULT_CAMERA_DISTANCE = 4f;
    public const float DEFAULT_MOUSE_SENSITIVITY = 3f;
    public const float DEFAULT_CAMERA_MIN_Y = -40f;
    public const float DEFAULT_CAMERA_MAX_Y = 60f;

    // Conveyor
    public const float DEFAULT_CONVEYOR_SPEED = 2f;
    public const float DEFAULT_CONVEYOR_WAIT_TIME = 1f;
    public const int MIN_PAUSE_POINTS = 2;

    // Enchantments
    public const int MIN_ENCHANTMENTS = 1;
    public const int MAX_ENCHANTMENTS = 3;

    // Player Level
    public const int STARTING_LEVEL = 1;
    public const int BASE_XP_TO_NEXT_LEVEL = 100;
    public const int XP_INCREMENT_PER_LEVEL = 20;
    public const int XP_PER_SWORD_SOLD = 10;

    // UI
    public const float DEFAULT_PROXIMITY_DISTANCE = 3f;
    public static readonly Vector3 DEFAULT_PROXIMITY_OFFSET = new Vector3(0, 1, 0);

    // Formatting
    public static readonly string[] MONEY_SUFFIXES = { "", "k", "M", "B", "T", "Qd", "Qn" };
    public const string MONEY_FORMAT = "F2";
}
