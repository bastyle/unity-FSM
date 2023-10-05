using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine 
{
    public class State
    {
        public string Name;
        public System.Action onEnter;
        public System.Action onExit;
        public System.Action onStay;

        public override string ToString()
        {
            return Name;
        }
    }

    public Dictionary<string, State> states = new Dictionary<string, State>();

    public State currentState
    {
        get;
        private set;
    }

    public State initialState;

    // factory pattern - create state method
    public State CreateState(string name)
    {
        Debug.Log("CreateState:: " + name);
        State newState = new State();
        newState.Name = name;
        if (states.Count == 0)
        {
            initialState = newState;
        }
        states[name] = newState;
        return newState;
    }

    public void Update()
    {
        if (states.Count == 0 || initialState == null)
        {
            //throw new Exception("");
            Debug.LogErrorFormat("***** State Machine has no states or initialState is null");
            return;
        }

        if (currentState == null)
        {
            //Debug.Log("currentState::null ::: "+ initialState.Name);
            ChangeState(initialState);
        }

        if (currentState.onStay != null)
        {
            //Debug.Log("ccurrentState.onStay != null");
            ChangeState(currentState);
            currentState.onStay();
        }


    }

    public void ChangeState(State newState)
    {
        if (states.Count == 0 || initialState == null)
        {
            //throw new Exception("");
            Debug.LogErrorFormat("Can not change to a null state!");
            return;
        }
        // s1 exit
        if (currentState != null && currentState.onExit != null)
        {
            currentState.onExit();
        }

        // s1 => s2\
        Debug.Log("changing to state" + newState.Name);
        currentState = newState;

        // s2 enter
        if (newState.onEnter != null)
        {
            newState.onEnter();
        }
    }

    public void ChangeState(String newStateName)
    {
        Debug.Log("newState: " + newStateName);
        foreach (var kvp in states.Values)
        {
            Debug.Log($"Key: {kvp.Name}");
        }
        if (!states.ContainsKey(newStateName))
        {
            Debug.LogErrorFormat($"the state machine does not contain the state {newStateName}");
            return;
        }
        ChangeState(states[newStateName]);

    }
}
