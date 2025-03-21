using System.Collections.Generic;
using UnityEngine;

//============---- ENUMS ----============

[System.Serializable]
public enum GameModeType
{
    Pins,
    Portals,
    Dartboard
}

public enum BallState
{
    OnTee,
    InControlledMotion,
    InPhysicsMotion
};

public enum CollectibleType
{
    Points,
    Multiple,
    Danger
};

public enum CollectibleParent
{
    World,
    UI
};

[System.Serializable]
public enum PointTokenType
{
    None,
    Cube_2x2,
    Cube_3x3,
    Cuboid_4x2,
    Pin_1x,
    Pin_2x,
    Pin_4x
};

[System.Serializable]
public enum MultiplierTokenType
{
    None,
    CircularTrigger,
    BoxCollision
};

public enum PinBehaviourPerTurn
{
    StayAsIs,
    Reset
}

[System.Serializable]
public enum WinCondition
{
    PointsRequired,
    PointsRanking
}

[System.Serializable]
public enum ButtonLocation
{
    GameHUD,
    MainMenu,
    BackButton,
    MainMenu_1,
    MainMenu_2,
    GameHUDAlt,
};

public enum GameContext
{
    InMenu,
    InGame,
    InPauseMenu,
    InQuitScreen
};

[System.Serializable]
public enum PFXType
{
    FlatHitEffect,
    HitPFX3D
};

[System.Serializable]
public enum Rarity
{
    Common,
    Rare,
    Epic,
    Legendary
};

[System.Serializable]
public enum GameEvent
{
    BallShot,
    NextShotCued
};

public enum GameState
{
    Menu,
    InGame,
    OnResultScreen
}

[System.Serializable]
public enum ButtonGroup
{
    Default,
    LevelSelection,
    BallSelection,
    CameraToggle,
    CamerBehaviour
}

//============---- STRUCTS ----============

public struct ShotInfo
{
    public int power;
    public int points;
    public Vector2 spin;
    public Vector2 angle;
    public List<Vector3> trajectory;
}

public struct PlayerGameData
{
    public string name;
    public int shotsTaken;
    public int totalPoints;
    public int projectileViewsRemaining;
    public List<ShotInfo> shotHistory;
}

[System.Serializable]
public struct ScalingData
{
    public Vector3 scaleInLevel;
    public Transform[] objectsToScale;
}

[System.Serializable]
public struct GameModeInfo
{
    public GameModeType gameMode;
    public string displayName;
    public string description;
    public int scene;
    public int starsRequiredToUnlock;
}

[System.Serializable]
public struct ButtonMaterials
{
    public ButtonLocation buttonLocation;
    public Material material;
}

[System.Serializable]
public struct BallProperties
{
    [Header("Display")]
    public string name;
    public string description;
    
    [Header("Rarity")]
    public Rarity rarity;

    [Header("Properties")]
    public float weight;
    public float spin;

    [Header("Construction")]
    public PhysicsMaterial physicsMaterial;
    public GameObject prefab;

    [Header("Meta")]
    public string id;
    public int cost;
}

[System.Serializable]
public struct RarityAppearance
{
    public Rarity rarity;
    public Color color;
    public Material material;
    public Material fontMaterial;
}

[System.Serializable]
public struct MinMaxInt
{
    public int Min;
    public int Max;

    public MinMaxInt(int min, int max)
    {
        Min = min;
        Max = max;
    }
    
    public int Clamp(int value) => Mathf.Clamp(value, Min, Max);
}

[System.Serializable]
public struct MinMaxFloat
{
    public float Min;
    public float Max;

    public MinMaxFloat(float min, float max)
    {
        Min = min;
        Max = max;
    }

    public float Clamp(float value) => Mathf.Clamp(value, Min, Max);
}

public static class Global
{
    public static LayerMask levelSurfaces = LayerMask.GetMask("Wall", "Ground");
}
