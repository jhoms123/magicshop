using System.Collections.Generic;
using UnityEngine;

namespace CottageMagic
{
    public sealed class WorldAssembler : MonoBehaviour
    {
        private Transform _yard;
        private Transform _home;
        private readonly List<PlotVisual> _plotVisuals = new List<PlotVisual>();
        public bool IsInterior { get; private set; }
        public Vector2 YardDoorPosition { get; private set; }

        public void Build()
        {
            _yard = MakeRoot("Cottage Yard");
            _home = MakeRoot("The Cozy House");
            BuildYard();
            BuildHome();
        }

        public void SetInterior(bool interior)
        {
            IsInterior = interior;
            if (_yard != null) _yard.gameObject.SetActive(!interior);
            if (_home != null) _home.gameObject.SetActive(interior);
        }

        public void RefreshPlots()
        {
            for (int i = 0; i < _plotVisuals.Count; i++) if (_plotVisuals[i] != null) _plotVisuals[i].Refresh();
        }

        private Transform MakeRoot(string name)
        {
            var go = new GameObject(name); go.transform.SetParent(transform, false); return go.transform;
        }

        private void BuildYard()
        {
            var grass = PixelArt.Slice("TinyTownAtlas", 0, 0, 16, 16);
            var path = PixelArt.Slice("TinyTownAtlas", 0, 16, 16, 16);
            for (int x = -13; x <= 13; x++)
            for (int y = -9; y <= 9; y++)
                MakeSprite(_yard, "Grass", grass, new Vector2(x, y), -30);

            // A packed dirt path is assembled from Kenney's original town tiles.
            for (int y = -6; y <= 3; y++) AddGroundCell(path, -4, y);
            for (int x = -4; x <= 4; x++) AddGroundCell(path, x, -6);
            for (int y = -5; y <= -1; y++) AddGroundCell(path, 3, y);

            var house = PixelArt.Slice("TinyTownAtlas", 0, 64, 48, 48);
            var houseObject = MakeSprite(_yard, "Cottage Exterior", house, new Vector2(-5.0f, 5.15f), 19);
            var frontWall = houseObject.AddComponent<BoxCollider2D>(); frontWall.size = new Vector2(2.7f, 0.45f); frontWall.offset = new Vector2(-0.55f, -1.12f);
            YardDoorPosition = new Vector2(-3.8f, 3.85f);
            MakeInteractable(_yard, "Cottage Door", StationKind.DoorIn, "Enter the cottage", YardDoorPosition);

            // The tree silhouettes, garden containers, flowers and crop sprites are all source art.
            var treeGreen = PixelArt.Slice("TinyTownAtlas", 64, 0, 16, 24);
            var treeSmall = PixelArt.Slice("TinyTownAtlas", 64, 24, 16, 24);
            var treeRound = PixelArt.Slice("TinyTownAtlas", 80, 0, 32, 32);
            var treeGold = PixelArt.Slice("TinyTownAtlas", 48, 0, 16, 24);
            MakeTree(treeGreen, new Vector2(-10, 4.8f));
            MakeTree(treeRound, new Vector2(-8.4f, -1.7f));
            MakeTree(treeGold, new Vector2(10.8f, 5.1f));
            MakeTree(treeSmall, new Vector2(11.1f, -3.0f));
            MakeTree(treeRound, new Vector2(9.5f, 7.4f));

            // A small starting garden with three open beds and three purchasable beds.
            var bed = PixelArt.Slice("TinyFarmAtlas", 0, 64, 32, 16);
            Vector2[] bedLocations = {
                new Vector2(2.4f, -2.2f), new Vector2(5.0f, -2.2f), new Vector2(7.6f, -2.2f),
                new Vector2(2.4f, -5.0f), new Vector2(5.0f, -5.0f), new Vector2(7.6f, -5.0f)
            };
            _plotVisuals.Clear();
            for (int i = 0; i < bedLocations.Length; i++)
            {
                var plotRoot = new GameObject("Garden Bed " + (i + 1)); plotRoot.transform.SetParent(_yard, false); plotRoot.transform.position = bedLocations[i];
                var bedSprite = plotRoot.AddComponent<SpriteRenderer>(); bedSprite.sprite = bed; bedSprite.sortingOrder = 40 - Mathf.RoundToInt(bedLocations[i].y * 4f);
                var bedCollider = plotRoot.AddComponent<BoxCollider2D>(); bedCollider.size = new Vector2(1.6f, 0.36f); bedCollider.offset = new Vector2(0, -0.22f);
                var plant = new GameObject("Growing Plant"); plant.transform.SetParent(plotRoot.transform, false); plant.transform.localPosition = new Vector3(0, 0.42f, 0);
                var plantRenderer = plant.AddComponent<SpriteRenderer>(); plantRenderer.sortingOrder = 2;
                var plotVisual = plotRoot.AddComponent<PlotVisual>(); plotVisual.PlotIndex = i; plotVisual.PlantRenderer = plantRenderer;
                var interact = plotRoot.AddComponent<WorldInteractable>(); interact.Kind = StationKind.Planting; interact.PlotIndex = i; interact.Label = "Garden bed " + (i + 1);
                _plotVisuals.Add(plotVisual);
            }

            var board = PixelArt.Slice("TinyTownAtlas", 176, 80, 16, 32);
            MakeStationWithSprite(_yard, "Order Board", board, StationKind.OrderBoard, "Check customer orders", new Vector2(-0.5f, 0.8f));
            var vendor = new GameObject("Visiting Trader"); vendor.transform.SetParent(_yard, false); vendor.transform.position = new Vector3(10.4f, 2.0f, 0);
            var vendorSprite = vendor.AddComponent<SpriteRenderer>(); vendorSprite.sprite = PixelArt.GrannyFrame(0, 0); vendorSprite.sortingOrder = 40 - Mathf.RoundToInt(vendor.transform.position.y * 4f);
            var vendorCollider = vendor.AddComponent<CircleCollider2D>(); vendorCollider.radius = 0.3f; vendorCollider.offset = new Vector2(0, -0.15f);
            var bob = vendor.AddComponent<IdlePixelAnimation>(); bob.Target = vendorSprite;
            var trader = vendor.AddComponent<WorldInteractable>(); trader.Kind = StationKind.Trader; trader.Label = "Trade with the traveling merchant";
            AddBounds(_yard, -13f, 13f, -9f, 9f, 0.7f);
            RefreshPlots();
        }

