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
            // Use complete source tiles from the CC0 Puny World sheet. The old
            // scene repeated a plant fragment as grass and stitched unrelated
            // atlas cells into the house and trees.
            var grassTiles = new[] {
                PixelArt.Slice("PunyWorld", 0, 0, 16, 16),
                PixelArt.Slice("PunyWorld", 16, 0, 16, 16),
                PixelArt.Slice("PunyWorld", 32, 0, 16, 16),
                PixelArt.Slice("PunyWorld", 16, 16, 16, 16)
            };
            for (int x = -13; x <= 13; x++)
            for (int y = -8; y <= 8; y++)
            {
                int variant = Mathf.Abs(x * 17 + y * 29 + x * y * 3) % grassTiles.Length;
                MakeSprite(_yard, "Lawn tile", grassTiles[variant], new Vector2(x, y), -30);
            }

            var dirtTile = PixelArt.Slice("PunyWorld", 112, 16, 16, 16);
            for (int y = -1; y <= 2; y++) AddPathCell(dirtTile, -5, y);
            for (int x = -5; x <= 7; x++) AddPathCell(dirtTile, x, -1);
            for (int y = -1; y <= 0; y++) AddPathCell(dirtTile, 0, y);

            // A complete tiny cottage sprite from the same CC0 world tileset,
            // enlarged with point filtering so its pixels stay crisp.
            var house = PixelArt.Slice("PunyWorld", 80, 416, 16, 16, 16f / 3f);
            var housePosition = new Vector2(-5.5f, 2.65f);
            var houseObject = MakeSprite(_yard, "Cottage Exterior", house, housePosition, 19);
            houseObject.GetComponent<SpriteRenderer>().sortingOrder = 40 - Mathf.RoundToInt(housePosition.y * 4f);
            MakeWall(_yard, housePosition + new Vector2(-1.35f, 0.1f), new Vector2(0.3f, 2.0f));
            MakeWall(_yard, housePosition + new Vector2(1.35f, 0.1f), new Vector2(0.3f, 2.0f));
            MakeWall(_yard, housePosition + new Vector2(0f, 1.25f), new Vector2(2.4f, 0.3f));
            MakeWall(_yard, housePosition + new Vector2(-0.85f, -1.25f), new Vector2(0.7f, 0.3f));
            MakeWall(_yard, housePosition + new Vector2(0.85f, -1.25f), new Vector2(0.7f, 0.3f));
            YardDoorPosition = housePosition + new Vector2(0f, -1.7f);
            MakeInteractable(_yard, "Cottage Door", StationKind.DoorIn, "Enter the cottage", YardDoorPosition);

            // Full tree clusters from Puny World frame the yard without exposing
            // the cut-up pieces that were visible in the first build.
            var treeCluster = PixelArt.Slice("PunyWorld", 0, 112, 48, 48);
            MakeTree(treeCluster, new Vector2(-9.3f, 2.4f));
            MakeTree(treeCluster, new Vector2(-9.3f, -2.3f));
            MakeTree(treeCluster, new Vector2(9.3f, 2.4f));
            MakeTree(treeCluster, new Vector2(10.0f, -2.3f));

            // Kenney's full raised-bed sprite is 32x16 at (32,128) on Tiny Farm.
            var bed = PixelArt.Slice("TinyFarmAtlas", 32, 128, 32, 16);
            Vector2[] bedLocations = {
                new Vector2(2.0f, -2.7f), new Vector2(4.5f, -2.7f), new Vector2(7.0f, -2.7f),
                new Vector2(2.0f, -4.9f), new Vector2(4.5f, -4.9f), new Vector2(7.0f, -4.9f)
            };
            _plotVisuals.Clear();
            for (int i = 0; i < bedLocations.Length; i++)
            {
                var plotRoot = new GameObject("Garden Bed " + (i + 1)); plotRoot.transform.SetParent(_yard, false); plotRoot.transform.position = bedLocations[i];
                var bedSprite = plotRoot.AddComponent<SpriteRenderer>(); bedSprite.sprite = bed; bedSprite.sortingOrder = 40 - Mathf.RoundToInt(bedLocations[i].y * 4f);
                var bedCollider = plotRoot.AddComponent<BoxCollider2D>(); bedCollider.size = new Vector2(1.75f, 0.38f); bedCollider.offset = new Vector2(0, -0.25f);
                var plant = new GameObject("Growing Plant"); plant.transform.SetParent(plotRoot.transform, false); plant.transform.localPosition = new Vector3(0, 0.55f, 0);
                var plantRenderer = plant.AddComponent<SpriteRenderer>(); plantRenderer.sortingOrder = 2;
                var plotVisual = plotRoot.AddComponent<PlotVisual>(); plotVisual.PlotIndex = i; plotVisual.PlantRenderer = plantRenderer;
                var interact = plotRoot.AddComponent<WorldInteractable>(); interact.Kind = StationKind.Planting; interact.PlotIndex = i; interact.Label = "Garden bed " + (i + 1);
                _plotVisuals.Add(plotVisual);
            }

            var board = PixelArt.Slice("PunyWorld", 16, 480, 16, 16, 8f);
            MakeStationWithSprite(_yard, "Order Board", board, StationKind.OrderBoard, "Check customer orders", new Vector2(0f, 1.2f));
            var stall = PixelArt.Slice("PunyWorld", 144, 496, 16, 16, 8f);
            MakeSprite(_yard, "Traveling trader's stall", stall, new Vector2(7.4f, 0.4f), 15);
            var vendor = new GameObject("Visiting Trader"); vendor.transform.SetParent(_yard, false); vendor.transform.position = new Vector3(7.4f, 1.75f, 0);
            var vendorSprite = vendor.AddComponent<SpriteRenderer>(); vendorSprite.sprite = PixelArt.CharacterFrame("PunyMerchant", 0, false, 1); vendorSprite.sortingOrder = 40 - Mathf.RoundToInt(vendor.transform.position.y * 4f);
            var vendorCollider = vendor.AddComponent<CircleCollider2D>(); vendorCollider.radius = 0.3f; vendorCollider.offset = new Vector2(0, -0.15f);
            var bob = vendor.AddComponent<IdlePixelAnimation>(); bob.Target = vendorSprite; bob.CharacterSheet = "PunyMerchant"; bob.FacingDirection = 0;
            var trader = vendor.AddComponent<WorldInteractable>(); trader.Kind = StationKind.Trader; trader.Label = "Trade with the traveling merchant";
            AddBounds(_yard, -11f, 11f, -7f, 7f, 0.7f);
            RefreshPlots();
        }

        private void BuildHome()
        {
            var room = PixelArt.Slice("GrannyIndoor", 0, 0, 224, 224);
            MakeSprite(_home, "Warm cottage interior", room, Vector2.zero, -20);

            // The CC0 interior sheet supplies the fireplace, clock, window,
            // door and the small furnishings used as the crafting stations.
            var fireplace = PixelArt.Slice("GrannyIndoor", 0, 256, 96, 48);
            var clock = PixelArt.Slice("GrannyIndoor", 32, 224, 32, 32);
            var window = PixelArt.Slice("GrannyIndoor", 64, 224, 32, 32);
            var insideDoor = PixelArt.Slice("GrannyIndoor", 96, 224, 32, 64);
            var bluePot = PixelArt.Slice("GrannyIndoor", 128, 224, 48, 32);
            var workTable = PixelArt.Slice("GrannyIndoor", 0, 240, 32, 16);
            var magicCabinet = PixelArt.Slice("GrannyIndoor", 128, 224, 48, 32);

            MakeStationWithSprite(_home, "Potion Hearth", fireplace, StationKind.Potion, "Brew a potion", new Vector2(-3.4f, 2.2f), 1);
            MakeStationWithSprite(_home, "Scroll Desk", workTable, StationKind.Scroll, "Make scrolls and ink", new Vector2(2.5f, 1.7f), 1);
            MakeStationWithSprite(_home, "Enchantment Cabinet", magicCabinet, StationKind.Enchant, "Enchant a blade", new Vector2(2.8f, -1.4f), 1);
            MakeStationWithSprite(_home, "Resting Clock", clock, StationKind.Rest, "Rest until morning", new Vector2(-4.3f, -2.3f), 1);
            MakeSprite(_home, "Window", window, new Vector2(3.7f, 4.0f), -5);
            MakeSprite(_home, "Interior cottage door", insideDoor, new Vector2(0, -4.8f), -6);
            MakeSprite(_home, "Blue Flower Pot", bluePot, new Vector2(4.3f, 3.4f), -4);
            MakeInteractable(_home, "Back Door", StationKind.DoorOut, "Go out to the garden", new Vector2(0, -3.75f));
            AddBounds(_home, -6.0f, 6.0f, -6.0f, 6.0f, 0.7f);
        }

        private void AddPathCell(Sprite tile, int x, int y)
        {
            MakeSprite(_yard, "Garden path tile", tile, new Vector2(x, y), -25);
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
