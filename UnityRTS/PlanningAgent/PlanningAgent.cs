using System.Collections.Generic;
using System.Linq;
using GameManager.EnumTypes;
using GameManager.GameElements;
using UnityEngine;
using UnityEngine.Assertions;

/////////////////////////////////////////////////////////////////////////////
/// This is the Moron Agent
/////////////////////////////////////////////////////////////////////////////

namespace GameManager
{
	abstract class State
	{
		public enum StateType
		{
			Build = 0,
			Attack,
			Win
		}
		protected enum Priority
		{
			Base,
			Barracks,
			Refinery,
			TrainWorker,
			TrainMelee,
			TrainArcher,
			AttackAll,
			Count
		}
		public enum BiasType
		{
			Worker = 0,
			Refinery,
			Soldier,
			Archer,
			Barracks,
			Count
		}

		public State( PlanningAgent agent )
		{
			a = agent;

			// Set up priorities to initially be empty.
			for( int i = 0; i < ( int )Priority.Count; ++i )
			{
				priorities.Add( ( Priority )i,0.0f );
			}
		}

		public abstract int UpdateModel();
		// Determine which action to take based on priorities.
		protected void CalcPrios()
		{
			float max = -1.0f;
			int n = 0;
			for( int i = 0; i < ( int )Priority.Count; ++i )
			{
				var curPrio = priorities[( Priority )i];
				if( curPrio > max )
				{
					max = curPrio;
					n = i;
				}
			}

			switch( n )
			{
				case 0: case 1: case 2:
					BuildNearMine( UnitType.BASE + n );
					break;
				case 3: case 4: case 5:
					TryTrain( ( UnitType )n - 2 );
					break;
				case ( int )Priority.AttackAll:
					a.AttackEnemy( a.mySoldiers );
					a.AttackEnemy( a.myArchers );
					break;
			}
		}

		// Average values.
		protected float Calculate( params float[] vals )
		{
			float total = 0.0f;
			for( int i = 0; i < vals.Count(); ++i )
			{
				total += vals[i];
			}
			return ( P( total / vals.Count(),1.0f ) );
		}
		// Clamp a value to 0-1 and scale by weight.
		protected float P( float val,float weight )
		{
			if( val > 1.0f ) val = 1.0f;
			if( val < 0.0f ) val = 0.0f;
			val *= weight;
			return( val );
		}
		// Return total number of units.
		protected int CountUnits()
		{
			return( a.myWorkers.Count + a.mySoldiers.Count +
				a.myArchers.Count );
		}
		// Find closest position to a mine.
		protected Vector3Int FindClosest( Vector3Int start )
		{
			Vector3Int pos = Vector3Int.zero;
			float minDist = 999999.0f;
			foreach( var i in a.mines )
			{
				var mine = ToUnit( i );
				var dist = GameManager.Instance.GetPathBetweenGridPositions(
					start,mine.GridPosition ).Count;
				if( dist < minDist )
				{
					minDist = dist;
					pos = mine.GridPosition;
				}
			}
			return ( pos );
		}
		// Try to build close to a mine.
		protected void BuildNearMine( UnitType type )
		{
			if( a.mines.Count < 1 ) return;
			if( a.myWorkers.Count < 1 ) return;

			List<Vector3Int> testPosList = new List<Vector3Int>();
			for( int i = 0; i < 50; ++i )
			{
				testPosList.Add( GameManager.Instance
					.GetRandomBuildableLocation( type ) );
			}

			var mine = ToUnit( a.mines[0] );
			var min = 99999999.0f;
			var pos = Vector3Int.zero;
			foreach( var testPos in testPosList )
			{
				var dist = ( mine.GridPosition - testPos ).sqrMagnitude;
				if( dist < min )
				{
					min = dist;
					pos = testPos;
				}
			}

			a.Build( ToUnit( a.myWorkers[0] ),pos,type );
		}
		// Simplify unit training.
		protected void TryTrain( UnitType type )
		{
			if( type == UnitType.WORKER && a.myBases.Count > 0 )
			{
				a.Train( GameManager.Instance.GetUnit( a.myBases[0] ),type );
			}
			else if( a.myBarracks.Count > 0 )
			{
				a.Train( GameManager.Instance.GetUnit(
					a.myBarracks[Random.Range( 0,a.myBarracks.Count )] ),type );
			}
		}
		// Shorthand for GameManager.Instance.GetUnit.
		protected Unit ToUnit( int unit )
		{
			return( GameManager.Instance.GetUnit( unit ) );
		}

