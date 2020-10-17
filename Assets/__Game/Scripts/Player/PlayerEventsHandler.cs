using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerEventsHandler
{
    public static Action onTimeStop;
    public static Action onTimeResume;
    
    static public void InvokeOnToggleTimeStop(bool stopped)
    {
        if (stopped)
        {
            onTimeStop?.Invoke();
        }
        else
        {
            onTimeResume?.Invoke();
        }
    }
}
