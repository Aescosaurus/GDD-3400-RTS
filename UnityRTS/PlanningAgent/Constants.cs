using System.Collections.Generic;
using UnityEngine;
using GameManager.EnumTypes;
using UnityEditor;

namespace GameManager
{
    /// <summary>
    /// Constants - set of game-defining constants.
    /// </summary>
    public class Constants
    {
        #region Game Configuration

        /// <summary>
        /// String that represents the name of the Human agent
        /// </summary>
        public const string HUMAN_NAME = "Human";

        /// <summary>
        /// String that represents the name of the Orc agent
        /// </summary>
        public const string ORC_NAME = "Orc";

        /// <summary>
        /// InitializeRound the game constants
        /// </summary>
        internal static void CalculateGameConstants()
        {
            SCALAR_MOVING_SPEED = GAME_SPEED;
            MOVING_SPEED = new Dictionary<UnitType, float>()
            {
                { UnitType.MINE,        0.0f},
                { UnitType.WORKER,      SCALAR_MOVING_SPEED * 0.1f},
                { UnitType.SOLDIER,     SCALAR_MOVING_SPEED * 0.2f },
                { UnitType.ARCHER,      SCALAR_MOVING_SPEED * 0.2f },
                { UnitType.BASE,        0.0f },
                { UnitType.BARRACKS,    0.0f },
                { UnitType.REFINERY,    0.0f},
            };

            SCALAR_MINING_SPEED = GAME_SPEED;
            MINING_SPEED = new Dictionary<UnitType, float>()
            {
                { UnitType.MINE,        0.0f},
                { UnitType.WORKER,      SCALAR_MINING_SPEED * MINING_BOOST * 20.0f},
                { UnitType.SOLDIER,     0.0f },
                { UnitType.ARCHER,      0.0f },
                { UnitType.BASE,        0.0f },
                { UnitType.BARRACKS,    0.0f },
                { UnitType.REFINERY,    0.0f},
            };

            SCALAR_CREATION_TIME = 1f / GAME_SPEED;
            CREATION_TIME = new Dictionary<UnitType, float>()
            {
                { UnitType.MINE,        0.0f },
                { UnitType.WORKER,      SCALAR_CREATION_TIME * 2 },
                { UnitType.SOLDIER,     SCALAR_CREATION_TIME * 4 },
                { UnitType.ARCHER,      SCALAR_CREATION_TIME * 5 },
                { UnitType.BASE,        SCALAR_CREATION_TIME * 10 },
                { UnitType.BARRACKS,    SCALAR_CREATION_TIME * 15 },
                { UnitType.REFINERY,    SCALAR_CREATION_TIME * 15 },
            };

            SCALAR_DAMAGE = GAME_SPEED;
            DAMAGE = new Dictionary<UnitType, float>()
            {
                { UnitType.MINE,        0.0f },
                { UnitType.WORKER,      0.0f },
                { UnitType.SOLDIER,     20.0f * SCALAR_DAMAGE },
                { UnitType.ARCHER,      10.0f * SCALAR_DAMAGE},
                { UnitType.BASE,        0.0f },
                { UnitType.BARRACKS,    0.0f },
                { UnitType.REFINERY,    0.0f },
            };

        }

        /// <summary>
        /// GAME_SPEED - increase this value to make the game go faster
        /// </summary>
        internal static int GAME_SPEED = 1;

        /// <summary>
        /// Maximum game speed
        /// </summary>
        internal readonly static int MAX_GAME_SPEED = 30;

        /// <summary>
        /// Health associated with each unit (Mine health is amount of gold)
        /// </summary>
        public Dictionary<UnitType, float> Health
        {
            get { return HEALTH; }
        }

        /// <summary>
        /// Damage associated with each unit
        /// </summary>
        public Dictionary<UnitType, float> Damage
        {
            get { return DAMAGE; }
        }

        /// <summary>
        /// Moving speed associated with each unit
        /// </summary>
        public Dictionary<UnitType, float> MovingSpeed
        {
            get { return MOVING_SPEED; }
        }

        /// <summary>
        /// Mining speed associated with each unit
        /// </summary>
        public Dictionary<UnitType, float> MiningSpeed
        {
            get { return MINING_SPEED; }
        }