		protected PlanningAgent a;
		protected Dictionary<Priority,float> priorities = new Dictionary<Priority,float>();
	}

	// Initial building phase.
	class BuildState
		:
		State
	{
		public BuildState( PlanningAgent agent )
			:
			base( agent )
		{}

		public override int UpdateModel()
		{
			// Set up priority weights.
			priorities[Priority.Base] = Calculate(
				P( 1 - a.myBases.Count,1.0f ),
				P( a.enemyBases.Count - a.myBases.Count,0.2f ),
				P( a.Gold / Constants.COST[UnitType.BASE],0.3f )
				);
			priorities[Priority.Barracks] = Calculate(
				P( 1 - a.myBarracks.Count,0.95f ),
				P( a.enemyBarracks.Count - a.myBarracks.Count,0.6f ),
				P( a.Gold / Constants.COST[UnitType.BARRACKS],0.3f ),
				P( a.biases[( int )BiasType.Barracks],1.0f )
				);
			priorities[Priority.Refinery] = Calculate(
				P( a.Gold / Constants.COST[UnitType.REFINERY],0.3f ),
				P( a.Gold > 3000 ? 1.0f : 0.0f,0.8f ),
				P( a.biases[( int )BiasType.Refinery],1.0f )
				);
			priorities[Priority.TrainWorker] = Calculate(
				P( 1 - a.myWorkers.Count,1.0f ),
				P( a.enemyWorkers.Count - a.myWorkers.Count,0.5f ),
				P( a.Gold / Constants.COST[UnitType.WORKER],0.1f ),
				P( PlanningAgent.MAX_NBR_WORKERS / Mathf.Max( a.myWorkers.Count,1 ),0.2f ),
				P( a.biases[( int )BiasType.Worker],1.0f )
				);
			priorities[Priority.TrainMelee] = Calculate(
				P( a.enemySoldiers.Count - a.mySoldiers.Count,0.7f ),
				P( a.enemyArchers.Count - a.mySoldiers.Count,0.6f ),
				P( a.Gold / Constants.COST[UnitType.SOLDIER],0.7f ),
				P( a.biases[( int )BiasType.Soldier],1.0f )
				);
			priorities[Priority.TrainArcher] = Calculate(
				P( a.enemySoldiers.Count - a.myArchers.Count,0.94f ),
				P( a.enemyArchers.Count - a.enemyArchers.Count,0.94f ),
				P( a.Gold / Constants.COST[UnitType.ARCHER],0.8f ),
				P( a.biases[( int )BiasType.Archer],1.0f )
				);

			CalcPrios();

			// Go to attack state.
			if( CountUnits() >= 13 && CountUnits() > a.enemyAgentNbr )
			{
				return( ( int )StateType.Attack );
			}

			return( ( int )StateType.Build );
		}
	}

