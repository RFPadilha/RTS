using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    //used as a message passing interface that removes the need to entangle scripts
    private Dictionary<string, UnityEvent> _events;
    private static EventManager _eventManager;
    private Dictionary<string, CustomEvent> _typedEvents;

    //only one instance of the event manager must be active at any given time
    public static EventManager instance
    {
        get
        {
            if (!_eventManager)
            {
                _eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;

                if (!_eventManager)
                    Debug.LogError("There needs to be one active EventManager script on a GameObject in your scene.");
                else
                    _eventManager.Init();
            }

            return _eventManager;
        }
    }

    //initializes event dictionaries, associating a name (string) to each event
    void Init()
    {
        if (_events == null)
        {
            _events = new Dictionary<string, UnityEvent>();
            _typedEvents = new Dictionary<string, CustomEvent>();
        }
    }

    public static void AddListener(string eventName, UnityAction listener)
    {
        UnityEvent evt = null;
        if (instance._events.TryGetValue(eventName, out evt))//if event already exists in the dictionary
        {
            evt.AddListener(listener);//adds listener to event
        }
        else//if event is not on dictionary
        {
            evt = new UnityEvent();//creates new event
            evt.AddListener(listener);//adds its listener
            instance._events.Add(eventName, evt);//adds string-key pair to dictionary
        }
    }

    public static void RemoveListener(string eventName, UnityAction listener)
    {
        if (_eventManager == null) return;
        UnityEvent evt = null;
        if (instance._events.TryGetValue(eventName, out evt))
            evt.RemoveListener(listener);
    }

    public static void TriggerEvent(string eventName)
    {
        UnityEvent evt = null;
        if (instance._events.TryGetValue(eventName, out evt))
            evt.Invoke();
    }

    //adds custom event to dictionary, same as AddListener, but with custom event data type
    public static void AddTypedListener(string eventName, UnityAction<CustomEventData> listener)
    {
        CustomEvent evt = null;
        if (instance._typedEvents.TryGetValue(eventName, out evt))
        {
            evt.AddListener(listener);
        }
        else
        {
            evt = new CustomEvent();
            evt.AddListener(listener);
            instance._typedEvents.Add(eventName, evt);
        }
    }

    public static void RemoveTypedListener(string eventName, UnityAction<CustomEventData> listener)
    {
        if (_eventManager == null) return;
        CustomEvent evt = null;
        if (instance._typedEvents.TryGetValue(eventName, out evt))
            evt.RemoveListener(listener);
    }

    public static void TriggerTypedEvent(string eventName, CustomEventData data)
    {
        CustomEvent evt = null;
        if (instance._typedEvents.TryGetValue(eventName, out evt))
            evt.Invoke(data);
    }
}
public class CustomEventData
{
    public UnitData unitData;
    public Unit unit;

    public CustomEventData(UnitData unitData)
    {
        this.unitData = unitData;
        this.unit = null;
    }
    public CustomEventData(Unit unit)
    {
        this.unitData = null;
        this.unit = unit;
    }
}

[System.Serializable]
public class CustomEvent : UnityEvent<CustomEventData> { }
