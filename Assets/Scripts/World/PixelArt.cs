using System.Collections.Generic;
using UnityEngine;

namespace CottageMagic
{
    public static class PixelArt
    {
        private static readonly Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

        private static Texture2D Texture(string key)
        {
            if (Textures.TryGetValue(key, out var cached)) return cached;
            var texture = Resources.Load<Texture2D>("Art/" + key);
            if (texture == null) Debug.LogError("Missing pixel art texture: " + key);
            else { texture.filterMode = FilterMode.Point; texture.wrapMode = TextureWrapMode.Clamp; Textures[key] = texture; }
            return texture;
        }

        // All sheet coordinates passed here use an origin at the top-left of the original file.
        public static Sprite Slice(string sheet, int x, int yFromTop, int width, int height, float pixelsPerUnit = 16f)
        {
            string key = sheet + "_" + x + "_" + yFromTop + "_" + width + "_" + height + "_" + pixelsPerUnit;
            if (Sprites.TryGetValue(key, out var cached)) return cached;
            var texture = Texture(sheet);
            if (texture == null) return null;
            var rect = new Rect(x, texture.height - yFromTop - height, width, height);
            var sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = key;
            Sprites[key] = sprite;
            return sprite;
        }

        public static Sprite MaleFrame(int column, int row)
        {
            // The CC0 16x16 base sheet uses 48x64 source cells (3x export scale).
            return Slice("base_male", column * 48, row * 64, 48, 64, 48f);
        }

        // Shade's CC0 Puny Characters sheet has three 32x32 frames for each of
        // eight directions. Row 0 is idle and row 1 is walking.
        public static Sprite CharacterFrame(string sheet, int direction, bool walking, int frame)
        {
            int column = Mathf.Clamp(direction, 0, 7) * 3 + Mathf.Clamp(frame, 0, 2);
            int row = walking ? 1 : 0;
            return Slice(sheet, column * 32, row * 32, 32, 32, 16f);
        }

        public static Sprite GrannyFrame(int column, int row)
        {
            return Slice("GrannySprite", column * 32, row * 32, 32, 32, 24f);
        }
    }
}
