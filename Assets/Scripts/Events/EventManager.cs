using System;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Threading;

public static class EventManager
{
    private static readonly Dictionary<string, UnityEvent<Dictionary<string, object>>> EventDictionary = new();
    private static readonly Dictionary<string, int> EventNumberOfListenersDictionary = new();

    // =======================
    // Start Listening Methods
    // =======================

    public static void StartListening<T>(T eventName, UnityAction listener) where T : Enum
    {
        var thisEvent = GetEvent(eventName);
        thisEvent.AddListener((_) => listener.Invoke());
    }

    public static void StartListening<T, T1>(T eventName, UnityAction<T1> listener) where T : Enum
    {
        var thisEvent = GetEvent(eventName);
        thisEvent.AddListener((parameters) => listener.Invoke(
            (T1)parameters["param1"]
        ));
    }

    public static void StartListening<T, T1, T2>(T eventName, UnityAction<T1, T2> listener) where T : Enum
    {
        var thisEvent = GetEvent(eventName);
        thisEvent.AddListener((parameters) => listener.Invoke(
            (T1)parameters["param1"],
            (T2)parameters["param2"]
        ));
    }

    public static void StartListening<T, T1, T2, T3>(T eventName, UnityAction<T1, T2, T3> listener) where T : Enum
    {
        var thisEvent = GetEvent(eventName);
        thisEvent.AddListener((parameters) => listener.Invoke(
            (T1)parameters["param1"],
            (T2)parameters["param2"],
            (T3)parameters["param3"]
        ));
    }

    public static void StartListening<T, T1, T2, T3, T4>(T eventName, UnityAction<T1, T2, T3, T4> listener)
        where T : Enum
    {
        var thisEvent = GetEvent(eventName);
        thisEvent.AddListener((parameters) => listener.Invoke(
            (T1)parameters["param1"],
            (T2)parameters["param2"],
            (T3)parameters["param3"],
            (T4)parameters["param4"]
        ));
    }

    // ==============================
    // Stop Listening Methods
    // ==============================

    public static void StopListening<T>(T eventName, UnityAction listener) where T : Enum
    {
        void MainListener(Dictionary<string, object> _) => listener.Invoke();
        StopListeningInternal(eventName.ToString(), MainListener);
    }

    public static void StopListening<T, T1>(T eventName, UnityAction<T1> listener) where T : Enum
    {
        void MainListener(Dictionary<string, object> parameters) => listener.Invoke(
            (T1)parameters["param1"]
        );

        StopListeningInternal(eventName.ToString(), MainListener);
    }

    public static void StopListening<T, T1, T2>(T eventName, UnityAction<T1, T2> listener) where T : Enum
    {
        void MainListener(Dictionary<string, object> parameters) => listener.Invoke(
            (T1)parameters["param1"],
            (T2)parameters["param2"]
        );

        StopListeningInternal(eventName.ToString(), MainListener);
    }

    public static void StopListening<T, T1, T2, T3>(T eventName, UnityAction<T1, T2, T3> listener) where T : Enum
    {
        void MainListener(Dictionary<string, object> parameters) => listener.Invoke(
            (T1)parameters["param1"],
            (T2)parameters["param2"],
            (T3)parameters["param3"]
        );

        StopListeningInternal(eventName.ToString(), MainListener);
    }

    public static void StopListening<T, T1, T2, T3, T4>(T eventName, UnityAction<T1, T2, T3, T4> listener)
        where T : Enum
    {
        void MainListener(Dictionary<string, object> parameters) => listener.Invoke(
            (T1)parameters["param1"],
            (T2)parameters["param2"],
            (T3)parameters["param3"],
            (T4)parameters["param4"]
        );

        StopListeningInternal(eventName.ToString(), MainListener);
    }

    // =======================
    // Private Helper Methods
    // =======================

    private static UnityEvent<Dictionary<string, object>> GetEvent<T>(T eventName) where T : Enum
    {
        if (!EventDictionary.TryGetValue(eventName.ToString(), out var thisEvent))
        {
            thisEvent = new UnityEvent<Dictionary<string, object>>();
            EventDictionary[eventName.ToString()] = thisEvent;
            EventNumberOfListenersDictionary[eventName.ToString()] = 1;
        }
        else
        {
            EventNumberOfListenersDictionary[eventName.ToString()] += 1;
        }

        return thisEvent;
    }

    private static void StopListeningInternal(string eventName, UnityAction<Dictionary<string, object>> listener)
    {
        if (EventDictionary.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent.RemoveListener(listener);
            EventNumberOfListenersDictionary[eventName] -= 1;

            if (EventNumberOfListenersDictionary[eventName] == 0)
            {
                EventDictionary.Remove(eventName);
            }
        }
    }

    // ================================
    // Event Trigger Methods
    // ================================

    public static void TriggerEvent<T>(T eventName) where T : Enum
    {
        TriggerEventInternal(eventName.ToString(), null);
    }

    public static void TriggerEvent<T, T1>(T eventName, T1 param1) where T : Enum
    {
        var parameters = new Dictionary<string, object>
        {
            { "param1", param1 }
        };
        TriggerEventInternal(eventName.ToString(), parameters);
    }

    public static void TriggerEvent<T, T1, T2>(T eventName, T1 param1, T2 param2) where T : Enum
    {
        var parameters = new Dictionary<string, object>
        {
            { "param1", param1 },
            { "param2", param2 }
        };
        TriggerEventInternal(eventName.ToString(), parameters);
    }

    public static void TriggerEvent<T, T1, T2, T3>(T eventName, T1 param1, T2 param2, T3 param3) where T : Enum
    {
        var parameters = new Dictionary<string, object>
        {
            { "param1", param1 },
            { "param2", param2 },
            { "param3", param3 }
        };
        TriggerEventInternal(eventName.ToString(), parameters);
    }

    public static void TriggerEvent<T, T1, T2, T3, T4>(T eventName, T1 param1, T2 param2, T3 param3, T4 param4)
        where T : Enum
    {
        var parameters = new Dictionary<string, object>
        {
            { "param1", param1 },
            { "param2", param2 },
            { "param3", param3 },
            { "param4", param4 }
        };
        TriggerEventInternal(eventName.ToString(), parameters);
    }

    // ========================
    // Private Trigger Logic
    // ========================

    private static void TriggerEventInternal(string eventName, Dictionary<string, object> parameters)
    {
        // Check if the current thread is the main thread
        if (Thread.CurrentThread.ManagedThreadId != 1)
        {
            // If not on the main thread, use MainThreadDispatcher to enqueue the event to run on the main thread
            if (UnityMainThreadDispatcher.Instance != null)
            {
                UnityMainThreadDispatcher.Enqueue(() => TriggerEventInternal(eventName, parameters));
            }
            else
            {
                TriggerEventInternal(eventName, parameters);
            }

            return;
        }

        // If on the main thread, trigger the event as usual
        if (EventDictionary.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent.Invoke(parameters);
        }
    }

    // ============================
    // Remove Event and Listeners
    // ============================

    public static void RemoveEvent<T>(T eventName) where T : Enum
    {
        if (EventDictionary.TryGetValue(eventName.ToString(), out var thisEvent))
        {
            thisEvent.RemoveAllListeners();
            EventDictionary.Remove(eventName.ToString());
        }
    }
}