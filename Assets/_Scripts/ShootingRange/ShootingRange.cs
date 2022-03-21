using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class ShootingRange : MonoBehaviour
{
    public int CurrentBullets { get; private set; }
    public int CurrentScore { get; private set; }
    public int CurrentLives { get; private set; }
    public int CurrentStage { get; private set; }

    public event ShootingRangeNotification OnGameWon;
    public event ShootingRangeNotification OnGameLost;

    private InputActions.ShootingRangeActions _inputActions;
    private ShootingRangeWeapon _currentWeapon;
    private GameObject _rangeTargetPrefab;
    private GameObject _weaponPrefab;
    private List<StageSettings> _stages;
    private Camera _cam;
    private EventSystem _eventSystem;

    private void Start()
    {
        StartCoroutine(Init());
    }

    private bool _isPointerOverUi;
    private void Update()
    {
        _isPointerOverUi = _eventSystem.IsPointerOverGameObject();
    }

    private IEnumerator Init()
    {
        _eventSystem = EventSystem.current;
        _cam = Camera.main;

        var gm = GameManager.Instance;
        CurrentLives = gm.GameSettings.InitialLives;
        _stages = gm.GameSettings.Stages;
        _inputActions = gm.InputActions.ShootingRange;
        _inputActions.Shoot.performed += OnShoot;
        CurrentStage = 0;
        CurrentBullets = 0;
        CurrentScore = 0;
        Time.timeScale = 1;

        var loadAssetsTask = LoadAssets();
        while (!loadAssetsTask.IsCompleted)
        {
            yield return null;
        }

        InitSpawnZones();

        _currentWeapon = Instantiate(_weaponPrefab, transform).GetComponent<ShootingRangeWeapon>();
        gm.InputActions.Enable();

        StartCoroutine(StageCoroutine(_stages[CurrentStage]));
    }

    private TargetSpawnLine _spawnLeft;
    private TargetSpawnLine _spawnRight;
    private void InitSpawnZones()
    {
        _spawnLeft = new TargetSpawnLine
        {
            startPoint = _cam.ViewportToWorldPoint(new Vector3(0, 0.2f)),
            endPoint = _cam.ViewportToWorldPoint(new Vector3(0, 0.8f)),
            targetsMoveDirection = Vector3.right
        };
        
        _spawnRight = new TargetSpawnLine
        {
            startPoint = _cam.ViewportToWorldPoint(new Vector3(1, 0.2f)),
            endPoint = _cam.ViewportToWorldPoint(new Vector3(1, 0.8f)),
            targetsMoveDirection = Vector3.left
        };
    }

    private async Task LoadAssets()
    {
        var gm = GameManager.Instance;
        
        var weaponAssetReferenceKey = gm.GameSettings.WeaponsDict[WeaponType.RayWeapon];
        var targetAssetReferenceKey = gm.GameSettings.TargetsDict[TargetType.BasicTarget];
        var weaponHandle = Addressables.LoadAssetAsync<GameObject>(weaponAssetReferenceKey);
        var targetHandle = Addressables.LoadAssetAsync<GameObject>(targetAssetReferenceKey);

        await Task.WhenAll(new List<Task> { weaponHandle.Task, targetHandle.Task });

        _weaponPrefab = weaponHandle.Result;
        _rangeTargetPrefab = targetHandle.Result;
    }

    private void OnShoot(InputAction.CallbackContext context)
    {
        if (!_currentWeapon.CanShot || CurrentBullets <= 0 || _isPointerOverUi)
        {
            return;
        }

        var screenPos = _inputActions.ShootPos.ReadValue<Vector2>();
        var shotRay = _cam.ScreenPointToRay(screenPos);

        CurrentBullets--;
        
        _currentWeapon.Shot(shotRay);
    }

    private IEnumerator StageCoroutine(StageSettings stageSettings)
    {
        var timeToNextSpawn = stageSettings.InitialDelay;

        CurrentBullets += stageSettings.BulletsCount;

        var spawnQueue = new Queue<SpawnQueueEntry>(stageSettings.SpawnQueue);
        while (!LoseCheck() && spawnQueue.Count > 0)
        {
            timeToNextSpawn -= Time.deltaTime;
            if (timeToNextSpawn <= 0f)
            {
                var spawnEntry = spawnQueue.Dequeue();
                timeToNextSpawn = spawnEntry.TimeToNextSpawn;
                SpawnNewShootingRangeTarget(stageSettings.TargetsSpeedCoefficient);
            }
            
            yield return null;
        }

        while (_activeRangeTargets.Count > 0 && !LoseCheck())
        {
            yield return null;
        }
        
        ActiveTargetsCleanup();

        if (LoseCheck())
        {
            OnGameLost?.Invoke(this);
            yield break;
        }

        CurrentStage++;
        if (CurrentStage > _stages.Count - 1)
        {
            OnGameWon?.Invoke(this);
            yield break;
        }

        StartCoroutine(StageCoroutine(_stages[CurrentStage]));
    }

    private void ActiveTargetsCleanup()
    {
        for (var i = _activeRangeTargets.Count - 1; i >= 0; i--)
        {
            _activeRangeTargets[i].CleanUp();
        }
    }

    private readonly List<ShootingRangeTarget> _activeRangeTargets = new List<ShootingRangeTarget>();
    private void SpawnNewShootingRangeTarget(float targetSpeed)
    {
        var spawn = GetSpawn();
        var spawnPoint = spawn.GetSpawnPoint();
        spawnPoint.z = 0;

        // @TODO: use objects pooling
        var newTarget = Instantiate(_rangeTargetPrefab);
        newTarget.transform.position = spawnPoint;
        newTarget.GetComponent<TargetMover>().Init(spawn.GetMoveDirection(), targetSpeed);
        
        var shootingRangeTarget = newTarget.GetComponent<ShootingRangeTarget>();
        
        shootingRangeTarget.OnHit += OnShootingRangeTargetHit;
        shootingRangeTarget.OnLost += OnShootingRangeTargetLost;
        shootingRangeTarget.OnCleanup += OnShootingRangeTargetCleanup;

        _activeRangeTargets.Add(shootingRangeTarget);
    }

    private bool LoseCheck()
    {
        return CurrentLives <= 0;
    }

    private void OnShootingRangeTargetHit(ShootingRangeTarget target)
    {
        CurrentScore++;
    }
    
    private void OnShootingRangeTargetLost(ShootingRangeTarget target)
    {
        CurrentLives--;
    }

    private void OnShootingRangeTargetCleanup(ShootingRangeTarget target)
    {
        target.OnHit -= OnShootingRangeTargetHit;
        target.OnLost -= OnShootingRangeTargetLost;
        target.OnCleanup -= OnShootingRangeTargetCleanup;
        _activeRangeTargets.Remove(target);
    }

    private ITargetSpawn GetSpawn()
    {
        return Random.Range(0, 2) == 0 ? _spawnLeft : _spawnRight;
    }
}

public delegate void ShootingRangeNotification(ShootingRange shootingRange);

[Serializable]
public class StageSettings
{
    public int BulletsCount;
    public float TargetsSpeedCoefficient = 1f;
    public float InitialDelay;
    public List<SpawnQueueEntry> SpawnQueue;
}

[Serializable]
public class SpawnQueueEntry
{
    public TargetType TargetType;
    public float TimeToNextSpawn;
}

public class TargetSpawnLine : ITargetSpawn
{
    public Vector3 startPoint;
    public Vector3 endPoint;
    public Vector3 targetsMoveDirection;

    public Vector3 GetMoveDirection() => targetsMoveDirection;

    public Vector3 GetSpawnPoint()
    {
        var delta = endPoint - startPoint;
        var offset = delta *  Random.Range(0f, 1f);
        return startPoint + offset;
    }
}

public interface ITargetSpawn
{
    Vector3 GetSpawnPoint();

    Vector3 GetMoveDirection();
}