        /// <summary>
        /// Mining time associated with each unit
        /// </summary>
        public Dictionary<UnitType, float> MiningCapacity
        {
            get { return MINING_CAPACITY; }
        }

        /// <summary>
        /// Cost associated with each unit
        /// </summary>
        public Dictionary<UnitType, float> Cost
        {
            get { return COST; }
        }

        /// <summary>
        /// Creation time associated with each unit
        /// </summary>
        public Dictionary<UnitType, float> CreationTime
        {
            get { return CREATION_TIME; }
        }

        /// <summary>
        /// Dependencies associated with each unit
        /// </summary>
        public Dictionary<UnitType, List<UnitType>> Dependency
        {
            get { return DEPENDENCY; }
        }

        /// <summary>
        /// Builds associated with each unit
        /// </summary>
        public Dictionary<UnitType, List<UnitType>> Builds
        {
            get { return BUILDS; }
        }

        /// <summary>
        /// Trains associated with each unit
        /// </summary>
        public Dictionary<UnitType, List<UnitType>> Trains
        {
            get { return TRAINS; }
        }

        /// <summary>
        /// Can unit move
        /// </summary>
        public Dictionary<UnitType, bool> CanMove
        {
            get { return CAN_MOVE; }
        }

        /// <summary>
        /// Can unit build
        /// </summary>
        public Dictionary<UnitType, bool> CanBuild
        {
            get { return CAN_BUILD; }
        }

        /// <summary>
        /// Can unit train
        /// </summary>
        public Dictionary<UnitType, bool> CanTrain
        {
            get { return CAN_TRAIN; }
        }

        /// <summary>
        /// Can unit attack
        /// </summary>
        public Dictionary<UnitType, bool> CanAttack
        {
            get { return CAN_ATTACK; }
        }

        /// <summary>
        /// Can unit gather
        /// </summary>
        public Dictionary<UnitType, bool> CanGather
        {
            get { return CAN_GATHER; }
        }

        /// <summary>
        /// Unit Attack range
        /// </summary>
        public Dictionary<UnitType, float> AttackRange
        {
            get { return ATTACK_RANGE; }
        }

        /// <summary>
        /// Unit size
        /// </summary>
        public Dictionary<UnitType, Vector3Int> UnitSize
        {
            get { return UNIT_SIZE; }
        }


        #endregion

        #region Static Arrays

        /// <summary>
        /// Directions used to control unit animations
        /// </summary>
        internal static readonly Dictionary<Direction, Vector3Int> directions = new Dictionary<Direction, Vector3Int>()
        {
            { Direction.S,  (Vector3Int.down) },
            { Direction.SE, (new Vector3Int(1, -1, 0)) },
            { Direction.E,  (Vector3Int.right) },
            { Direction.NE, (new Vector3Int(1, 1, 0)) },
            { Direction.N,  (Vector3Int.up) },
            { Direction.NW, (new Vector3Int(-1, 1, 0)) },
            { Direction.W,  (Vector3Int.left) },
            { Direction.SW, (new Vector3Int(-1, -1, 0)) }
        };

        /// <summary>
        /// Initial health associated with each unit
        /// </summary>
        public static readonly Dictionary<UnitType, float> HEALTH = new Dictionary<UnitType, float>()
        {
            { UnitType.MINE,        GameManager.Instance.StartingMineGold },
            { UnitType.WORKER,      50.0f },
            { UnitType.SOLDIER,     100.0f },
            { UnitType.ARCHER,      75.0f },
            { UnitType.BASE,        1000.0f },
            { UnitType.BARRACKS,    500.0f },
            { UnitType.REFINERY,    500.0f },
        };

        /// <summary>
        /// Primary damage value
        /// </summary>
        internal static float SCALAR_DAMAGE;

        /// <summary>
        /// Initial damage associated with each unit
        /// </summary>
        public static Dictionary<UnitType, float> DAMAGE;

        /// <summary>
        /// Primary moving speed that all troops are scaled by
        /// </summary>
        internal static float SCALAR_MOVING_SPEED;

        /// <summary>
        /// Speed at which each unit moves
        /// </summary>
        internal static Dictionary<UnitType, float> MOVING_SPEED;

