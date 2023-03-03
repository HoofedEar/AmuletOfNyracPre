﻿using DarkWoodsRL.MapObjects.Components.Items;
using DarkWoodsRL.Maps;
using SadRogue.Integration;
using SadRogue.Primitives;

namespace DarkWoodsRL.MapObjects.Items;

/// <summary>
/// Simple class with some static functions for creating items.
/// </summary>
internal static class Factory
{
    public static RogueLikeEntity HealthPotion()
    {
        var potion = new RogueLikeEntity(new Color(127, 0, 255), Color.Black, '!', layer: (int)GameMap.Layer.Items)
        {
            Name = "Health Potion"
        };
        potion.AllComponents.Add(new HealingConsumable(4));

        return potion;
    }
}