	// Focus on increasing offense capacity by building barracks and training units.
	class AttackState
		:
		State
	{
		public AttackState( PlanningAgent agent )
			:
			base( agent )
		{ }

		public override int UpdateModel()
		{
			// Set up priority weights.
			priorities[Priority.Barracks] = Calculate(
				P( a.enemyBarracks.Count - a.myBarracks.Count,0.5f ),
				P( a.Gold / Constants.COST[UnitType.BARRACKS],0.2f ),
				P( 3 - a.myBarracks.Count,0.6f )
				);
			priorities[Priority.TrainMelee] = Calculate(
				P( a.enemySoldiers.Count - a.mySoldiers.Count,0.5f ),
				P( a.enemyArchers.Count - a.mySoldiers.Count,0.3f )
				);
			priorities[Priority.TrainArcher] = Calculate(
				P( a.enemySoldiers.Count - a.myArchers.Count,0.9f ),
				P( a.enemyArchers.Count - a.enemyArchers.Count,0.8f ),
				P( a.Gold / Constants.COST[UnitType.ARCHER],0.5f )
				);
			priorities[Priority.AttackAll] = Calculate(
				P( a.myArchers.Count - a.enemyArchers.Count,0.9f ),
				P( a.mySoldiers.Count - a.enemySoldiers.Count,0.7f )
				);

			CalcPrios();

			// Return to build state if we do not have enough offensive units.
			// If enemy has no more attack units go to win state.
			if( a.myWorkers.Count > CountUnits() / 2 )
			{
				return( ( int )StateType.Build );
			}
			else if( a.enemySoldiers.Count < 1 &&
				a.enemyArchers.Count < 1 )
			{
				return( ( int )StateType.Win );
			}

			return( ( int )StateType.Attack );
		}
	}

	// Force attack all enemies.
	class WinState
		:
		State
	{
		public WinState( PlanningAgent agent )
			:
			base( agent )
		{}

		public override int UpdateModel()
		{
			priorities[Priority.AttackAll] = 1.0f;

			// If we have fewer total units than the enemy return to attack state.
			if( a.enemyAgentNbr > CountUnits() )
			{
				return ( ( int )StateType.Attack );
			}

			return( ( int )StateType.Win );
		}
	}

	///<summary>Planning Agent is the over-head planner that decided where
	/// individual units go and what tasks they perform.  Low-level 
	/// AI is handled by other classes (like pathfinding).
	///</summary> 
	public class PlanningAgent : Agent
    {
        public const int MAX_NBR_WORKERS = 20;

		#region Private Data

		List<State> states;
		int curState = 0;

		public List<float> biases = new List<float>();

		List<List<float>> recordings = new List<List<float>>();

		const int recInterval = 120;
		int recTimer = 0;
		const float changePower = 0.1f;

		///////////////////////////////////////////////////////////////////////
		// Handy short-cuts for pulling all of the relevant data that you
		// might use for each decision.  Feel free to add your own.
		///////////////////////////////////////////////////////////////////////

		/// <summary>
		/// The enemy's agent number
		/// </summary>
		public int enemyAgentNbr { get; set; }

		/// <summary>
		/// My primary mine number
		/// </summary>
		public int mainMineNbr { get; set; }

		/// <summary>
		/// My primary base number
		/// </summary>
		public int mainBaseNbr { get; set; }

		/// <summary>
		/// List of all the mines on the map
		/// </summary>
		public List<int> mines { get; set; }

		/// <summary>
		/// List of all of my workers
		/// </summary>
		public List<int> myWorkers { get; set; }

		/// <summary>
		/// List of all of my soldiers
		/// </summary>
		public List<int> mySoldiers { get; set; }

		/// <summary>
		/// List of all of my archers
		/// </summary>
		public List<int> myArchers { get; set; }

		/// <summary>
		/// List of all of my bases
		/// </summary>
		public List<int> myBases { get; set; }

		/// <summary>
		/// List of all of my barracks
		/// </summary>
		public List<int> myBarracks { get; set; }

		/// <summary>
		/// List of all of my refineries
		/// </summary>
		public List<int> myRefineries { get; set; }

		/// <summary>
		/// List of the enemy's workers
		/// </summary>
		public List<int> enemyWorkers { get; set; }

		/// <summary>
		/// List of the enemy's soldiers
		/// </summary>
		public List<int> enemySoldiers { get; set; }

		/// <summary>
		/// List of enemy's archers
		/// </summary>
		public List<int> enemyArchers { get; set; }

