using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

public class StoreView : MonoBehaviour
{
    [SerializeField]
    private Transform _randomIngredientsRoot;

    [SerializeField]
    [FormerlySerializedAs("_randomIngredientPrefab")]
    private IngredientStoreView _ingredientPrefab;

    [SerializeField]
    private Transform _fixedIngredientsRoot;

    [SerializeField]
    private Transform[] _leftIngredientPositions;

    [SerializeField]
    private Transform[] _rightIngredientPositions;

    [SerializeField]
    private IngredientStoreView _leftIngredientPrefab;

    [SerializeField]
    private IngredientStoreView _rightIngredientPrefab;

    [SerializeField]
    private IngredientStorePurchasePrompt _purchasePrompt;

    [Inject]
    private readonly Store _store;

    private readonly struct IngredientSlot
    {
        public IngredientSlot(Transform position, IngredientStoreView prefab)
        {
            Position = position;
            Prefab = prefab;
        }

        public Transform Position { get; }
        public IngredientStoreView Prefab { get; }
    }

    private void Awake()
    {
        var ingredientSlots = BuildShuffledIngredientSlots();
        var nextSlotIndex = 0;

        BuildRandomIngredients(ingredientSlots, ref nextSlotIndex);
        BuildFixedIngredients(ingredientSlots, ref nextSlotIndex);
    }

    private void BuildRandomIngredients(IngredientSlot[] ingredientSlots, ref int nextSlotIndex)
    {
        var randomIngredients = _store.RandomIngredients;

        foreach (var ingredient in randomIngredients)
        {
            if (!TryCreateIngredientView(ingredientSlots, ref nextSlotIndex, out var itemView))
                return;

            itemView.BindDaily(ingredient, _purchasePrompt);
        }
    }

    private void BuildFixedIngredients(IngredientSlot[] ingredientSlots, ref int nextSlotIndex)
    {
        var fixedIngredients = _store.FixedIngredients;

        foreach (var fixedIngredientOffer in fixedIngredients)
        {
            if (!TryCreateIngredientView(ingredientSlots, ref nextSlotIndex, out var itemView))
                return;

            itemView.BindFixed(fixedIngredientOffer, _purchasePrompt);
        }
    }

    private IngredientSlot[] BuildShuffledIngredientSlots()
    {
        var leftIngredientPrefab = GetPrefabForSide(_leftIngredientPositions, _leftIngredientPrefab, nameof(_leftIngredientPrefab));
        var rightIngredientPrefab = GetPrefabForSide(_rightIngredientPositions, _rightIngredientPrefab, nameof(_rightIngredientPrefab));
        var leftSlotCount = GetValidSlotCount(_leftIngredientPositions, leftIngredientPrefab);
        var rightSlotCount = GetValidSlotCount(_rightIngredientPositions, rightIngredientPrefab);
        var ingredientSlots = new IngredientSlot[leftSlotCount + rightSlotCount];
        var nextSlotIndex = 0;

        AddSlots(_leftIngredientPositions, leftIngredientPrefab, ingredientSlots, ref nextSlotIndex);
        AddSlots(_rightIngredientPositions, rightIngredientPrefab, ingredientSlots, ref nextSlotIndex);

        for (var i = ingredientSlots.Length - 1; i > 0; i--)
        {
            var swapIndex = Random.Range(0, i + 1);
            (ingredientSlots[i], ingredientSlots[swapIndex]) = (ingredientSlots[swapIndex], ingredientSlots[i]);
        }

        return ingredientSlots;
    }

    private bool TryCreateIngredientView(IngredientSlot[] ingredientSlots, ref int nextSlotIndex, out IngredientStoreView itemView)
    {
        itemView = null;

        if (nextSlotIndex >= ingredientSlots.Length)
        {
            Debug.LogWarning($"{nameof(StoreView)} does not have enough ingredient positions configured.", this);
            return false;
        }

        var slot = ingredientSlots[nextSlotIndex];
        nextSlotIndex++;
        itemView = Instantiate(slot.Prefab, slot.Position);
        return true;
    }

    private static int GetValidSlotCount(Transform[] positions, IngredientStoreView prefab)
    {
        if (positions == null || prefab == null)
            return 0;

        var count = 0;
        for (var i = 0; i < positions.Length; i++)
        {
            if (positions[i] != null)
                count++;
        }

        return count;
    }

    private static void AddSlots(
        Transform[] positions,
        IngredientStoreView prefab,
        IngredientSlot[] ingredientSlots,
        ref int nextSlotIndex)
    {
        if (positions == null || prefab == null)
            return;

        for (var i = 0; i < positions.Length; i++)
        {
            var position = positions[i];
            if (position == null)
                continue;

            ingredientSlots[nextSlotIndex] = new IngredientSlot(position, prefab);
            nextSlotIndex++;
        }
    }

    private IngredientStoreView GetPrefabForSide(Transform[] positions, IngredientStoreView sidePrefab, string fieldName)
    {
        if (sidePrefab != null)
            return sidePrefab;

        if (_ingredientPrefab != null)
        {
            Debug.LogWarning(
                $"{nameof(StoreView)} is using the legacy ingredient prefab because {fieldName} is not assigned.",
                this);
            return _ingredientPrefab;
        }

        if (HasValidPosition(positions))
        {
            Debug.LogWarning(
                $"{nameof(StoreView)} has ingredient positions configured but {fieldName} is not assigned.",
                this);
        }

        return null;
    }

    private static bool HasValidPosition(Transform[] positions)
    {
        if (positions == null)
            return false;

        for (var i = 0; i < positions.Length; i++)
        {
            if (positions[i] != null)
                return true;
        }

        return false;
    }
}
