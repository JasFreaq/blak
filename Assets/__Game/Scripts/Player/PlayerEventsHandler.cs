using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerEventsHandler
{
    #region Time States

    static Action _onTimeStop;
    static Action _onTimeResume;

    static public void RegisterOnTimeStop(Action action)
    {
        _onTimeStop += action;
    }

    static public void DeregisterOnTimeStop(Action action)
    {
        _onTimeStop -= action;
    }

    static public void RegisterOnTimeResume(Action action)
    {
        _onTimeResume += action;
    }

    static public void DeregisterOnTimeResume(Action action)
    {
        _onTimeResume -= action;
    }

    static public void InvokeOnTimeStop()
    {
        _onTimeStop?.Invoke();
    }

    static public void InvokeOnTimeResume()
    {
        _onTimeResume?.Invoke();
    }

    #endregion

    #region Shape Formation

    public static Action _onShapeForm;
    public static Action _onShapeAbsorb;

    static public void RegisterOnShapeForm(Action action)
    {
        _onShapeForm += action;
    }

    static public void DeregisterOnShapeForm(Action action)
    {
        _onShapeForm -= action;
    }

    static public void RegisterOnShapeAbsorb(Action action)
    {
        _onShapeAbsorb += action;
    }

    static public void DeregisterOnShapeAbsorb(Action action)
    {
        _onShapeAbsorb -= action;
    }

    static public void InvokeOnShapeForm()
    {
        _onShapeForm?.Invoke();
    }

    static public void InvokeOnShapeAbsorb()
    {
        _onShapeAbsorb?.Invoke();
    }

    #endregion
}
