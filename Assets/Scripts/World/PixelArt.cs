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

        // Puny Characters uses eight 32x32 facing rows and three animation
        // frames in the first three columns. Keep the complete silhouette,
        // trim the sheet padding, and anchor at the feet so the sprite, collider,
        // and depth sorting all agree about where the character stands.
        public static Sprite CharacterFrame(string sheet, int direction, bool walking, int frame)
        {
            int column = walking ? Mathf.Clamp(frame, 0, 2) : 1;
            int row = Mathf.Clamp(direction, 0, 7);
            // Slice the complete 32x32 frame. The sheet contains a few frames
            // whose pixels extend beyond the old 18x26 trim rectangle.
            // Keeping the full cell prevents hats, hands, and other details
            // from being clipped. The low pivot keeps the character grounded.
            return Slice(sheet, column * 32, row * 32, 32, 32, PixelsPerUnit, new Vector2(0.5f, 0.125f));
        }

        public static Sprite GrannyFrame(int column, int row)
        {
            return Slice("GrannySprite", column * 32, row * 32, 32, 32, 24f);
        }

        // The reference game's character sheet has four facing rows and two
        // walking frames per row. The sprites were trimmed into a small,
        // purpose-built atlas so the character uses the same 16px scale as
        // the crops and soil tiles.
        public static Sprite SproutCharacterFrame(int direction, bool walking, int frame)
        {
            int row = Mathf.Clamp(direction, 0, 3);
            int column = walking ? Mathf.Clamp(frame, 0, 1) : 0;
            return Slice("MagicFarm/People", column * 16, row * 16, 16, 16,
                PixelsPerUnit, new Vector2(0.5f, 0f));
        }
    }
}
