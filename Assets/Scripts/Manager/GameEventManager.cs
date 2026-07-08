using System;
using System.Collections.Generic;
using GGG.Tool.Singleton;
using UnityEngine;

public class GameEventManager : SingletonNonMono<GameEventManager>
{
    private readonly Dictionary<string, Delegate> _eventMap = new Dictionary<string, Delegate>();

    public void AddEvent(string eventName, Action action)
    {
        AddDelegate(eventName, action);
    }

    public void AddEvent<T>(string eventName, Action<T> action)
    {
        AddDelegate(eventName, action);
    }

    public void AddEvent<T1, T2>(string eventName, Action<T1, T2> action)
    {
        AddDelegate(eventName, action);
    }

    public void AddEvent<T1, T2, T3>(string eventName, Action<T1, T2, T3> action)
    {
        AddDelegate(eventName, action);
    }

    public void RemoveEvent(string eventName, Action action)
    {
        RemoveDelegate(eventName, action);
    }

    public void RemoveEvent<T>(string eventName, Action<T> action)
    {
        RemoveDelegate(eventName, action);
    }

    public void RemoveEvent<T1, T2>(string eventName, Action<T1, T2> action)
    {
        RemoveDelegate(eventName, action);
    }

    public void RemoveEvent<T1, T2, T3>(string eventName, Action<T1, T2, T3> action)
    {
        RemoveDelegate(eventName, action);
    }

    public void Call(string eventName)
    {
        if (TryGetAction<Action>(eventName, out Action action))
            action.Invoke();
    }

    public void Call<T>(string eventName, T arg)
    {
        if (TryGetAction<Action<T>>(eventName, out Action<T> action))
            action.Invoke(arg);
    }

    public void Call<T1, T2>(string eventName, T1 arg1, T2 arg2)
    {
        if (TryGetAction<Action<T1, T2>>(eventName, out Action<T1, T2> action))
            action.Invoke(arg1, arg2);
    }

    public void Call<T1, T2, T3>(string eventName, T1 arg1, T2 arg2, T3 arg3)
    {
        if (TryGetAction<Action<T1, T2, T3>>(eventName, out Action<T1, T2, T3> action))
            action.Invoke(arg1, arg2, arg3);
    }

    public void ClearEvent(string eventName)
    {
        _eventMap.Remove(eventName);
    }

    public void ClearAllEvents()
    {
        _eventMap.Clear();
    }

    private void AddDelegate(string eventName, Delegate callback)
    {
        if (!CanUseEvent(eventName, callback))
            return;

        if (_eventMap.TryGetValue(eventName, out Delegate current))
        {
            if (current.GetType() != callback.GetType())
            {
                Debug.LogError($"Event {eventName} already exists with a different signature.");
                return;
            }

            _eventMap[eventName] = Delegate.Combine(current, callback);
            return;
        }

        _eventMap.Add(eventName, callback);
    }

    private void RemoveDelegate(string eventName, Delegate callback)
    {
        if (!CanUseEvent(eventName, callback))
            return;

        if (!_eventMap.TryGetValue(eventName, out Delegate current))
            return;

        if (current.GetType() != callback.GetType())
        {
            Debug.LogError($"Event {eventName} remove failed because the signature does not match.");
            return;
        }

        Delegate next = Delegate.Remove(current, callback);
        if (next == null)
            _eventMap.Remove(eventName);
        else
            _eventMap[eventName] = next;
    }

    private bool TryGetAction<TAction>(string eventName, out TAction action) where TAction : class
    {
        action = null;
        if (string.IsNullOrEmpty(eventName))
            return false;

        if (!_eventMap.TryGetValue(eventName, out Delegate callback))
            return false;

        action = callback as TAction;
        if (action != null)
            return true;

        Debug.LogError($"Event {eventName} call failed because the signature does not match.");
        return false;
    }

    private static bool CanUseEvent(string eventName, Delegate callback)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogError("Event name cannot be empty.");
            return false;
        }

        if (callback != null)
            return true;

        Debug.LogError($"Event {eventName} callback cannot be null.");
        return false;
    }
}