        /// <summary>
        /// Mining speed constants (per second)
        /// </summary>		
        internal static float SCALAR_MINING_SPEED;

        /// <summary>
        /// Speed at which each unit mines resources
        /// </summary>
        internal static Dictionary<UnitType, float> MINING_SPEED;

        /// <summary>
        /// Time constants
        /// </summary>
        internal static float SCALAR_MINING_CAPACITY = 10f;

        /// <summary>
        /// Time to mine a resource in seconds
        /// </summary>
        public static readonly Dictionary<UnitType, float> MINING_CAPACITY = new Dictionary<UnitType, float>()
        {
            { UnitType.MINE,        0.0f },
            { UnitType.WORKER,      SCALAR_MINING_CAPACITY * 10.0f },
            { UnitType.SOLDIER,     0.0f },
            { UnitType.ARCHER,      0.0f },
            { UnitType.BASE,        0.0f },
            { UnitType.BARRACKS,    0.0f },
            { UnitType.REFINERY,    0.0f },
        };

        /// <summary>
        /// Cost constants
        /// </summary>
        internal static float SCALAR_COST = 50f;

        /// <summary>
        /// Cost to build each unit
        /// </summary>
        public static readonly Dictionary<UnitType, float> COST = new Dictionary<UnitType, float>()
        {
            { UnitType.MINE,        0.0f },
            { UnitType.WORKER,      SCALAR_COST },
            { UnitType.SOLDIER,     SCALAR_COST * 2 },
            { UnitType.ARCHER,      SCALAR_COST * 3 },
            { UnitType.BASE,        SCALAR_COST * 10 },
            { UnitType.BARRACKS,    SCALAR_COST * 7 },
            { UnitType.REFINERY,    SCALAR_COST * 6 },
        };

        /// <summary>
        /// Creation time constants
        /// </summary>
        internal static float SCALAR_CREATION_TIME;

        /// <summary>
        /// Time to create each unit in seconds
        /// </summary>
        internal static Dictionary<UnitType, float> CREATION_TIME;

        /// <summary>
        /// Dependencies of each unit in order to build/train them
        /// </summary>
        public static readonly Dictionary<UnitType, List<UnitType>> DEPENDENCY = new Dictionary<UnitType, List<UnitType>>()
        {
            { UnitType.MINE,        new List<UnitType>() { } },
            { UnitType.WORKER,      new List<UnitType>() { UnitType.BASE } },
            { UnitType.SOLDIER,     new List<UnitType>() { UnitType.BARRACKS } },
            { UnitType.ARCHER,      new List<UnitType>() { UnitType.BARRACKS } },
            { UnitType.BASE,        new List<UnitType>() { } },
            { UnitType.BARRACKS,    new List<UnitType>() { UnitType.BASE } },
            { UnitType.REFINERY,    new List<UnitType>() { UnitType.BASE, UnitType.BARRACKS } },
        };

        /// <summary>
        /// Set of Units built by each unit
        /// </summary>
        public static readonly Dictionary<UnitType, List<UnitType>> BUILDS = new Dictionary<UnitType, List<UnitType>>()
        {
            { UnitType.MINE,        new List<UnitType>() { } },
            { UnitType.WORKER,      new List<UnitType>() { UnitType.BASE, UnitType.BARRACKS, UnitType.REFINERY } },
            { UnitType.SOLDIER,     new List<UnitType>() { } },
            { UnitType.ARCHER,      new List<UnitType>() { } },
            { UnitType.BASE,        new List<UnitType>() { } },
            { UnitType.BARRACKS,    new List<UnitType>() { } },
            { UnitType.REFINERY,    new List<UnitType>() { } },
        };

        /// <summary>
        /// Set of Units trained by each unit
        /// </summary>
        public static readonly Dictionary<UnitType, List<UnitType>> TRAINS = new Dictionary<UnitType, List<UnitType>>()
        {
            { UnitType.MINE,        new List<UnitType>() { } },
            { UnitType.WORKER,      new List<UnitType>() { } },
            { UnitType.SOLDIER,     new List<UnitType>() { } },
            { UnitType.ARCHER,      new List<UnitType>() { } },
            { UnitType.BASE,        new List<UnitType>() { UnitType.WORKER } },
            { UnitType.BARRACKS,    new List<UnitType>() { UnitType.SOLDIER, UnitType.ARCHER } },
            { UnitType.REFINERY,    new List<UnitType>() { } },
        };

