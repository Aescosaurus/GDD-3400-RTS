// using GameManager.EnumTypes;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using UnityEngine;
// 
// namespace GameManager
// {
// 	abstract class State
// 	{
// 		public enum StateType
// 		{
// 			Build = 0,
// 			Attack,
// 			Win
// 		}
// 
// 		public State( PlanningAgent agent )
// 		{
// 			this.a = agent;
// 		}
// 
// 		public abstract int UpdateModel();
// 
// 		protected float Calculate( params float[] vals )
// 		{
// 			float total = 0.0f;
// 			for( int i = 0; i < vals.Count(); ++i )
// 			{
// 				total += vals[i];
// 			}
// 			return( P( total / vals.Count(),1.0f ) );
// 		}
// 		protected float P( float val,float weight )
// 		{
// 			if( val > 1.0f ) val = 1.0f;
// 			if( val < 0.0f ) val = 0.0f;
// 			val *= weight;
// 			return ( val );
// 		}
// 		protected int CountUnits()
// 		{
// 			return( a.myWorkers.Count + a.mySoldiers.Count +
// 				a.myArchers.Count );
// 		}
// 		protected Vector3Int FindClosest( Vector3Int start )
// 		{
// 			Vector3Int pos = Vector3Int.zero;
// 			float minDist = 999999.0f;
// 			foreach( var i in a.mines )
// 			{
// 				var mine = ToUnit( i );
// 				var dist = GameManager.Instance.GetPathBetweenGridPositions(
// 					start,mine.GridPosition ).Count;
// 				if( dist < minDist )
// 				{
// 					minDist = dist;
// 					pos = mine.GridPosition;
// 				}
// 			}
// 			return( pos );
// 		}
// 		protected void BuildNearMine( UnitType type )
// 		{
// 			if( a.mines.Count < 1 ) return;
// 			if( a.myWorkers.Count < 1 ) return;
// 
// 			List<Vector3Int> testPosList = new List<Vector3Int>();
// 			for( int i = 0; i < 50; ++i )
// 			{
// 				testPosList.Add( GameManager.Instance
// 					.GetRandomBuildableLocation( type ) );
// 			}
// 
// 			var mine = ToUnit( a.mines[0] );
// 			var min = 99999999.0f;
// 			var pos = Vector3Int.zero;
// 			foreach( var testPos in testPosList )
// 			{
// 				var dist = ( mine.GridPosition - testPos ).sqrMagnitude;
// 				if( dist < min )
// 				{
// 					min = dist;
// 					pos = testPos;
// 				}
// 			}
// 
// 			a.Build( ToUnit( a.myWorkers[0] ),pos,type );
// 		}
// 		protected void TryTrain( UnitType type )
// 		{
// 			// a.Train( GameManager.Instance.GetUnit( a.myBases[0] ),UnitType.WORKER );
// 			if( type == UnitType.WORKER && a.myBases.Count > 0 )
// 			{
// 				a.Train( GameManager.Instance.GetUnit( a.myBases[0] ),type );
// 			}
// 			else if( a.myBarracks.Count > 0 )
// 			{
// 				a.Train( GameManager.Instance.GetUnit(
// 					a.myBarracks[0] ),type );
// 			}
// 		}
// 		protected GameElements.Unit ToUnit( int unit )
// 		{
// 			return( GameManager.Instance.GetUnit( unit ) );
// 		}
// 
// 		protected PlanningAgent a;
// 	}
// 
// 	class BuildState
// 		:
// 		State
// 	{
// 		enum Priority
// 		{
// 			Base,
// 			Barracks,
// 			Refinery,
// 			TrainWorker,
// 			TrainMelee,
// 			TrainArcher,
// 			Count
// 		}
// 
// 		Dictionary<Priority,float> priorities = new Dictionary<Priority,float>();
// 
// 		public BuildState( PlanningAgent agent )
// 			:
// 			base( agent )
// 		{
// 			for( int i = 0; i < ( int )Priority.Count; ++i )
// 			{
// 				priorities.Add( ( Priority )i,0.0f );
// 			}
// 		}
// 
// 		public override int UpdateModel()
// 		{
// 			// Set up priority weights.
// 			priorities[Priority.Base] = Calculate(
// 				P( 1 - a.myBases.Count,1.0f ),
// 				P( a.enemyBases.Count - a.myBases.Count,0.2f ),
// 				P( a.Gold / Constants.COST[UnitType.BASE],0.3f )
// 				);
// 			priorities[Priority.Barracks] = Calculate(
// 				P( 1 - a.myBarracks.Count,1.0f ),
// 				P( a.enemyBarracks.Count - a.myBarracks.Count,0.6f ),
// 				P( a.Gold / Constants.COST[UnitType.BARRACKS],0.3f )
// 				);
// 			priorities[Priority.Refinery] = Calculate(
// 				P( a.Gold / Constants.COST[UnitType.REFINERY],0.3f ),
// 				P( a.Gold > 3000 ? 1.0f : 0.0f,0.8f )
// 				);
// 			priorities[Priority.TrainWorker] = Calculate(
// 				P( 1 - a.myWorkers.Count,1.0f ),
// 				P( a.enemyWorkers.Count - a.myWorkers.Count,0.5f ),
// 				P( a.Gold / Constants.COST[UnitType.WORKER],0.1f ),
// 				P( PlanningAgent.MAX_NBR_WORKERS / Math.Max( a.myWorkers.Count,1 ),0.2f )
// 				);
// 			priorities[Priority.TrainMelee] = Calculate(
// 				P( a.enemySoldiers.Count - a.mySoldiers.Count,1.0f ),
// 				P( a.enemyArchers.Count - a.mySoldiers.Count,0.6f ),
// 				P( a.Gold / Constants.COST[UnitType.SOLDIER],0.7f )
// 				);
// 			priorities[Priority.TrainArcher] = Calculate(
// 				P( a.enemyArchers.Count - a.enemyArchers.Count,0.7f ),
// 				P( a.Gold / Constants.COST[UnitType.ARCHER],0.7f )
// 				);
// 
// 			float max = -1.0f;
// 			int n = 0;
// 			for( int i = 0; i < ( int )Priority.Count; ++i )
// 			{
// 				var curPrio = priorities[( Priority )i];
// 				if( curPrio > max )
// 				{
// 					max = curPrio;
// 					n = i;
// 				}
// 			}
// 
// 			switch( n )
// 			{
// 				case 0:
// 				case 1:
// 				case 2:
// 					// a.BuildBuilding( UnitType.BASE + n );
// 					BuildNearMine( UnitType.BASE + n );
// 					break;
// 				case 3: case 4: case 5:
// 					TryTrain( ( UnitType )n - 2 );
// 					break;
// 			}
// 
// 			if( CountUnits() >= 13 )
// 			{
// 				return( ( int )StateType.Attack );
// 			}
// 
// 			return( ( int )StateType.Build );
// 		}
// 	}
// 
// 	class AttackState
// 		:
// 		State
// 	{
// 		public AttackState( PlanningAgent agent )
// 			:
// 			base( agent )
// 		{}
// 
// 		public override int UpdateModel()
// 		{
// 			a.AttackEnemy( a.mySoldiers );
// 
// 			if( a.myWorkers.Count > CountUnits() / 2 )
// 			{
// 				return( ( int )StateType.Build );
// 			}
// 			else if( a.enemyAgentNbr < CountUnits() )
// 			{
// 				return( ( int )StateType.Win );
// 			}
// 
// 			return ( ( int )StateType.Attack );
// 		}
// 	}
// 
// 	class WinState
// 		:
// 		State
// 	{
// 		public WinState( PlanningAgent agent )
// 			:
// 			base( agent )
// 		{}
// 
// 		public override int UpdateModel()
// 		{
// 			a.AttackEnemy( a.mySoldiers );
// 			a.AttackEnemy( a.myArchers );
// 
// 			if( a.enemyAgentNbr > CountUnits() )
// 			{
// 				return( ( int )StateType.Attack );
// 			}
// 
// 			return( ( int )StateType.Win );
// 		}
// 	}
// }