		/// <summary>
		/// List of the enemy's bases
		/// </summary>
		public List<int> enemyBases { get; set; }

		/// <summary>
		/// List of the enemy's barracks
		/// </summary>
		public List<int> enemyBarracks { get; set; }

		/// <summary>
		/// List of the enemy's refineries
		/// </summary>
		public List<int> enemyRefineries { get; set; }

		/// <summary>
		/// List of the possible build positions for a 3x3 unit
		/// </summary>
		public List<Vector3Int> buildPositions { get; set; }

        /// <summary>
        /// Finds all of the possible build locations for a specific UnitType.
        /// Currently, all structures are 3x3, so these positions can be reused
        /// for all structures (Base, Barracks, Refinery)
        /// Run this once at the beginning of the game and have a list of
        /// locations that you can use to reduce later computation.  When you
        /// need a location for a build-site, simply pull one off of this list,
        /// determine if it is still buildable, determine if you want to use it
        /// (perhaps it is too far away or too close or not close enough to a mine),
        /// and then simply remove it from the list and build on it!
        /// This method is called from the Awake() method to run only once at the
        /// beginning of the game.
        /// </summary>
        /// <param name="unitType">the type of unit you want to build</param>
        public void FindProspectiveBuildPositions(UnitType unitType)
        {
            // For the entire map
            for (int i = 0; i < GameManager.Instance.MapSize.x; ++i)
            {
                for (int j = 0; j < GameManager.Instance.MapSize.y; ++j)
                {
                    // Construct a new point near gridPosition
                    Vector3Int testGridPosition = new Vector3Int(i, j, 0);

                    // Test if that position can be used to build the unit
                    if (Utility.IsValidGridLocation(testGridPosition)
                        && GameManager.Instance.IsBoundedAreaBuildable(unitType, testGridPosition))
                    {
                        // If this position is buildable, add it to the list
                        buildPositions.Add(testGridPosition);
                    }
                }
            }
        }

        /// <summary>
        /// Build a building
        /// </summary>
        /// <param name="unitType"></param>
        public void BuildBuilding(UnitType unitType)
        {
            // For each worker
            foreach (int worker in myWorkers)
            {
                // Grab the unit we need for this function
                Unit unit = GameManager.Instance.GetUnit(worker);

				// Make sure this unit actually exists and we have enough gold
				if( unit != null && Gold >= Constants.COST[unitType] )
				{
					// Find the closest build position to this worker's position (DUMB) and 
					// build the base there
					Vector3Int pos = Vector3Int.zero;
					float minDist = 99999999.0f;
					foreach( Vector3Int toBuild in buildPositions )
					{
						if( GameManager.Instance.IsBoundedAreaBuildable( unitType,toBuild ) )
						{
							var dist = ( toBuild - unit.GridPosition ).sqrMagnitude;
							if( dist < minDist )
							{
								minDist = dist;
								pos = toBuild;
							}
							// Build(unit, toBuild, unitType);
							// return;
						}
					}
					if( pos != Vector3Int.zero )
					{
						Build( unit,pos,unitType );
					}
                }
            }
        }

