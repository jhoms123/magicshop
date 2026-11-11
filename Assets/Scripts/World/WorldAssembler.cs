using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CottageMagic
{
    public sealed class WorldAssembler : MonoBehaviour
    {
        private Transform _yard;
        private Transform _home;
        private Transform _traderVisit;
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

        public void SetTraderVisible(bool visible)
        {
            if (_traderVisit != null) _traderVisit.gameObject.SetActive(visible);
        }

        public Vector2 ClampYardPosition(Vector2 point)
        {
            return new Vector2(Mathf.Clamp(point.x, -8.45f, 8.45f), Mathf.Clamp(point.y, -4.95f, 4.95f));
        }

        public Vector2 ClampInteriorPosition(Vector2 point)
        {
            return new Vector2(Mathf.Clamp(point.x, -5.65f, 5.65f), Mathf.Clamp(point.y, -5.35f, 3.95f));
        }

        public void RefreshPlots()
        {
            for (int i = 0; i < _plotVisuals.Count; i++)
                if (_plotVisuals[i] != null) _plotVisuals[i].Refresh();
        }

        private Transform MakeRoot(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            return go.transform;
        }

        private void BuildYard()
        {
            BuildGroundAndPaths();
            BuildCottageExterior();
            BuildForestBorder();
            BuildGarden();
            BuildTradeBox();
            BuildTrailEnd();
            BuildVisitingTrader();
            AddBounds(_yard, -8.8f, 8.8f, -5.2f, 5.2f, 0.7f);
        }

        private void BuildGroundAndPaths()
        {
            var gridObject = new GameObject("Yard Tile Grid");
            gridObject.transform.SetParent(_yard, false);
            gridObject.transform.localPosition = new Vector3(-0.5f, -0.5f, 0f);
            gridObject.AddComponent<Grid>().cellSize = Vector3.one;

            var grassMap = CreateTilemap(gridObject.transform, "Grass Ground", -30);
            var pathMap = CreateTilemap(gridObject.transform, "Tilled Earth Paths", -25);
            var grassSprite = PixelArt.Slice("MagicFarm/Ground", 0, 0, 16, 16);
            var soilSprite = PixelArt.Slice("MagicFarm/Ground", 16, 0, 16, 16);
            var grass = MakeTile("Sprout Lands Grass", grassSprite);
            var soilPath = MakeTile("Tilled Earth Path", soilSprite);

            for (int x = -10; x <= 10; x++)
            for (int y = -6; y <= 6; y++)
                grassMap.SetTile(new Vector3Int(x, y, 0), grass);

            for (int y = -4; y <= 2; y++) pathMap.SetTile(new Vector3Int(0, y, 0), soilPath);
            for (int x = -3; x <= 0; x++) pathMap.SetTile(new Vector3Int(x, -1, 0), soilPath);
            for (int x = 0; x <= 6; x++) pathMap.SetTile(new Vector3Int(x, 1, 0), soilPath);
            grassMap.CompressBounds();
            pathMap.CompressBounds();
        }

        private Tilemap CreateTilemap(Transform parent, string name, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tilemap = go.AddComponent<Tilemap>();
            var renderer = go.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = sortingOrder;
            renderer.mode = TilemapRenderer.Mode.Chunk;
            return tilemap;
        }

        private Tile MakeTile(string name, Sprite sprite)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = name;
            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.None;
            return tile;
        }

        private void BuildCottageExterior()
        {
            var house = PixelArt.Slice("CottageExterior", 0, 0, 63, 52);
            var housePosition = new Vector2(-0.6875f, 3.0f);
            MakeSprite(_yard, "Cottage Exterior", house, housePosition, SortOrder(housePosition.y));
            float halfWidth = house.bounds.size.x * 0.5f;
            float halfHeight = house.bounds.size.y * 0.5f;

            MakeWall(_yard, housePosition + new Vector2(-halfWidth + 0.08f, 0.05f), new Vector2(0.24f, 2.8f));
            MakeWall(_yard, housePosition + new Vector2(halfWidth - 0.08f, 0.05f), new Vector2(0.24f, 2.8f));
            MakeWall(_yard, housePosition + new Vector2(0f, halfHeight - 0.22f), new Vector2(house.bounds.size.x - 0.3f, 0.24f));

            const float doorwayOffsetX = 0.6875f;
            const float doorwayWidth = 0.82f;
            float bottomWallY = housePosition.y - halfHeight + 0.12f;
            float leftWidth = halfWidth + doorwayOffsetX - doorwayWidth * 0.5f;
            float rightWidth = halfWidth - doorwayOffsetX - doorwayWidth * 0.5f;
            MakeWall(_yard, new Vector2(housePosition.x - halfWidth + leftWidth * 0.5f, bottomWallY), new Vector2(leftWidth, 0.24f));
            MakeWall(_yard, new Vector2(housePosition.x + halfWidth - rightWidth * 0.5f, bottomWallY), new Vector2(rightWidth, 0.24f));

            YardDoorPosition = new Vector2(housePosition.x + doorwayOffsetX, housePosition.y - halfHeight + 0.17f);
            MakeInteractable(_yard, "Cottage Door", StationKind.DoorIn, "Enter the cottage", YardDoorPosition);
        }

        private void BuildForestBorder()
        {
            var forest = new[]
            {
                PixelArt.Slice("MagicFarm/Nature", 0, 0, 16, 32, PixelArt.PixelsPerUnit, new Vector2(0.5f, 0f)),
                PixelArt.Slice("MagicFarm/Nature", 16, 0, 32, 32, PixelArt.PixelsPerUnit, new Vector2(0.5f, 0f)),
                PixelArt.Slice("MagicFarm/Nature", 48, 0, 32, 32, PixelArt.PixelsPerUnit, new Vector2(0.5f, 0f))
            };

            int variant = 0;
            // Leave the upper-right clearing open for the trader's stall.
            float[] topTrees = { -8.4f, -6.8f, -5.2f, -3.6f, -2.0f, -0.4f, 1.2f, 2.8f, 4.4f };
            for (int i = 0; i < topTrees.Length; i++)
                MakeForestPatch(forest[variant++ % forest.Length], new Vector2(topTrees[i], 3.55f));

            float[] sideY = { 3.4f, 1.4f, -0.6f, -2.6f, -4.6f };
            for (int i = 0; i < sideY.Length; i++)
            {
                MakeForestPatch(forest[variant++ % forest.Length], new Vector2(-8.8f, sideY[i]));
                MakeForestPatch(forest[variant++ % forest.Length], new Vector2(8.8f, sideY[i]));
            }

            var bottomTrees = new[]
            {
                PixelArt.Slice("MagicFarm/Nature", 0, 0, 16, 32),
                PixelArt.Slice("MagicFarm/Nature", 16, 0, 32, 32),
                PixelArt.Slice("MagicFarm/Nature", 48, 0, 32, 32)
            };
            float[] bottomTreeX = { -8.4f, -6.8f, -5.2f, -3.6f, -2.0f, 2.0f, 3.6f, 5.2f, 6.8f, 8.4f };
            for (int i = 0; i < bottomTreeX.Length; i++)
                MakeForestPatch(bottomTrees[i % bottomTrees.Length], new Vector2(bottomTreeX[i], -5.7f));
        }

        private void MakeForestPatch(Sprite sprite, Vector2 position)
        {
            MakeSprite(_yard, "Forest Edge", sprite, position, -18);
        }

        private void BuildGarden()
        {
            var bed = PixelArt.Slice("MagicFarm/Ground", 0, 16, 48, 16);
            Vector2[] positions =
            {
                new Vector2(3f, -0.45f), new Vector2(6.5f, -0.45f),
                new Vector2(3f, -2.0f), new Vector2(6.5f, -2.0f),
                new Vector2(3f, -3.55f), new Vector2(6.5f, -3.55f)
            };

            _plotVisuals.Clear();
            for (int i = 0; i < positions.Length; i++)
            {
                var plotRoot = new GameObject("Garden Bed " + (i + 1));
                plotRoot.transform.SetParent(_yard, false);
                plotRoot.transform.position = positions[i];
                var bedRenderer = plotRoot.AddComponent<SpriteRenderer>();
                bedRenderer.sprite = bed;
                bedRenderer.sortingOrder = -20;
                var bedCollider = plotRoot.AddComponent<BoxCollider2D>();
                bedCollider.size = new Vector2(2.6f, 0.38f);
                bedCollider.offset = new Vector2(0, -0.25f);

                var plant = new GameObject("Growing Plant");
                plant.transform.SetParent(plotRoot.transform, false);
                plant.transform.localPosition = new Vector3(0, 0.5f, 0);
                var plantRenderer = plant.AddComponent<SpriteRenderer>();
                var plotVisual = plotRoot.AddComponent<PlotVisual>();
                plotVisual.PlotIndex = i;
                plotVisual.PlantRenderer = plantRenderer;
                var interact = plotRoot.AddComponent<WorldInteractable>();
                interact.Kind = StationKind.Planting;
                interact.PlotIndex = i;
                interact.Label = "Garden bed " + (i + 1);
                _plotVisuals.Add(plotVisual);
            }

            var flower = PixelArt.Slice("MagicFarm/Nature", 80, 0, 16, 16);
            MakeSprite(_yard, "Garden flower", flower, new Vector2(-2.65f, 0.15f), SortOrder(0.15f));
            MakeSprite(_yard, "Garden flower", flower, new Vector2(2.0f, -3.8f), SortOrder(-3.8f));
            MakeSprite(_yard, "Garden flower", flower, new Vector2(7.85f, -3.8f), SortOrder(-3.8f));
            MakeSprite(_yard, "Garden flower", flower, new Vector2(2.25f, 0.2f), SortOrder(0.2f));
            RefreshPlots();
        }

        private void BuildTradeBox()
        {
            var chest = PixelArt.Slice("MagicFarm/Furniture", 0, 0, 16, 14);
            MakeStationWithSprite(_yard, "Trade and Request Box", chest, StationKind.OrderBoard,
                "Open the trade and request box", new Vector2(-3.0f, -1.0f));
        }

        private void BuildTrailEnd()
        {
            var fence = PixelArt.Slice("MagicFarm/Furniture", 0, 16, 48, 16);
            var fenceObject = MakeSprite(_yard, "End of the Forest Trail", fence, new Vector2(0, -4.62f), SortOrder(-4.62f));
            var fenceCollider = fenceObject.AddComponent<BoxCollider2D>();
            fenceCollider.size = new Vector2(2.95f, 0.22f);
            fenceCollider.offset = new Vector2(0, 0.12f);

            var sign = PixelArt.Slice("TinyTownAtlas", 176, 96, 16, 16);
            MakeStationWithSprite(_yard, "Forest Trail Sign", sign, StationKind.Sign,
                "Read the forest trail sign", new Vector2(1.42f, -3.85f));
        }

        private void BuildVisitingTrader()
        {
            _traderVisit = new GameObject("Visiting Trader Stop").transform;
            _traderVisit.SetParent(_yard, false);
            var table = PixelArt.Slice("MagicFarm/Furniture", 16, 0, 16, 16);
            Vector2 counterCenter = new Vector2(6.125f, 1.875f);
            var counter = new GameObject("Potion Merchant's Counter");
            counter.transform.SetParent(_traderVisit, false);
            counter.transform.position = counterCenter;
            var counterCollider = counter.AddComponent<BoxCollider2D>();
            counterCollider.size = new Vector2(2.95f, 0.25f);
            counterCollider.offset = new Vector2(0f, -0.32f);

            // Three real wooden tables make a counter wide enough for the
            // products, while each retains its own y-sorted sprite.
            for (int i = -1; i <= 1; i++)
                MakeSprite(_traderVisit, "Market Table", table,
                    counterCenter + new Vector2(i, 0f), SortOrder(1.375f));

            var cauldron = PixelArt.Slice("MagicFarm/WitchProps", 0, 0, 64, 64,
                48f, new Vector2(0.5f, 0f));
            var bottles = PixelArt.Slice("MagicFarm/WitchProps", 64, 0, 64, 64,
                48f, new Vector2(0.5f, 0f));
            var herbs = PixelArt.Slice("MagicFarm/WitchProps", 128, 0, 64, 64,
                48f, new Vector2(0.5f, 0f));
            MakeSprite(_traderVisit, "Bubbling Cauldron Display", cauldron,
                new Vector2(5.1f, 2.375f), SortOrder(2.375f));
            MakeSprite(_traderVisit, "Potion Bottle Display", bottles,
                new Vector2(6.35f, 2.375f), SortOrder(2.375f));
            MakeSprite(_traderVisit, "Dried Herb Display", herbs,
                new Vector2(7.3f, 2.375f), SortOrder(2.375f));

            var merchant = new GameObject("Traveling Trader");
            merchant.transform.SetParent(_traderVisit, false);
            merchant.transform.localPosition = new Vector3(6.125f, 2.9f, 0);
            var merchantSprite = merchant.AddComponent<SpriteRenderer>();
            merchantSprite.sprite = PixelArt.Slice("MagicFarm/WitchFox", 0, 0, 64, 64,
                48f, new Vector2(0.5f, 0f));
            merchantSprite.sortingOrder = SortOrder(merchant.transform.position.y);
            var collider = merchant.AddComponent<CircleCollider2D>();
            collider.radius = 0.3f;
            collider.offset = new Vector2(0, 0.16f);
            var animation = merchant.AddComponent<IdlePixelAnimation>();
            animation.Target = merchantSprite;
            var interact = merchant.AddComponent<WorldInteractable>();
            interact.Kind = StationKind.Trader;
            interact.Label = "Trade with the visiting merchant";
        }

        private void BuildHome()
        {
            var room = PixelArt.Slice("GrannyIndoor", 0, 0, 224, 224);
            MakeSprite(_home, "Warm cottage room", room, Vector2.zero, -20);

            var clock = PixelArt.Slice("GrannyIndoor", 32, 224, 32, 32);
            var window = PixelArt.Slice("GrannyIndoor", 64, 224, 32, 32);
            var insideDoor = PixelArt.Slice("GrannyIndoor", 96, 224, 32, 64);
            var pottedPlant = PixelArt.Slice("GrannyIndoor", 0, 240, 32, 16);
            var cauldron = PixelArt.Slice("MagicFarm/WitchProps", 0, 0, 64, 64,
                48f, new Vector2(0.5f, 0f));
            var potionShelf = PixelArt.Slice("MagicFarm/WitchProps", 64, 0, 64, 64,
                48f, new Vector2(0.5f, 0f));
            var driedHerbs = PixelArt.Slice("MagicFarm/WitchProps", 128, 0, 64, 64,
                48f, new Vector2(0.5f, 0f));
            var workTable = PixelArt.Slice("MagicFarm/Furniture", 16, 0, 16, 16);
            var chest = PixelArt.Slice("MagicFarm/Furniture", 0, 0, 16, 14);
            var stool = PixelArt.Slice("MagicFarm/Furniture", 32, 0, 16, 16);

            MakeStationWithSprite(_home, "Bubbling Potion Cauldron", cauldron, StationKind.Potion,
                "Brew a potion", new Vector2(-3.05f, 2.15f));
            MakeSprite(_home, "Potion Bottle Shelf", potionShelf,
                new Vector2(-1.7f, 3.35f), SortOrder(3.35f));
            MakeSprite(_home, "Hanging Dried Herbs", driedHerbs,
                new Vector2(-4.05f, 3.25f), SortOrder(3.25f));

            MakeStationWithSprite(_home, "Scroll Writing Table", workTable, StationKind.Scroll,
                "Write scrolls and distill ink", new Vector2(-1.55f, 0.85f));
            MakeSprite(_home, "Scroll Desk Stool", stool, new Vector2(-1.55f, -0.05f), SortOrder(-0.05f));
            MakeStationWithSprite(_home, "Enchantment Table", workTable, StationKind.Enchant,
                "Enchant a blade", new Vector2(1.65f, -1.25f));
            MakeSprite(_home, "Enchantment Desk Stool", stool, new Vector2(1.65f, -2.15f), SortOrder(-2.15f));
            MakeStationWithSprite(_home, "Wall Clock", clock, StationKind.Rest,
                "Rest until morning", new Vector2(1.45f, 2.45f));
            MakeSprite(_home, "Window", window, new Vector2(2.75f, 2.95f), SortOrder(2.95f));
            MakeSprite(_home, "Potted Cottage Plant", pottedPlant, new Vector2(4.55f, -2.3f), SortOrder(-2.3f));
            MakeSprite(_home, "Cottage Storage Chest", chest, new Vector2(-4.6f, -3.25f), SortOrder(-3.25f));
            MakeSprite(_home, "Interior Cottage Door", insideDoor, new Vector2(0, -3.9f), SortOrder(-2.3f));
            MakeInteractable(_home, "Back Door", StationKind.DoorOut, "Go out to the garden", new Vector2(0, -3.15f));

            AddBounds(_home, -6.0f, 6.0f, -5.9f, 5.9f, 0.7f);
        }

        private int SortOrder(float y) { return 40 - Mathf.RoundToInt(y * 4f); }

        private GameObject MakeSprite(Transform parent, string name, Sprite sprite, Vector2 position, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            float pixelsPerUnit = PixelArt.PixelsPerUnit;
            go.transform.position = new Vector3(
                Mathf.Round(position.x * pixelsPerUnit) / pixelsPerUnit,
                Mathf.Round(position.y * pixelsPerUnit) / pixelsPerUnit,
                0f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            return go;
        }

        private GameObject MakeStationWithSprite(Transform parent, string name, Sprite sprite, StationKind kind, string label, Vector2 position)
        {
            var go = MakeSprite(parent, name, sprite, position, SortOrder(position.y));
            if (sprite != null)
            {
                var collider = go.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(Mathf.Max(0.45f, sprite.bounds.size.x * 0.76f), Mathf.Max(0.28f, sprite.bounds.size.y * 0.45f));
                collider.offset = new Vector2(0, -sprite.bounds.size.y * 0.2f);
            }
            var interact = go.AddComponent<WorldInteractable>();
            interact.Kind = kind;
            interact.Label = label;
            return go;
        }

        private GameObject MakeInteractable(Transform parent, string name, StationKind kind, string label, Vector2 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var interact = go.AddComponent<WorldInteractable>();
            interact.Kind = kind;
            interact.Label = label;
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
            var wall = new GameObject("Invisible boundary");
            wall.transform.SetParent(parent, false);
            wall.transform.position = center;
            wall.AddComponent<BoxCollider2D>().size = size;
        }
    }
}
