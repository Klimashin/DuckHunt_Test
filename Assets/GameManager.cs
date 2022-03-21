using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class GameManager
{
    private static GameManager _instanceRef;
    public static GameManager Instance 
    { 
        get => _instanceRef;
        private set
        {
            if (_instanceRef != null)
            {
                throw new Exception("GameManager singleton violation");
            }

            _instanceRef = value;
        }
    }

    public InputActions InputActions { get; }
    public GameSettings GameSettings { get; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadMain()
    {
        Instance = new GameManager();
    }

    private const string GameSettingsAddressablesKey = "GameSettings.asset";
    private GameManager()
    {
        InputActions = new InputActions();

        var gameSettingsHandle = Addressables.LoadAssetAsync<GameSettings>(GameSettingsAddressablesKey);
        GameSettings = gameSettingsHandle.WaitForCompletion();
    }
}