        /// <summary>
        /// Attack the enemy
        /// </summary>
        /// <param name="myTroops"></param>
        public void AttackEnemy(List<int> myTroops)
        {
            // For each of my troops in this collection
            foreach (int troopNbr in myTroops)
            {
                // If this troop is idle, give him something to attack
                Unit troopUnit = GameManager.Instance.GetUnit(troopNbr);
                if (troopUnit.CurrentAction == UnitAction.IDLE)
                {
                    // If there are archers to attack
                    if (enemyArchers.Count > 0)
                    {
                        Attack(troopUnit, GameManager.Instance.GetUnit(enemyArchers[Random.Range(0, enemyArchers.Count)]));
                    }
                    // If there are soldiers to attack
                    else if (enemySoldiers.Count > 0)
                    {
                        Attack(troopUnit, GameManager.Instance.GetUnit(enemySoldiers[Random.Range(0, enemySoldiers.Count)]));
                    }
                    // If there are workers to attack
                    else if (enemyWorkers.Count > 0)
                    {
                        Attack(troopUnit, GameManager.Instance.GetUnit(enemyWorkers[Random.Range(0, enemyWorkers.Count)]));
                    }
                    // If there are bases to attack
                    else if (enemyBases.Count > 0)
                    {
                        Attack(troopUnit, GameManager.Instance.GetUnit(enemyBases[Random.Range(0, enemyBases.Count)]));
                    }
                    // If there are barracks to attack
                    else if (enemyBarracks.Count > 0)
                    {
                        Attack(troopUnit, GameManager.Instance.GetUnit(enemyBarracks[Random.Range(0, enemyBarracks.Count)]));
                    }
                    // If there are refineries to attack
                    else if (enemyRefineries.Count > 0)
                    {
                        Attack(troopUnit, GameManager.Instance.GetUnit(enemyRefineries[Random.Range(0, enemyRefineries.Count)]));
                    }
                }
            }
        }

        #endregion

        #region Public Methods

		public PlanningAgent()
		{
			states = new List<State>()
			{
				new BuildState( this ),
				new AttackState( this ),
				new WinState( this )
			};

			for( int i = 0; i < ( int )State.BiasType.Count; ++i )
			{
				biases.Add( 0.4f );
			}
		}

        /// <summary>
        /// Called at the end of each round before remaining units are
        /// destroyed to allow the agent to observe the "win/loss" state
        /// </summary>
        public override void Learn()
        {
            Debug.Log("PlanningAgent::Learn");

			List<float> averages = new List<float>();
			
			// Populate averages with 0s.
			for( int i = 0; i < ( int )( State.BiasType.Count ) * 2; ++i )
			{
				averages.Add( 0.0f );
			}

			// Sum totals of recording numbers.
			foreach( var rec in recordings )
			{
				for( int i = 0; i < rec.Count; ++i )
				{
					averages[i] += rec[i];
				}
			}

			// Calculate averages.
			for( int i = 0; i < averages.Count; ++i )
			{
				averages[i] /= recordings.Count;
			}

			foreach( var avg in averages ) print( "<color=cyan>avg: " + avg );

			// Find difference to enemy score and balance based on that.
			for( int i = 0; i < ( int )State.BiasType.Count; ++i )
			{
				var diff = averages[i * 2] - averages[i * 2 + 1];
				if( Mathf.Abs( diff ) > 0.0f )
				{
					var change = diff / Mathf.Abs( diff );
					biases[i] += change * changePower;
				}
			}
        }

        /// <summary>
        /// Called before each match between two agents.  Matches have
        /// multiple rounds. 
        /// </summary>
        public override void InitializeMatch()
        {
	        Debug.Log("Mike's: " + AgentName);
			Debug.Log("PlanningAgent::InitializeMatch");
        }

        /// <summary>
        /// Called at the beginning of each round in a match.
        /// There are multiple rounds in a single match between two agents.
        /// </summary>
        public override void InitializeRound()
        {
            Debug.Log("PlanningAgent::InitializeRound");
            buildPositions = new List<Vector3Int>();

            FindProspectiveBuildPositions(UnitType.BASE);

            // Set the main mine and base to "non-existent"
            mainMineNbr = -1;
            mainBaseNbr = -1;

            // Initialize all of the unit lists
            mines = new List<int>();

            myWorkers = new List<int>();
            mySoldiers = new List<int>();
            myArchers = new List<int>();
            myBases = new List<int>();
            myBarracks = new List<int>();
            myRefineries = new List<int>();

            enemyWorkers = new List<int>();
            enemySoldiers = new List<int>();
            enemyArchers = new List<int>();
            enemyBases = new List<int>();
            enemyBarracks = new List<int>();
            enemyRefineries = new List<int>();

			curState = 0;

			recordings.Clear();
        }

