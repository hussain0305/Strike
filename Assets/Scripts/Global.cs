using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#region Enums

[System.Serializable]
public enum GameModeType
{
    Pins,
    Portals,
    Deathmatch,
    DisappearingTokens,
    Dartboard,
    Endless //Always keep this last
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

public enum ObstacleType
{
    None,
    Fan,
    Window,
    SwitchDoor,
    Wall,
    SmallFan,
    SmallWall,
    SmallWallSeeThrough,
    WallSeeThrough, //8
    ForcePad,
    PhysicsPlank,
    SwitchFlipper,
    LongWall,
    SquareWall,
    HighPoweredForcePad,
    SmallHatch, //15
    WideWallSeeThrough,
    WreckingBall,
    SpeedNullifier
};

public enum Positioning
{
    OnPlatform,
    InWorld,
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
    Pin_4x,
    Brick,
    Cuboid_Gutter,
    Cube4x4,
    Hex_1x, //10
    Hex_2x,
    HexFlatTop_1x,
    HexFlatTop_2x
};

[System.Serializable]
public enum MultiplierTokenType
{
    None,
    CircularTrigger,
    BigMultiplier
};

[System.Serializable]
public enum DangerTokenType
{
    None,
    Cuboid_Gutter,
    Pin_2x,
    Cuboid_2x2,
    DangerGround
};

public enum PinBehaviourPerTurn
{
    StayAsIs,
    Reset,
    DisappearUponCollection,
    RefreshPin
}

[System.Serializable]
public enum WinCondition
{
    PointsRequired,
    PointsRanking,
    Survival
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
    FusionSlotButton,
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
    HitPFXCollectible,
    HitPFXObstacle,
    HitPFXDanger,
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

public enum EliminationReason
{
    None,
    HitDangerPin,
    HitNoting
};

#endregion

#region Structs

public struct ShotInfo
{
    public int power;
    public int points;
    public Vector2 spin;
    public Vector2 angle;
    public List<Vector3> trajectory;
}

public struct ShotData
{
    public int ownerIndex;
    public int pointsAccrued;
    public int multiplierAccrued;
    public bool hitNormalPint;
    public bool hitDangerPin;

    public void Reset()
    {
        ownerIndex = -1;
        pointsAccrued = 0;
        multiplierAccrued = 1;
        hitNormalPint = false;
        hitDangerPin = false;
    }

    public void StartLogging(int playerIndex)
    {
        Reset();
        ownerIndex = playerIndex;
    }
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
    public string[] rules;
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
    public AbilityAxis abilityAxis;
    public string abilityAxisText;
    public string abilityText;
    public AbilityModuleDefinition moduleDefinition;
    
    [Header("Construction")]
    public PhysicsMaterial physicsMaterial;
    public GameObject prefab;

    [Header("Meta")]
    public string id;
    public int cost;
    
    public IBallAbilityModule CreateModuleInstance()
    {
        return moduleDefinition?.CreateInstance();
    }
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

public struct GameEntityState
{
    public bool collected;
    public Vector3 position;
    public Quaternion rotation;
}

#endregion

#region Events

public class BallShotEvent { }
public class CueNextShotEvent { }
public class PreNextShotCuedEvent { }
public class InGameEvent { }
public class GameEndedEvent { }
public class GameExitedEvent { }
public class GameRestartedEvent { }

public class ShotCompleteEvent
{
    public List<Vector3> ShotTrajectory;
    public ShotCompleteEvent(List<Vector3> capturedTrajectory)
    {
        ShotTrajectory = capturedTrajectory;
    }
}

public class ProjectilesSpawnedEvent
{
    public GameObject[] projectiles;
    public ProjectilesSpawnedEvent(GameObject[] _projectiles)
    {
        projectiles = _projectiles;
    }
}

public class NewGameStartedEvent
{
    public int NumPlayers;

    public NewGameStartedEvent(int _numPlayers)
    {
        NumPlayers = _numPlayers;
    }
}

public class NextShotCuedEvent
{
    public int CurrentPlayerTurn;
    public NextShotCuedEvent()
    {
        CurrentPlayerTurn = 0;
    }
    public NextShotCuedEvent(int _currentPlayerTurn)
    {
        CurrentPlayerTurn = _currentPlayerTurn;
    }
}

public class NewRoundStartedEvent
{
    public int RoundNumber;
    public NewRoundStartedEvent(int roundNumber)
    {
        RoundNumber = roundNumber;
    }
}

public class ResultDeterminedEvent
{
    public bool IsPlayingSolo;
    public bool Won;
    
    public ResultDeterminedEvent(bool _isPlayingSolo, bool _won)
    {
        IsPlayingSolo = _isPlayingSolo;
        Won = _won;
    }
}

public class PlayerEliminatedEvent
{
    public int PlayerIndex;
    public EliminationReason EliminationReason;

    public PlayerEliminatedEvent(int playerIndex, EliminationReason eliminationReason)
    {
        this.PlayerIndex = playerIndex;
        this.EliminationReason = eliminationReason;
    }
}

public class CameraSwitchProcessedEvent
{
    public CameraHoistLocation NewCameraPos { get; }
    public float SwitchTime { get; }
    
