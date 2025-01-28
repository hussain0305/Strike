using System.Collections.Generic;
using UnityEngine;

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
    MainMenu
};

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
    public string name;
    public float weight;
    public float spin;
    public float bounce;
    public GameObject prefab;
}

public class Global : MonoBehaviour
{
    
}