        private void BuildHome()
        {
            var room = PixelArt.Slice("GrannyIndoor", 0, 0, 224, 224);
            MakeSprite(_home, "Warm cottage interior", room, Vector2.zero, -20);

            // Existing furniture from the CC0 interior sheet gives the little home its fireplace,
            // clock and window. Crafting props use the matching farm pack's tables and containers.
            var fireplace = PixelArt.Slice("GrannyIndoor", 0, 256, 64, 32);
            var clock = PixelArt.Slice("GrannyIndoor", 32, 224, 24, 48);
            var window = PixelArt.Slice("GrannyIndoor", 64, 224, 32, 32);
            var wallArt = PixelArt.Slice("GrannyIndoor", 0, 224, 32, 16);
            var insideDoor = PixelArt.Slice("GrannyIndoor", 96, 224, 32, 64);
            var bluePot = PixelArt.Slice("GrannyIndoor", 128, 224, 48, 32);
            var workTable = PixelArt.Slice("TinyFarmAtlas", 32, 128, 32, 16);
            var magicCabinet = PixelArt.Slice("TinyFarmAtlas", 64, 128, 32, 16);

            MakeStationWithSprite(_home, "Potion Hearth", fireplace, StationKind.Potion, "Brew a potion", new Vector2(-3.4f, 2.2f), 1);
            MakeStationWithSprite(_home, "Scroll Desk", workTable, StationKind.Scroll, "Make scrolls and ink", new Vector2(2.5f, 1.7f), 1);
            MakeStationWithSprite(_home, "Enchantment Cabinet", magicCabinet, StationKind.Enchant, "Enchant a blade", new Vector2(2.8f, -1.4f), 1);
            MakeStationWithSprite(_home, "Resting Clock", clock, StationKind.Rest, "Rest until morning", new Vector2(-4.3f, -2.3f), 1);
            MakeSprite(_home, "Window", window, new Vector2(3.7f, 4.0f), -5);
            MakeSprite(_home, "Framed cottage painting", wallArt, new Vector2(-0.1f, 5.4f), -5);
            MakeSprite(_home, "Interior cottage door", insideDoor, new Vector2(0, -4.8f), -6);
            MakeSprite(_home, "Blue Flower Pot", bluePot, new Vector2(4.3f, 3.4f), -4);
            MakeInteractable(_home, "Back Door", StationKind.DoorOut, "Go out to the garden", new Vector2(0, -3.75f));
            AddBounds(_home, -6.0f, 6.0f, -6.0f, 6.0f, 0.7f);
        }

