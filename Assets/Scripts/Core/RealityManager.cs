using System;
using UnityEngine;

/// <summary>
/// Singleton. Gestiona la realidad activa (Fisica / Ciberespacial) y notifica cambios.
/// </summary>
public enum RealityType { Physical, Cyberspace }

public class RealityManager : MonoBehaviour
{
    // Evento global para swap de realidades
    public static event Action<RealityType> OnRealityChanged;

    private static RealityManager _instance;
    public static RealityManager Instance => _instance;

    private RealityType _currentReality = RealityType.Physical;
    public RealityType CurrentReality => _currentReality;

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        // Tecla swap: Tab (puede cambiarse)
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwapReality();
        }
    }

    public void SwapReality()
    {
        _currentReality = (_currentReality == RealityType.Physical)
            ? RealityType.Cyberspace
            : RealityType.Physical;

        OnRealityChanged?.Invoke(_currentReality);
    }

    public void SetReality(RealityType targetReality)
    {
        if (_currentReality != targetReality)
        {
            _currentReality = targetReality;
            OnRealityChanged?.Invoke(_currentReality);
        }
    }
}