    public CameraSwitchProcessedEvent(CameraHoistLocation newCameraPos, float time)
    {
        NewCameraPos = newCameraPos;
        SwitchTime = time;
    }
}

public class CameraSwitchCompletedEvent
{
    public CameraHoistLocation NewCameraPos { get; }

    public CameraSwitchCompletedEvent(CameraHoistLocation newCameraPos)
    {
        NewCameraPos = newCameraPos;
    }
}

public class GameModeChangedEvent
{
    public GameModeInfo GameModeInfo;
    public GameModeChangedEvent(GameModeInfo gameModeInfo)
    {
        GameModeInfo = gameModeInfo;
    }
}
public class NumPlayersChangedEvent
{
    public int numPlayers;
    public NumPlayersChangedEvent(int _num)
    {
        numPlayers = _num;
    }
}

public class GameStateChangedEvent
{
    public GameState gameState;

    public GameStateChangedEvent(GameState newGameState)
    {
        gameState = newGameState;
    }
}

public class MenuClosedEvent
{
    public MenuBase.MenuType ClosedMenu;
    public MenuClosedEvent(MenuBase.MenuType closedMenu)
    {
        ClosedMenu = closedMenu;
    }
}

#endregion

#region Endless Mode

[System.Serializable]
public struct EdgeDefinition
{
    public Transform end1;
    public Transform end2;
}

public struct AreaWorldBound
{
    public float xMin;
    public float xMax;
    public float zMin;
    public float zMax;

    public AreaWorldBound(float _xMin, float _xMax, float _zMin, float _zMax)
    {
        xMin = _xMin;
        xMax = _xMax;
        zMin = _zMin;
        zMax = _zMax;
    }

    public Vector3 Center()
    {
        return new Vector3((xMin + xMax) / 2, 0, (zMin + zMax) / 2);
    }
}

public struct SectorInfo
{
    public SectorCoord sectorCoord;
    public int numPointTokens;
    public int numObstacles;

    public SectorInfo(SectorCoord _sectorCoord, SectorSpawnPayload payload)
    {
        sectorCoord = _sectorCoord;
        numPointTokens = 0;
        numObstacles = 0;
        
        if (payload == null)
            return;
        
        foreach (var entry in payload.Entries)
        {
            if (entry.pointTokenType != PointTokenType.None)
            {
                numPointTokens++;
            }
            if (entry.obstacleType != ObstacleType.None)
            {
                numObstacles++;
            }
        }
    }
}

public struct SectorCoord
{
    public int x;
    public int z;

    public SectorCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    public bool Equals(SectorCoord other)
    {
        return x == other.x && z == other.z;
    }

    public override bool Equals(object obj)
    {
        return obj is SectorCoord other && Equals(other);
    }

    public override int GetHashCode()
    {
        return System.HashCode.Combine(x, z);
    }

    public static bool operator ==(SectorCoord a, SectorCoord b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(SectorCoord a, SectorCoord b)
    {
        return !a.Equals(b);
    }

    public override string ToString()
    {
        return $"[{x},{z}]";
    }
}

public struct AreaBoundingCoord
{
    public int xMin;
    public int xMax;
    public int zMin;
    public int zMax;

    public int xDimension;
    public int zDimension;

    public AreaBoundingCoord(int _xMin, int _zMin, int _xMax, int _zMax)
    {
        xMin = _xMin;
        xMax = _xMax;
        zMin = _zMin;
        zMax = _zMax;
        
        xDimension = xMax - xMin;
        zDimension = zMax - zMin;
    }
    
    public AreaBoundingCoord(SectorCoord[] sectors)
    {
        xMin = sectors.Min(s => s.x);
        xMax = sectors.Max(s => s.x);
        zMin = sectors.Min(s => s.z);
        zMax = sectors.Max(s => s.z);
        
        xDimension = xMax - xMin;
        zDimension = zMax - zMin;
    }

    public bool IsSquare()
    {
        return xDimension == zDimension;
    }

    public SectorCoord CenterCoord()
    {
        return new SectorCoord((xMin + xMax) / 2, (zMin + zMax) / 2);
    }
}

[System.Serializable]
public struct EndlessLevelWalls
{
    public Transform LeftSlat;
    public Transform[] LeftWall;
    public Transform[] LeftCap;
    public Transform[] RightCap;
    public Transform[] RightWall;
    public Transform RightSlat;
}

class SpawnedToken
{
    public Collectible collectible;
    public SectorInfo sectorInfo;
    public Vector3 position;
}

#endregion

public static class Global
{
    public static readonly string ResistComponentDeletionTag = "ResistComponentDeletion";
    public static readonly LayerMask GroundSurface = LayerMask.GetMask("Ground");
    public static readonly LayerMask LevelSurfaces = LayerMask.GetMask("Wall", "Ground");
    public static readonly LayerMask StickySurfaces = LayerMask.GetMask("CollideWithBall");

    public static readonly float BALL_RADIUS = 0.525f;
}