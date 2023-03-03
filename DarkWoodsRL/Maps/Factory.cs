﻿using GoRogue.MapGeneration;
using GoRogue.MapGeneration.ContextComponents;
using GoRogue.Random;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using ShaiRandom.Generators;

namespace DarkWoodsRL.Maps;

/// <summary>
/// Basic factory which produces different types of maps.
/// </summary>
/// <remarks>
/// The map gen below won't use GoRogue's map generation system for the custom parts, although we could; it simply isn't
/// necessary for the relatively simple, single type of map we have below.
/// </remarks>
internal static class Factory
{
    private const int MaxMonstersPerRoom = 2;
    private const int MaxPotionsPerRoom = 2;

    public static (GameMap map, Point playerSpawn) Dungeon()
    {
        // Generate a dungeon maze map
        var generator = new Generator(60, 30)
            .ConfigAndGenerateSafe(gen =>
            {
                gen.AddSteps(DefaultAlgorithms.BasicRandomRoomsMapSteps(minRooms: 20, maxRooms: 30, roomMinSize: 8, roomMaxSize: 10));
            });

        // Extract components from the map GoRogue generated which hold basic information about the map
        var generatedMap = generator.Context.GetFirst<ISettableGridView<bool>>("WallFloor");
        var rooms = generator.Context.GetFirst<ItemList<Rectangle>>("Rooms");

        // Create actual integration library map with a proper component for the character "memory" system.
        var map = new GameMap(generator.Context.Width, generator.Context.Height, null);
        map.AllComponents.Add(new TerrainFOVVisibilityHandler());

        // Translate GoRogue's terrain data into actual integration library objects.
        map.ApplyTerrainOverlay(generatedMap, (pos, val) => val ? MapObjects.Factory.Floor(pos) : MapObjects.Factory.Wall(pos));

        // Set player spawn
        var playerSpawn = GetPlayerSpawn(rooms);

        // Spawn enemies/items/etc
        SpawnMonsters(map, rooms, playerSpawn);
        SpawnPotions(map, rooms, playerSpawn);

        return (map, playerSpawn);
    }

    private static Point GetPlayerSpawn(ItemList<Rectangle> rooms)
    {
        // Add player to map at the center of the first room we placed
        return rooms.Items[0].Center;
    }

    private static void SpawnMonsters(GameMap map, ItemList<Rectangle> rooms, Point playerSpawn)
    {
        // Generate between zero and the max monsters per room.  Each monster has an 80% chance of being an orc (weaker)
        // and a 20% chance of being a troll (stronger).
        foreach (var room in rooms.Items)
        {
            int enemies = GlobalRandom.DefaultRNG.NextInt(0, MaxMonstersPerRoom + 1);
            for (int i = 0; i < enemies; i++)
            {
                bool isOrc = GlobalRandom.DefaultRNG.PercentageCheck(80f);

                var enemy = isOrc ? MapObjects.Factory.Orc() : MapObjects.Factory.Troll();
                enemy.Position = GlobalRandom.DefaultRNG.RandomPosition(room, pos => map.WalkabilityView[pos] && pos != playerSpawn);
                map.AddEntity(enemy);
            }
        }
    }

    private static void SpawnPotions(GameMap map, ItemList<Rectangle> rooms, Point playerSpawn)
    {
        // Generate between zero and the max potions per room.
        foreach (var room in rooms.Items)
        {
            int potions = GlobalRandom.DefaultRNG.NextInt(0, MaxPotionsPerRoom + 1);
            for (int i = 0; i < potions; i++)
            {
                var potion = MapObjects.Items.Factory.HealthPotion();
                potion.Position = GlobalRandom.DefaultRNG.RandomPosition(room, pos => map.WalkabilityView[pos] && pos != playerSpawn);
                map.AddEntity(potion);
            }
        }
    }
}