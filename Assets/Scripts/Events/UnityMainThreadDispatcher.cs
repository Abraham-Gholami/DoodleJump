using System.Collections.Generic;
using UnityEngine;
using System;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher _instance;

    public static UnityMainThreadDispatcher Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject dispatcherObject = new GameObject("MainThreadDispatcher");
                _instance = dispatcherObject.AddComponent<UnityMainThreadDispatcher>();
            }

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    private readonly Queue<Action> _executionQueue = new();

    public static void Enqueue(Action action)
    {
        lock (Instance._executionQueue)
        {
            Instance._executionQueue.Enqueue(action);
        }
    }

    private void Update()
    {
        while (_executionQueue.Count > 0)
        {
            var action = _executionQueue.Dequeue();
            action?.Invoke();
        }
    }
}