        /// <summary>
        /// Updates the game state for the Agent - called once per frame for GameManager
        /// Pulls all of the agents from the game and identifies who they belong to
        /// </summary>
        public void UpdateGameState()
        {
            // Update the common resources
            mines = GameManager.Instance.GetUnitNbrsOfType(UnitType.MINE);

            // Update all of my unitNbrs
            myWorkers = GameManager.Instance.GetUnitNbrsOfType(UnitType.WORKER, AgentNbr);
            mySoldiers = GameManager.Instance.GetUnitNbrsOfType(UnitType.SOLDIER, AgentNbr);
            myArchers = GameManager.Instance.GetUnitNbrsOfType(UnitType.ARCHER, AgentNbr);
            myBarracks = GameManager.Instance.GetUnitNbrsOfType(UnitType.BARRACKS, AgentNbr);
            myBases = GameManager.Instance.GetUnitNbrsOfType(UnitType.BASE, AgentNbr);
            myRefineries = GameManager.Instance.GetUnitNbrsOfType(UnitType.REFINERY, AgentNbr);

            // Update the enemy agents & unitNbrs
            List<int> enemyAgentNbrs = GameManager.Instance.GetEnemyAgentNbrs(AgentNbr);
            if (enemyAgentNbrs.Any())
            {
                enemyAgentNbr = enemyAgentNbrs[0];
                enemyWorkers = GameManager.Instance.GetUnitNbrsOfType(UnitType.WORKER, enemyAgentNbr);
                enemySoldiers = GameManager.Instance.GetUnitNbrsOfType(UnitType.SOLDIER, enemyAgentNbr);
                enemyArchers = GameManager.Instance.GetUnitNbrsOfType(UnitType.ARCHER, enemyAgentNbr);
                enemyBarracks = GameManager.Instance.GetUnitNbrsOfType(UnitType.BARRACKS, enemyAgentNbr);
                enemyBases = GameManager.Instance.GetUnitNbrsOfType(UnitType.BASE, enemyAgentNbr);
                enemyRefineries = GameManager.Instance.GetUnitNbrsOfType(UnitType.REFINERY, enemyAgentNbr);
            }
        }

        /// <summary>
        /// Update the GameManager - called once per frame
        /// </summary>
        public override void Update()
        {
            UpdateGameState();

			if( ++recTimer > recInterval )
			{
				recTimer = 0;
				var data = new List<float>();
				data.Add( myWorkers.Count );
				data.Add( enemyWorkers.Count );
				data.Add( myRefineries.Count );
				data.Add( enemyRefineries.Count );
				data.Add( mySoldiers.Count );
				data.Add( enemySoldiers.Count );
				data.Add( myArchers.Count );
				data.Add( enemyArchers.Count );
				data.Add( myBarracks.Count );
				data.Add( enemyBarracks.Count );
				recordings.Add( data );
			}
			
			// Updating current state returns the next state to switch to.
			curState = states[curState].UpdateModel();

			AttackEnemy( mySoldiers );
			AttackEnemy( myArchers );

			if( myBases.Count > 0 ) mainBaseNbr = myBases[0];
			if( mines.Count > 0 ) mainMineNbr = mines[0];

			foreach( int worker in myWorkers )
			{
				// Grab the unit we need for this function
				Unit unit = GameManager.Instance.GetUnit( worker );

				// Make sure this unit actually exists and is idle
				if( unit != null && unit.CurrentAction == UnitAction.IDLE && mainBaseNbr >= 0 && mainMineNbr >= 0 )
				{
					// Grab the mine
					Unit mineUnit = GameManager.Instance.GetUnit( mainMineNbr );
					Unit baseUnit = GameManager.Instance.GetUnit( mainBaseNbr );
					if( mineUnit != null && baseUnit != null && mineUnit.Health > 0 )
					{
						Gather( unit,mineUnit,baseUnit );
					}
				}
			}
		}

        #endregion
    }
}