        private void AddGroundCell(Sprite tile, int x, int y)
        {
            if (tile == null) return;
            MakeSprite(_yard, "Garden Path", tile, new Vector2(x, y), -25);
        }

        private GameObject MakeSprite(Transform parent, string name, Sprite sprite, Vector2 position, int sortingOrder)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.position = position;
            var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingOrder = sortingOrder;
            return go;
        }

        private GameObject MakeStationWithSprite(Transform parent, string name, Sprite sprite, StationKind kind, string label, Vector2 position, int sortingOrder = 2)
        {
            var go = MakeSprite(parent, name, sprite, position, sortingOrder);
            if (sprite != null)
            {
                go.GetComponent<SpriteRenderer>().sortingOrder = 40 - Mathf.RoundToInt(position.y * 4f);
                var collider = go.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(Mathf.Max(0.45f, sprite.bounds.size.x * 0.76f), Mathf.Max(0.28f, sprite.bounds.size.y * 0.45f));
                collider.offset = new Vector2(0, -sprite.bounds.size.y * 0.2f);
            }
            var interact = go.AddComponent<WorldInteractable>(); interact.Kind = kind; interact.Label = label;
            return go;
        }

        private void MakeTree(Sprite sprite, Vector2 position)
        {
            var tree = MakeSprite(_yard, "Garden Tree", sprite, position, 40 - Mathf.RoundToInt(position.y * 4f));
            var trunk = tree.AddComponent<CircleCollider2D>(); trunk.radius = 0.23f; trunk.offset = new Vector2(0, -sprite.bounds.size.y * 0.34f);
        }

        private GameObject MakeInteractable(Transform parent, string name, StationKind kind, string label, Vector2 position)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.position = position;
            var interact = go.AddComponent<WorldInteractable>(); interact.Kind = kind; interact.Label = label;
            return go;
        }

        private void AddBounds(Transform parent, float left, float right, float bottom, float top, float thickness)
        {
            MakeWall(parent, new Vector2((left + right) * 0.5f, top + thickness * 0.5f), new Vector2(right - left + thickness * 2f, thickness));
            MakeWall(parent, new Vector2((left + right) * 0.5f, bottom - thickness * 0.5f), new Vector2(right - left + thickness * 2f, thickness));
            MakeWall(parent, new Vector2(left - thickness * 0.5f, (bottom + top) * 0.5f), new Vector2(thickness, top - bottom));
            MakeWall(parent, new Vector2(right + thickness * 0.5f, (bottom + top) * 0.5f), new Vector2(thickness, top - bottom));
        }

        private void MakeWall(Transform parent, Vector2 center, Vector2 size)
        {
            var wall = new GameObject("Invisible yard boundary"); wall.transform.SetParent(parent, false); wall.transform.position = center;
            var collider = wall.AddComponent<BoxCollider2D>(); collider.size = size;
        }
    }
}
