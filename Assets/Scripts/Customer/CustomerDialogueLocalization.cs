using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public static class CustomerDialogueLocalization
{
    private const string PortugueseLanguageCode = "pt";

    public static string GetRandomLocalizedDialogue(LocalizedStringTable tableReference, string ownerName, string fieldName) =>
        GetRandomLocalizedEntry(tableReference, ownerName, fieldName).GetLocalizedString();

    public static StringTableEntry GetRandomLocalizedEntry(LocalizedStringTable tableReference, string ownerName, string fieldName)
    {
        var table = GetRequiredTable(tableReference, ownerName, fieldName);

        if (table.SharedData?.Entries == null || table.SharedData.Entries.Count == 0)
            throw new InvalidOperationException($"Localized table '{fieldName}' has no entries.");

        var randomIndex = UnityEngine.Random.Range(0, table.SharedData.Entries.Count);
        var sharedEntry = table.SharedData.Entries[randomIndex];
        var entry = table.GetEntry(sharedEntry.Id);

        if (entry == null)
            throw new InvalidOperationException($"Localized table '{fieldName}' is missing entry id '{sharedEntry.Id}' for the selected locale.");

        return entry;
    }

    public static string GetLocalizedIngredientName(LocalizedStringTable tableReference, Ingredient ingredient, string ownerName, string fieldName, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(ingredient.LocalizationKey))
            throw new InvalidOperationException($"Ingredient '{ingredient.name}' is missing {nameof(Ingredient.LocalizationKey)}.");

        var entry = GetLocalizedEntry(tableReference, ingredient.LocalizationKey, ownerName, fieldName);
        return entry.GetLocalizedString(new { amount });
    }

    public static string JoinLocalizedList(IReadOnlyList<string> items)
    {
        if (items.Count == 0)
            return string.Empty;

        if (items.Count == 1)
            return items[0];

        var separator = ", ";
        var finalSeparator = IsPortugueseSelected() ? " e " : " and ";

        if (items.Count == 2)
            return $"{items[0]}{finalSeparator}{items[1]}";

        var combinedText = items[0];

        for (var i = 1; i < items.Count - 1; i++)
            combinedText += separator + items[i];

        return combinedText + finalSeparator + items[^1];
    }

    private static StringTableEntry GetLocalizedEntry(LocalizedStringTable tableReference, string key, string ownerName, string fieldName)
    {
        var table = GetRequiredTable(tableReference, ownerName, fieldName);
        var entry = table.GetEntry(key);

        if (entry == null)
            throw new InvalidOperationException($"Localized table '{fieldName}' does not contain key '{key}' for the selected locale.");

        return entry;
    }

    private static StringTable GetRequiredTable(LocalizedStringTable tableReference, string ownerName, string fieldName)
    {
        if (tableReference == null)
            throw new InvalidOperationException($"{ownerName} field '{fieldName}' is not assigned in the inspector.");

        var table = tableReference.GetTable();

        if (table == null)
            throw new InvalidOperationException($"{ownerName} field '{fieldName}' could not resolve a String Table for locale '{LocalizationSettings.SelectedLocale?.Identifier.Code}'.");

        return table;
    }

    private static bool IsPortugueseSelected()
    {
        var localeCode = LocalizationSettings.SelectedLocale?.Identifier.Code;
        return !string.IsNullOrEmpty(localeCode) && localeCode.StartsWith(PortugueseLanguageCode, StringComparison.OrdinalIgnoreCase);
    }
}
