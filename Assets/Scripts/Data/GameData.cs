using System;
using System.Collections.Generic;
using UnityEngine;

namespace CottageMagic
{
    public enum ItemKind { Seed, Ingredient, Soil, Water, Food, Paper, Ink, Potion, Scroll, Weapon, Material, Other }
    public enum StationKind { Potion, Scroll, Enchant, Planting, DoorIn, DoorOut, OrderBoard, Trader, Rest, None }

    [Serializable]
    public sealed class ItemRecord
    {
        public string id;
        public string name;
        public string kind;
        public string category;
        public int amount;
        public int power;
        public int effect;

        public ItemRecord Copy(int count)
        {
            return new ItemRecord { id = id, name = name, kind = kind, category = category, amount = count, power = power, effect = effect };
        }
    }

    [Serializable]
    public sealed class PlantRecord
    {
        public int plotIndex;
        public bool unlocked;
        public string species;
        public string soil;
        public string charm;
        public int wateringCount;
        public int lastWaterDay = -1;
        public int bonusPower;
    }

    [Serializable]
    public sealed class OrderRecord
    {
        public string id;
        public string client;
        public string request;
        public string kind;
        public int minimumPower;
        public int reward;
        public int reputationReward;
        public double issuedAt;
        public double dueAt;
        public bool penalized;
    }

    [Serializable]
    public sealed class SaveData
    {
        public int version = 1;
        public int gold = 75;
        public int reputation;
        public double worldMinutes = 480;
        public int lastOrderDay = 1;
        public bool indoors;
        public float playerX;
        public float playerY;
        public List<ItemRecord> inventory = new List<ItemRecord>();
        public List<PlantRecord> plots = new List<PlantRecord>();
        public List<OrderRecord> orders = new List<OrderRecord>();
    }

    public sealed class ItemTemplate
    {
        public readonly string Id;
        public readonly string Name;
        public readonly ItemKind Kind;
        public readonly string Category;
        public readonly int Power;
        public readonly int Effect;
        public readonly int Price;

        public ItemTemplate(string id, string name, ItemKind kind, string category, int power, int effect, int price)
        {
            Id = id; Name = name; Kind = kind; Category = category; Power = power; Effect = effect; Price = price;
        }

        public ItemRecord Stack(int amount)
        {
            return new ItemRecord { id = Id, name = Name, kind = Kind.ToString(), category = Category, power = Power, effect = Effect, amount = amount };
        }
    }

    public static class GameCatalog
    {
        public static readonly Dictionary<string, ItemTemplate> Items = new Dictionary<string, ItemTemplate>();
        public static readonly Dictionary<string, PlantDefinition> Plants = new Dictionary<string, PlantDefinition>();

        static GameCatalog()
        {
            Add("seed_mooncap", "Mooncap seed", ItemKind.Seed, "mooncap", 0, 0, 6);
            Add("seed_emberroot", "Emberroot seed", ItemKind.Seed, "emberroot", 0, 0, 7);
            Add("seed_glowberry", "Glowberry seed", ItemKind.Seed, "glowberry", 0, 0, 7);
            Add("seed_dreammint", "Dreammint seed", ItemKind.Seed, "dreammint", 0, 0, 8);
            Add("seed_bitebloom", "Bitebloom seed", ItemKind.Seed, "bitebloom", 0, 0, 10);
            Add("mooncap", "Mooncap", ItemKind.Ingredient, "dream", 1, 2, 12);
            Add("emberroot", "Emberroot", ItemKind.Ingredient, "heat", 1, 2, 13);
            Add("glowberry", "Glowberry", ItemKind.Ingredient, "restore", 1, 2, 12);
            Add("dreammint", "Dreammint", ItemKind.Ingredient, "calm", 1, 1, 13);
            Add("bitebloom", "Bitebloom", ItemKind.Ingredient, "venom", 1, -3, 15);
            Add("water", "Dew water", ItemKind.Water, "plain", 0, 0, 1);
            Add("moonwater", "Moonwater", ItemKind.Water, "moon", 0, 0, 9);
            Add("compost", "Rich compost", ItemKind.Soil, "rich", 1, 0, 8);
            Add("treated_soil", "Moon-treated soil", ItemKind.Soil, "moon", 1, 0, 15);
            Add("raw_meat", "Raw morsel", ItemKind.Food, "carnivore", 1, 0, 8);
            Add("moon_shard", "Moon shard", ItemKind.Material, "moon", 1, 1, 12);
            Add("arcane_dust", "Arcane dust", ItemKind.Material, "arcane", 2, 1, 30);
            Add("parchment", "Spell parchment", ItemKind.Paper, "paper", 0, 0, 10);
            Add("empty_vial", "Glass vial", ItemKind.Material, "glass", 0, 0, 5);
            Add("worn_blade", "Worn blade", ItemKind.Weapon, "plain", 0, 0, 20);
            Add("enchant_blade", "Enchanted blade", ItemKind.Weapon, "enchanted", 0, 0, 50);

            Plants.Add("mooncap", new PlantDefinition("Mooncap", "moonwater", false, 6, 4, 2));
            Plants.Add("emberroot", new PlantDefinition("Emberroot", "water", false, 6, 3, 3));
            Plants.Add("glowberry", new PlantDefinition("Glowberry", "water", false, 6, 3, 4));
            Plants.Add("dreammint", new PlantDefinition("Dreammint", "moonwater", false, 6, 3, 5));
            Plants.Add("bitebloom", new PlantDefinition("Bitebloom", "water", true, 6, 4, 6));
        }

        private static void Add(string id, string name, ItemKind kind, string category, int power, int effect, int price)
        {
            Items[id] = new ItemTemplate(id, name, kind, category, power, effect, price);
        }

        public static ItemRecord MakeDynamic(string id, string name, ItemKind kind, string category, int power, int effect, int amount = 1)
        {
            return new ItemRecord { id = id, name = name, kind = kind.ToString(), category = category, power = power, effect = effect, amount = amount };
        }

        public static string KindOf(ItemRecord item) { return item == null ? string.Empty : item.kind ?? string.Empty; }
    }

    public sealed class PlantDefinition
    {
        public readonly string Name;
        public readonly string RequiredWater;
        public readonly bool Carnivorous;
        public readonly int RequiredWaterings;
        public readonly int HarvestYield;
        public readonly int VisualSet;

        public PlantDefinition(string name, string requiredWater, bool carnivorous, int requiredWaterings, int harvestYield, int visualSet)
        {
            Name = name; RequiredWater = requiredWater; Carnivorous = carnivorous;
            RequiredWaterings = requiredWaterings; HarvestYield = harvestYield; VisualSet = visualSet;
        }
    }
}
