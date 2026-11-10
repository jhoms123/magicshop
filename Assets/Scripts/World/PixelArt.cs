using System.Collections.Generic;
using UnityEngine;

namespace CottageMagic
{
    public static class PixelArt
    {
        public const float PixelsPerUnit = 16f;
        private static readonly Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

        private static Texture2D Texture(string key)
        {
            if (Textures.TryGetValue(key, out var cached)) return cached;
            var texture = Resources.Load<Texture2D>("Art/" + key);
            if (texture == null) Debug.LogError("Missing pixel art texture: " + key);
            else
            {
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;
                Textures[key] = texture;
            }
            return texture;
        }

        // Coordinates use an origin at the top-left of the original art sheet.
        public static Sprite Slice(string sheet, int x, int yFromTop, int width, int height, float pixelsPerUnit = PixelsPerUnit)
        {
            return Slice(sheet, x, yFromTop, width, height, pixelsPerUnit, new Vector2(0.5f, 0.5f));
        }

        public static Sprite Slice(string sheet, int x, int yFromTop, int width, int height, float pixelsPerUnit, Vector2 pivot)
        {
            string key = sheet + "_" + x + "_" + yFromTop + "_" + width + "_" + height + "_" + pixelsPerUnit + "_" + pivot.x + "_" + pivot.y;
            if (Sprites.TryGetValue(key, out var cached)) return cached;

            var texture = Texture(sheet);
            if (texture == null) return null;
            if (x < 0 || yFromTop < 0 || width <= 0 || height <= 0 || x + width > texture.width || yFromTop + height > texture.height)
            {
                Debug.LogError("Pixel art crop is outside " + sheet + " (" + x + ", " + yFromTop + ", " + width + ", " + height + ") on " + texture.width + "x" + texture.height + " sheet.");
                return null;
            }

            var rect = new Rect(x, texture.height - yFromTop - height, width, height);
            var sprite = Sprite.Create(texture, rect, pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = key;
            Sprites[key] = sprite;
            return sprite;
        }

        public static Sprite MaleFrame(int column, int row)
        {
            // The CC0 16x16 base sheet uses 48x64 source cells (3x export scale).
            return Slice("base_male", column * 48, row * 64, 48, 64, 48f);
        }

        // Puny Characters uses 32x32 source cells, with eight directions per
        // row. Cropping transparent padding keeps the actual people readable
        // at the game's 16-pixel-per-unit scale. The custom pivot preserves
        // the original cell center, so animation does not wobble as frames change.
        public static Sprite CharacterFrame(string sheet, int direction, bool walking, int frame)
        {
            int column = Mathf.Clamp(direction, 0, 7) * 3 + Mathf.Clamp(frame, 0, 2);
            int row = walking ? 1 : 0;
            return Slice(sheet, column * 32 + 8, row * 32 + 2, 18, 26, PixelsPerUnit, new Vector2(8f / 18f, 12f / 26f));
        }

        public static Sprite GrannyFrame(int column, int row)
        {
            return Slice("GrannySprite", column * 32, row * 32, 32, 32, 24f);
        }
    }
}
