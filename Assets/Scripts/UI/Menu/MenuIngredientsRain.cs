using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Pool;

public sealed class MenuIngredientsRain : ValidatedMonoBehaviour
{
    [SerializeField]
    private MenuRainIngredient _ingredientPrefab;

    [SerializeField, Self]
    private RectTransform _spawnRoot;

    [SerializeField]
    private Sprite[] _ingredientSprites;

    [SerializeField]
    private float _spawnHeight = 540f;

    [SerializeField]
    private Vector2 _spawnIntervalRange = new(0.2f, 0.8f);

    [SerializeField]
    private Vector2 _speedRange = new(120f, 260f);

    [SerializeField]
    private Vector2 _directionAngleRange = new(220f, 320f);

    [SerializeField]
    private Vector2 _angularSpeedRange = new(-180f, 180f);

    [SerializeField]
    private float _ingredientLifetime = 6f;

    [SerializeField, Min(0)]
    private int _defaultCapacity = 16;

    [SerializeField, Min(1)]
    private int _maxSize = 64;

    private readonly List<MenuRainIngredient> _activeIngredients = new();
    private ObjectPool<MenuRainIngredient> _pool;
    private CancellationTokenSource _spawnLoopCancellation;

    private void Awake()
    {
        _pool = new ObjectPool<MenuRainIngredient>(
            CreateIngredient,
            ingredient => ingredient.gameObject.SetActive(true),
            ingredient =>
            {
                ingredient.StopForPool();
                ingredient.gameObject.SetActive(false);
            },
            ingredient => Destroy(ingredient.gameObject),
            true,
            _defaultCapacity,
            _maxSize);
    }

    private void OnEnable()
    {
        _spawnLoopCancellation = new CancellationTokenSource();
        SpawnLoopAsync(_spawnLoopCancellation.Token).Forget();
    }

    private void OnDisable()
    {
        StopSpawnLoop();
        ReleaseActiveIngredients();
    }

    private void OnDestroy()
    {
        StopSpawnLoop();
        _pool?.Clear();
    }

    private async UniTaskVoid SpawnLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var delay = UnityEngine.Random.Range(_spawnIntervalRange.x, _spawnIntervalRange.y);
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken);

                if (cancellationToken.IsCancellationRequested || !isActiveAndEnabled)
                    return;

                SpawnIngredient();
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the menu or component is disabled.
        }
    }

    private void SpawnIngredient()
    {
        var sprite = GetRandomSprite();
        if (sprite == null)
            return;

        var rainIngredient = _pool.Get();
        _activeIngredients.Add(rainIngredient);

        rainIngredient.Initialize(
            sprite,
            GetRandomSpawnPosition(),
            GetRandomVelocity(),
            UnityEngine.Random.Range(_angularSpeedRange.x, _angularSpeedRange.y),
            _ingredientLifetime,
            ReleaseIngredient);
    }

    private MenuRainIngredient CreateIngredient()
    {
        return Instantiate(_ingredientPrefab, _spawnRoot);
    }

    private Sprite GetRandomSprite()
    {
        return _ingredientSprites[UnityEngine.Random.Range(0, _ingredientSprites.Length)];
    }

    private Vector2 GetRandomSpawnPosition()
    {
        var originPosition = _spawnRoot.anchoredPosition;
        var halfWidth = _spawnRoot.rect.width * 0.5f;

        return new Vector2(
            UnityEngine.Random.Range(originPosition.x - halfWidth, originPosition.x + halfWidth),
            originPosition.y + _spawnHeight);
    }

    private Vector2 GetRandomVelocity()
    {
        var angle = UnityEngine.Random.Range(_directionAngleRange.x, _directionAngleRange.y) * Mathf.Deg2Rad;
        var speed = UnityEngine.Random.Range(_speedRange.x, _speedRange.y);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
    }

    private void ReleaseIngredient(MenuRainIngredient ingredient)
    {
        if (ingredient == null || !_activeIngredients.Remove(ingredient))
            return;

        _pool.Release(ingredient);
    }

    private void ReleaseActiveIngredients()
    {
        for (var i = _activeIngredients.Count - 1; i >= 0; i--)
            _pool.Release(_activeIngredients[i]);

        _activeIngredients.Clear();
    }

    private void StopSpawnLoop()
    {
        if (_spawnLoopCancellation == null)
            return;

        _spawnLoopCancellation.Cancel();
        _spawnLoopCancellation.Dispose();
        _spawnLoopCancellation = null;
    }
}
