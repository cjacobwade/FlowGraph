using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Luckshot.FSM
{
	[System.Serializable]
	public class StateMachine
	{
		protected MonoBehaviour owner = null;
		public MonoBehaviour Owner => owner;

		public StateMachine(MonoBehaviour inOwner)
		{ owner = inOwner; }
		
		private Dictionary<System.Type, StateMachineState> stateMap = new Dictionary<System.Type, StateMachineState>();

		public delegate void StateChangeEvent(StateMachine stateMachine, StateMachineState from, StateMachineState to);

		public event StateChangeEvent OnStateChanged = delegate { };

		[SerializeField]
		private StateMachineState currentState = null;
		public StateMachineState CurrentState => currentState;

		public bool IsInState(System.Type type)
		{ return currentState.GetType() == type; }

		public bool IsInState<T>()
		{ return currentState.GetType() == typeof(T); }

		public bool InAnyState(params System.Type[] types)
		{
			System.Type currentStateType = currentState.GetType();
			for (int i = 0; i < types.Length; i++)
			{
				if (types[i].IsAssignableFrom(currentStateType))
					return true;
			}

			return false;
		}

		public K GetState<K>() where K : StateMachineState
		{
			StateMachineState state = null;
			stateMap.TryGetValue(typeof(K), out state);
			return (K)state;
		}

		public void RegisterState(StateMachineState state)
		{
			if (!stateMap.ContainsKey(state.GetType()))
			{
				state.SetStateMachine(this);
				stateMap.Add(state.GetType(), state);
			}
		}

		public void ChangeState(StateMachineState state, StateParams stateParams = null)
		{ ChangeState(state.GetType(), stateParams); }

		public void ChangeState<J>(StateParams stateParams = null) where J : StateMachineState
		{ ChangeState(typeof(J), stateParams); }

		public void ChangeState(System.Type type, StateParams stateParams = null)
		{
			if (stateParams != null)
				stateParams.SetStateMachine(this);

			if (stateMap.TryGetValue(type, out StateMachineState nextState))
			{
				StateMachineState prevState = currentState;

				currentState = nextState;

				if (prevState != null)
					prevState.Exit();

				currentState.Enter(stateParams);

				OnStateChanged(this, prevState, nextState);
			}
			else
			{
				Debug.LogError("Tried to enter null state. This isn't supported");
			}
		}

		public void Tick()
		{ currentState.Tick(); }

		public void FixedTick()
		{ currentState.FixedTick(); }

		public void LateTick()
		{ currentState.LateTick(); }

		public void CollisionEnter(Collision collision)
		{ currentState.CollisionEnter(collision); }

		public void CollisionExit(Collision collision)
		{ currentState.CollisionExit(collision); }

		public void TriggerEnter(Collider collider)
		{ currentState.TriggerEnter(collider); }

		public void TriggerExit(Collider collider)
		{ currentState.TriggerExit(collider); }
	}


	[System.Serializable]
	public class StateMachine<T> : StateMachine where T : MonoBehaviour
	{
		public StateMachine(T inOwner) : base(inOwner)
		{
		}

		public new T Owner
		{ get { return owner as T; } }
	}
}