        /// <summary>
        /// Which Units can move
        /// </summary>
        public static readonly Dictionary<UnitType, bool> CAN_MOVE = new Dictionary<UnitType, bool>()
        {
            { UnitType.MINE,        false },
            { UnitType.WORKER,      true },
            { UnitType.SOLDIER,     true },
            { UnitType.ARCHER,      true },
            { UnitType.BASE,        false },
            { UnitType.BARRACKS,    false },
            { UnitType.REFINERY,    false },
        };

        /// <summary>
        /// Which Units can build
        /// </summary>
        public static readonly Dictionary<UnitType, bool> CAN_BUILD = new Dictionary<UnitType, bool>()
        {
            { UnitType.MINE,        false },
            { UnitType.WORKER,      true },
            { UnitType.SOLDIER,     false },
            { UnitType.ARCHER,      false },
            { UnitType.BASE,        false },
            { UnitType.BARRACKS,    false },
            { UnitType.REFINERY,    false },
        };

        /// <summary>
        /// Which Units can train
        /// </summary>
        public static readonly Dictionary<UnitType, bool> CAN_TRAIN = new Dictionary<UnitType, bool>()
        {
            { UnitType.MINE,        false },
            { UnitType.WORKER,      false },
            { UnitType.SOLDIER,     false },
            { UnitType.ARCHER,      false },
            { UnitType.BASE,        true },
            { UnitType.BARRACKS,    true },
            { UnitType.REFINERY,    false },
        };

        /// <summary>
        /// Which Units can attack
        /// </summary>
        public static readonly Dictionary<UnitType, bool> CAN_ATTACK = new Dictionary<UnitType, bool>()
        {
            { UnitType.MINE,        false },
            { UnitType.WORKER,      false },
            { UnitType.SOLDIER,     true },
            { UnitType.ARCHER,      true },
            { UnitType.BASE,        false },
            { UnitType.BARRACKS,    false },
            { UnitType.REFINERY,    false },
        };

        /// <summary>
        /// Which Units can gather
        /// </summary>
        public static readonly Dictionary<UnitType, bool> CAN_GATHER = new Dictionary<UnitType, bool>()
        {
            { UnitType.MINE,        false },
            { UnitType.WORKER,      true },
            { UnitType.SOLDIER,     false },
            { UnitType.ARCHER,      false },
            { UnitType.BASE,        false },
            { UnitType.BARRACKS,    false },
            { UnitType.REFINERY,    false },
        };

        /// <summary>
        /// Speed at which each unit moves
        /// </summary>
        public static readonly Dictionary<UnitType, float> ATTACK_RANGE = new Dictionary<UnitType, float>()
        {
            { UnitType.MINE,        0.0f },
            { UnitType.WORKER,      0.0f },
            { UnitType.SOLDIER,     1.5f },
            { UnitType.ARCHER,      5.0f },
            { UnitType.BASE,        0.0f },
            { UnitType.BARRACKS,    0.0f },
            { UnitType.REFINERY,    0.0f },
        };

        /// <summary>
        /// Raw unit sizes (since sprites aren't always square...)
        /// </summary>
        public static readonly Dictionary<UnitType, Vector3Int> UNIT_SIZE = new Dictionary<UnitType, Vector3Int>()
        {
            { UnitType.MINE,        new Vector3Int(3, 3, 0) },
            { UnitType.WORKER,      new Vector3Int(1, 1, 0) },
            { UnitType.SOLDIER,     new Vector3Int(1, 1, 0) },
            { UnitType.ARCHER,      new Vector3Int(1, 1, 0) },
            { UnitType.BASE,        new Vector3Int(3, 3, 0) },
            { UnitType.BARRACKS,    new Vector3Int(3, 3, 0) },
            { UnitType.REFINERY,    new Vector3Int(3, 3, 0) },
        };

        /// <summary>
        /// Gold mining boost for refineries
        /// </summary>
        public static readonly float MINING_BOOST = 2.0f;

        #endregion
    }

}
