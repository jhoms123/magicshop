
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CottageMagic.EditorTools
{
    public class AtlasSpritePickerWindow : EditorWindow
    {
        private const int CellSize = 16;
        private const int GridColumns = 12;

        private Texture2D farmAtlas;
        private Texture2D townAtlas;

        private Vector2 farmScroll;
        private Vector2 townScroll;
        private Vector2 assignmentScroll;

        private string selectedAtlas = "TinyFarmAtlas";
        private int selectedColumn;
        private int selectedRow;

        private string customItemName = "NewItem";

        private readonly Dictionary<string, SpriteSelection> assignments =
            new Dictionary<string, SpriteSelection>();

        private readonly string[] standardSlots =
        {
            "Trade Chest",
            "Home Storage Chest",
            "Blue Bottle",
            "Clay Pot",
            "Garden Flower",
            "Garden Bed",
            "Watering Can",
            "Barrel",
            "Shovel",
            "Axe",
            "Scythe",
            "Wood House Window",
            "Wood House Door",
            "Wood House Double Door Left",
            "Wood House Double Door Right",
            "Brick House Window",
            "Barn Top Right",
            "Barn Bottom Right",
            "Well Top",
            "Arrow Target"
        };

        [Serializable]
        private class SpriteSelection
        {
            public string atlas;
            public int column;
            public int row;
            public int width = CellSize;
            public int height = CellSize;
        }

        [MenuItem("Tools/Cottage Magic/Atlas Sprite Picker")]
        public static void Open()
        {
            var window = GetWindow<AtlasSpritePickerWindow>();
            window.titleContent = new GUIContent("Atlas Picker");
            window.minSize = new Vector2(700, 650);
            window.Show();
        }

        private void OnEnable()
        {
            FindAtlases();
        }

        private void FindAtlases()
        {
            if (farmAtlas == null)
                farmAtlas = FindTexture("TinyFarmAtlas");

            if (townAtlas == null)
                townAtlas = FindTexture("TinyTownAtlas");
        }

        private Texture2D FindTexture(string textureName)
        {
            string[] guids = AssetDatabase.FindAssets(textureName + " t:Texture2D");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                if (texture != null &&
                    texture.name.IndexOf(textureName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return texture;
                }
            }

            return null;
        }

        private void OnGUI()
        {
            FindAtlases();

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Cottage Magic - Sprite Atlas Picker",
                EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Click a tile in either atlas to select it. Then assign it to an item " +
                "or create a custom assignment. The generated code uses PixelArt.Slice() " +
                "with coordinates measured from the top-left of the atlas.",
                MessageType.Info);

            DrawAtlasFields();

            EditorGUILayout.Space(8);
            DrawSelectedSprite();

            EditorGUILayout.Space(8);
            DrawAtlasTabs();

            EditorGUILayout.Space(8);
            DrawAssignments();

            EditorGUILayout.Space(8);
            DrawGeneratedCode();
        }

        private void DrawAtlasFields()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Atlas Textures", EditorStyles.boldLabel);

            farmAtlas = (Texture2D)EditorGUILayout.ObjectField(
                "TinyFarmAtlas", farmAtlas, typeof(Texture2D), false);

            townAtlas = (Texture2D)EditorGUILayout.ObjectField(
                "TinyTownAtlas", townAtlas, typeof(Texture2D), false);

            if (farmAtlas == null || townAtlas == null)
            {
                EditorGUILayout.HelpBox(
                    "If an atlas was not found automatically, drag its Texture2D asset here.",
                    MessageType.Warning);
            }

            if (GUILayout.Button("Find Atlases Again"))
            {
                farmAtlas = null;
                townAtlas = null;
                FindAtlases();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSelectedSprite()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Selected Sprite", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Atlas", selectedAtlas);
            EditorGUILayout.LabelField("Column", selectedColumn.ToString());
            EditorGUILayout.LabelField("Row", selectedRow.ToString());
            EditorGUILayout.LabelField("X coordinate", (selectedColumn * CellSize).ToString());
            EditorGUILayout.LabelField("Y coordinate", (selectedRow * CellSize).ToString());

            Texture2D texture = GetSelectedTexture();

            if (texture != null)
            {
                Rect previewRect = GUILayoutUtility.GetRect(96, 96,
                    GUILayout.Width(96), GUILayout.Height(96));

                EditorGUI.DrawRect(previewRect, new Color(0.15f, 0.15f, 0.15f));

                Rect uv = GetCellUV(texture, selectedColumn, selectedRow);
                GUI.DrawTextureWithTexCoords(previewRect, texture, uv, true);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawAtlasTabs()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Toggle(selectedAtlas == "TinyFarmAtlas",
                    "TinyFarmAtlas", "Button"))
            {
                selectedAtlas = "TinyFarmAtlas";
            }

            if (GUILayout.Toggle(selectedAtlas == "TinyTownAtlas",
                    "TinyTownAtlas", "Button"))
            {
                selectedAtlas = "TinyTownAtlas";
            }

            EditorGUILayout.EndHorizontal();

            if (selectedAtlas == "TinyFarmAtlas")
                DrawAtlasGrid(farmAtlas, ref farmScroll);
            else
                DrawAtlasGrid(townAtlas, ref townScroll);
        }

        private void DrawAtlasGrid(Texture2D texture, ref Vector2 scroll)
        {
            EditorGUILayout.BeginVertical("box");

            if (texture == null)
            {
                EditorGUILayout.HelpBox("Atlas texture is missing.", MessageType.Error);
                EditorGUILayout.EndVertical();
                return;
            }

            int columns = Mathf.Max(1, texture.width / CellSize);
            int rows = Mathf.Max(1, texture.height / CellSize);

            EditorGUILayout.LabelField(
                texture.name + "  (" + texture.width + " x " + texture.height + ")",
                EditorStyles.boldLabel);

            if (texture.width % CellSize != 0 || texture.height % CellSize != 0)
            {
                EditorGUILayout.HelpBox(
                    "This atlas is not an exact multiple of 16 pixels. " +
                    "The grid displays complete 16x16 cells only.",
                    MessageType.Warning);
            }

            scroll = EditorGUILayout.BeginScrollView(scroll,
                GUILayout.Height(270));

            float tileSize = 42f;

            for (int row = 0; row < rows; row++)
            {
                EditorGUILayout.BeginHorizontal();

                for (int column = 0; column < columns; column++)
                {
                    Rect tileRect = GUILayoutUtility.GetRect(
                        tileSize, tileSize,
                        GUILayout.Width(tileSize),
                        GUILayout.Height(tileSize));

                    bool isSelected =
                        selectedAtlas == texture.name &&
                        selectedColumn == column &&
                        selectedRow == row;

                    EditorGUI.DrawRect(tileRect,
                        isSelected ? new Color(0.2f, 0.65f, 0.3f) :
                        new Color(0.22f, 0.22f, 0.22f));

                    Rect imageRect = new Rect(
                        tileRect.x + 3,
                        tileRect.y + 3,
                        tileRect.width - 6,
                        tileRect.height - 6);

                    GUI.DrawTextureWithTexCoords(
                        imageRect,
                        texture,
                        GetCellUV(texture, column, row),
                        true);

                    if (GUI.Button(tileRect, GUIContent.none,
                            GUIStyle.none))
                    {
                        selectedAtlas = texture.name;
                        selectedColumn = column;
                        selectedRow = row;
                        Repaint();
                    }

                    GUI.Label(
                        new Rect(tileRect.x, tileRect.yMax - 12,
                            tileRect.width, 11),
                        column + "," + row,
                        TinyLabelStyle());
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private Rect GetCellUV(Texture2D texture, int column, int row)
        {
            float x = (float)(column * CellSize) / texture.width;
            float y = 1f - (float)((row + 1) * CellSize) / texture.height;
            float width = (float)CellSize / texture.width;
            float height = (float)CellSize / texture.height;

            return new Rect(x, y, width, height);
        }

        private GUIStyle TinyLabelStyle()
        {
            return new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.LowerCenter,
                fontSize = 8,
                normal =
                {
                    textColor = Color.white
                }
            };
        }

        private Texture2D GetSelectedTexture()
        {
            return selectedAtlas == "TinyFarmAtlas"
                ? farmAtlas
                : townAtlas;
        }

        private void DrawAssignments()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Assign Sprites to Game Items",
                EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Select a tile above, then click an item button to assign that tile.",
                MessageType.None);

            assignmentScroll = EditorGUILayout.BeginScrollView(
                assignmentScroll, GUILayout.Height(240));

            int columns = 2;

            for (int i = 0; i < standardSlots.Length; i += columns)
            {
                EditorGUILayout.BeginHorizontal();

                for (int j = 0; j < columns && i + j < standardSlots.Length; j++)
                {
                    string slot = standardSlots[i + j];

                    string buttonText = slot;

                    if (assignments.TryGetValue(slot, out SpriteSelection assigned))
                    {
                        buttonText += "  [C" + assigned.column +
                                      " R" + assigned.row + "]";
                    }

                    if (GUILayout.Button(buttonText, GUILayout.Height(28)))
                    {
                        Assign(slot);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Custom Item", EditorStyles.boldLabel);

            customItemName = EditorGUILayout.TextField(
                "Item Name", customItemName);

            if (GUILayout.Button("Assign Selected Tile to Custom Item"))
            {
                if (!string.IsNullOrWhiteSpace(customItemName))
                    Assign(customItemName.Trim());
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(6);

            if (GUILayout.Button("Clear All Assignments"))
            {
                if (EditorUtility.DisplayDialog(
                        "Clear assignments?",
                        "Remove all assigned sprites?",
                        "Clear", "Cancel"))
                {
                    assignments.Clear();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void Assign(string itemName)
        {
            assignments[itemName] = new SpriteSelection
            {
                atlas = selectedAtlas,
                column = selectedColumn,
                row = selectedRow
            };

            Debug.Log(
                "Assigned " + itemName + " to " + selectedAtlas +
                " column " + selectedColumn +
                " row " + selectedRow);
        }

        private void DrawGeneratedCode()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Generated PixelArt.Slice() Code",
                EditorStyles.boldLabel);

            string code = GenerateCode();

            EditorGUILayout.TextArea(code, GUILayout.MinHeight(150));

            if (GUILayout.Button("Copy Code"))
            {
                EditorGUIUtility.systemCopyBuffer = code;
                ShowNotification(new GUIContent("Code copied!"));
            }

            EditorGUILayout.EndVertical();
        }

        private string GenerateCode()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("// Generated by Cottage Magic Atlas Sprite Picker");
            builder.AppendLine("// Coordinates are measured from the top-left of each atlas.");
            builder.AppendLine();

            foreach (var pair in assignments)
            {
                string variableName = MakeVariableName(pair.Key);
                SpriteSelection sprite = pair.Value;

                int x = sprite.column * CellSize;
                int y = sprite.row * CellSize;

                builder.Append("Sprite ")
                    .Append(variableName)
                    .Append(" = PixelArt.Slice(\"")
                    .Append(sprite.atlas)
                    .Append("\", ")
                    .Append(x)
                    .Append(", ")
                    .Append(y)
                    .Append(", ")
                    .Append(sprite.width)
                    .Append(", ")
                    .Append(sprite.height)
                    .AppendLine(");");
            }

            return builder.ToString();
        }

        private string MakeVariableName(string name)
        {
            StringBuilder result = new StringBuilder();
            bool capitalizeNext = false;

            foreach (char c in name)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    capitalizeNext = true;
                    continue;
                }

                if (result.Length == 0)
                    result.Append(char.ToLowerInvariant(c));
                else if (capitalizeNext)
                    result.Append(char.ToUpperInvariant(c));
                else
                    result.Append(c);

                capitalizeNext = false;
            }

            if (result.Length == 0)
                return "sprite";

            if (char.IsDigit(result[0]))
                result.Insert(0, '_');

            return result.ToString();
        }
    }
}
