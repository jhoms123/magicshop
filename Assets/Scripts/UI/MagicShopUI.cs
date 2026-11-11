using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CottageMagic
{
    public sealed class MagicShopUI : MonoBehaviour
    {
        private enum Panel { None, Inventory, Orders, Planting, Potion, Scroll, Enchant, Trader }
        private Panel _panel;
        private int _plotIndex;
        private string _seedSlot;
        private string _soilSlot;
        private string _specialSlot;
        private string _inkSlot;
        private string _scrollSlot;
        private readonly List<string> _potionSlots = new List<string>();
        private Vector2 _scroll;
        private string _toast;
        private float _toastUntil;
        private bool _stylesReady;
        private Font _font;
        private GUIStyle _panelStyle, _hudStyle, _titleStyle, _textStyle, _smallStyle, _buttonStyle, _slotStyle, _cardStyle;
        private Texture2D _dialogTexture, _panelTexture, _cardTexture, _buttonTexture, _buttonHoverTexture;
        private MagicShopDirector Game => MagicShopDirector.Instance;
        public bool BlocksWorldInput => _panel != Panel.None;
        public bool IsTraderPanelOpen => _panel == Panel.Trader;

        private void Start()
        {
            _font = Resources.Load<Font>("Fonts/Silkscreen-Regular");
            if (Game != null) Game.StateChanged += RepaintState;
        }

        private void OnDestroy()
        {
            if (Game != null) Game.StateChanged -= RepaintState;
        }

        private void RepaintState() { }

        public void SetToast(string message)
        {
            _toast = message;
            _toastUntil = Time.unscaledTime + 4.2f;
        }

        public void ClosePanel() { _panel = Panel.None; _scroll = Vector2.zero; }
        public void CloseTraderPanel() { if (_panel == Panel.Trader) ClosePanel(); }
        public void ToggleInventory() { _panel = _panel == Panel.Inventory ? Panel.None : Panel.Inventory; _scroll = Vector2.zero; }
        public void OpenOrders() { _panel = Panel.Orders; _scroll = Vector2.zero; }
        public void OpenPlanting(int plotIndex) { _panel = Panel.Planting; _plotIndex = plotIndex; _seedSlot = _soilSlot = _specialSlot = null; _scroll = Vector2.zero; }
        public void OpenPotion() { _panel = Panel.Potion; _potionSlots.Clear(); _scroll = Vector2.zero; }
        public void OpenScroll() { _panel = Panel.Scroll; _inkSlot = null; _scroll = Vector2.zero; }
        public void OpenEnchant() { _panel = Panel.Enchant; _scrollSlot = null; _scroll = Vector2.zero; }
        public void OpenTrader()
        {
            if (Game != null && !Game.TraderAvailable) { SetToast("The trader is away. " + Game.NextTraderVisitText + "."); return; }
            _panel = Panel.Trader; _scroll = Vector2.zero;
        }

        private Texture2D PixelTexture(string resource)
        {
            var texture = Resources.Load<Texture2D>("Art/" + resource);
            if (texture != null)
            {
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;
            }
            return texture;
        }

        private void EnsureStyles()
        {
            if (_stylesReady) return;
            _stylesReady = true;
            _dialogTexture = PixelTexture("PixelUI_Dialog");
            _panelTexture = PixelTexture("PixelUI_Panel");
            _cardTexture = PixelTexture("PixelUI_Card");
            _buttonTexture = PixelTexture("PixelUI_Button");
            _buttonHoverTexture = PixelTexture("PixelUI_ButtonHover");

            Color ink = new Color32(71, 51, 35, 255);
            Color mutedInk = new Color32(104, 91, 67, 255);
            Color paper = new Color32(250, 235, 197, 255);
            _panelStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(22, 22, 18, 18), alignment = TextAnchor.UpperLeft };
            _panelStyle.normal.background = _dialogTexture != null ? _dialogTexture : GUI.skin.box.normal.background;
            _panelStyle.border = new RectOffset(6, 6, 6, 6);
            _hudStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(10, 10, 8, 8), alignment = TextAnchor.UpperLeft };
            _hudStyle.normal.background = _panelTexture != null ? _panelTexture : GUI.skin.box.normal.background;
            _hudStyle.border = new RectOffset(5, 5, 5, 5);

            _titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 19, fontStyle = FontStyle.Bold, wordWrap = true };
            _titleStyle.normal.textColor = ink;
            _textStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true, richText = true };
            _textStyle.normal.textColor = ink;
            _smallStyle = new GUIStyle(_textStyle) { fontSize = 12 };
            _smallStyle.normal.textColor = mutedInk;

            _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 14, padding = new RectOffset(10, 10, 7, 7), wordWrap = true, alignment = TextAnchor.MiddleCenter };
            _buttonStyle.normal.background = _buttonTexture != null ? _buttonTexture : GUI.skin.button.normal.background;
            _buttonStyle.hover.background = _buttonHoverTexture != null ? _buttonHoverTexture : _buttonStyle.normal.background;
            _buttonStyle.active.background = _buttonHoverTexture != null ? _buttonHoverTexture : _buttonStyle.normal.background;
            _buttonStyle.focused.background = _buttonHoverTexture != null ? _buttonHoverTexture : _buttonStyle.normal.background;
            _buttonStyle.disabled.background = _buttonTexture != null ? _buttonTexture : _buttonStyle.normal.background;
            _buttonStyle.normal.textColor = paper;
            _buttonStyle.hover.textColor = ink;
            _buttonStyle.active.textColor = ink;
            _buttonStyle.focused.textColor = ink;
            _buttonStyle.disabled.textColor = mutedInk;
            _buttonStyle.border = new RectOffset(5, 5, 5, 5);

            _slotStyle = new GUIStyle(GUI.skin.box) { fontSize = 13, padding = new RectOffset(8, 8, 10, 10), alignment = TextAnchor.MiddleCenter, wordWrap = true };
            _slotStyle.normal.background = _cardTexture != null ? _cardTexture : GUI.skin.box.normal.background;
            _slotStyle.normal.textColor = ink;
            _slotStyle.border = new RectOffset(5, 5, 5, 5);
            _cardStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(12, 12, 8, 8), alignment = TextAnchor.UpperLeft };
            _cardStyle.normal.background = _cardTexture != null ? _cardTexture : GUI.skin.box.normal.background;
            _cardStyle.border = new RectOffset(5, 5, 5, 5);
            if (_font != null)
            {
                _titleStyle.font = _font; _textStyle.font = _font; _smallStyle.font = _font;
                _buttonStyle.font = _font; _slotStyle.font = _font; _cardStyle.font = _font;
            }
        }

        private void OnGUI()
        {
            if (Game == null || Game.Save == null) return;
            EnsureStyles();
            float fitScale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            float scale = fitScale >= 1f ? Mathf.Floor(fitScale) : fitScale;
            if (scale <= 0) scale = 1;
            var oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) * 0.5f, (Screen.height - 720 * scale) * 0.5f, 0), Quaternion.identity, Vector3.one * scale);
            DrawHud();
            if (_panel != Panel.None) DrawPanel();
            if (!string.IsNullOrEmpty(_toast) && Time.unscaledTime < _toastUntil) DrawToast();
            GUI.matrix = oldMatrix;
        }

        private void DrawHud()
        {
            GUI.color = Color.white;
            GUI.Box(new Rect(20, 18, 408, 112), GUIContent.none, _hudStyle);
            GUILayout.BeginArea(new Rect(38, 28, 375, 96));
            GUILayout.Label(Game.ShopTier, _titleStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Coins  " + Game.Save.gold, _textStyle, GUILayout.Width(145));
            GUILayout.Label("Renown  " + Game.Reputation, _textStyle);
            GUILayout.EndHorizontal();
            GUILayout.Label("Day " + Game.Day + "   " + Game.ClockText() + "   ·   30 min / day", _smallStyle);
            GUILayout.EndArea();

            // Keep the room's back-wall window and clock unobstructed. The
            // request box remains one key away indoors and gets its full
            // schedule card in the yard where the trader is visible.
            if (Game.World == null || !Game.World.IsInterior)
            {
                GUI.Box(new Rect(884, 18, 376, 112), GUIContent.none, _hudStyle);
                GUILayout.BeginArea(new Rect(902, 25, 340, 96));
                GUILayout.Label("TRADE & REQUEST BOX", _titleStyle);
                GUILayout.Label(Game.PendingRequestCount + " new  ·  " + Game.ActiveCommissionCount + " active", _smallStyle);
                GUILayout.Label(Game.TraderStatusText, _smallStyle);
                GUILayout.EndArea();
            }

            GUI.Box(new Rect(818, 680, 442, 28), GUIContent.none, _hudStyle);
            GUILayout.BeginArea(new Rect(831, 683, 416, 22));
            GUILayout.Label("WASD MOVE   ·   E USE   ·   I SATCHEL   ·   J BOX", _smallStyle);
            GUILayout.EndArea();

            if (Game.Player != null && !string.IsNullOrEmpty(Game.Player.Prompt))
            {
                GUI.Box(new Rect(410, 650, 450, 48), GUIContent.none, _hudStyle);
                GUI.Label(new Rect(428, 660, 414, 28), Game.Player.Prompt, _textStyle);
            }
        }

        private string DaysText(float days)
        {
            if (days >= 1f) return Mathf.CeilToInt(days) + " days";
            int hours = Mathf.Max(1, Mathf.CeilToInt(days * 24));
            return hours + (hours == 1 ? " hour" : " hours");
        }

        private void DrawToast()
        {
            GUI.color = Color.white;
            GUI.Box(new Rect(335, 545, 610, 70), GUIContent.none, _hudStyle);
            GUI.color = Color.white;
            GUI.Label(new Rect(355, 555, 570, 52), _toast, _textStyle);
        }

        private void DrawPanel()
        {
            GUI.color = new Color(0.09f, 0.13f, 0.11f, 0.68f);
            GUI.DrawTexture(new Rect(0, 0, 1280, 720), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.color = Color.white;
            GUI.Box(new Rect(300, 74, 680, 572), GUIContent.none, _panelStyle);
            GUI.color = Color.white;
            GUILayout.BeginArea(new Rect(326, 92, 628, 532));
            GUILayout.BeginHorizontal();
            GUILayout.Label(PanelTitle(), _titleStyle, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Close  ×", _buttonStyle, GUILayout.Width(112), GUILayout.Height(40))) ClosePanel();
            GUILayout.EndHorizontal();
            GUILayout.Space(6);
            _scroll = GUILayout.BeginScrollView(_scroll, false, true, GUILayout.ExpandHeight(true));
            switch (_panel)
            {
                case Panel.Inventory: DrawInventory(); break;
                case Panel.Orders: DrawOrders(); break;
                case Panel.Planting: DrawPlanting(); break;
                case Panel.Potion: DrawPotion(); break;
                case Panel.Scroll: DrawScroll(); break;
                case Panel.Enchant: DrawEnchant(); break;
                case Panel.Trader: DrawTrader(); break;
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private string PanelTitle()
        {
            switch (_panel)
            {
                case Panel.Inventory: return "Satchel";
                case Panel.Orders: return "Trade & Request Box";
                case Panel.Planting: return "Garden · Bed " + (_plotIndex + 1);
                case Panel.Potion: return "Potion Hearth";
                case Panel.Scroll: return "Ink & Scroll Desk";
                case Panel.Enchant: return "Enchanting Cabinet";
                case Panel.Trader: return "Traveling Trader";
                default: return "Magic Shop";
            }
        }

        private void DrawInventory()
        {
            GUILayout.Label("Everything you are carrying, including ingredients, water and garden supplies.", _smallStyle);
            GUILayout.Space(8);
            foreach (var item in Game.Inventory.OrderBy(i => i.kind).ThenBy(i => i.name))
            {
                GUILayout.BeginHorizontal(_cardStyle);
                GUILayout.Label(item.name + "  ×" + item.amount + (item.power > 0 ? "  · Power " + item.power : ""), _textStyle, GUILayout.ExpandWidth(true));
                GUILayout.Label(KindLabel(item.kind), _smallStyle, GUILayout.Width(94));
                GUILayout.EndHorizontal();
            }
            if (Game.Inventory.Count == 0) GUILayout.Label("Your satchel is empty. Visit the garden or trader to restock.", _textStyle);
        }

        private string KindLabel(string kind)
        {
            if (kind == ItemKind.Ingredient.ToString()) return "Ingredient";
            if (kind == ItemKind.Seed.ToString()) return "Seed";
            return kind;
        }

        private void DrawOrders()
        {
            GUILayout.Label("Customer requests arrive here. Accept one to start its deadline, or decline it with no penalty.", _smallStyle);
            GUILayout.Space(10);
            foreach (var order in Game.Orders.OrderBy(o => o.accepted).ThenBy(o => o.dueAt).ToList())
            {
                GUILayout.BeginVertical(_cardStyle);
                GUILayout.Label(order.request + "  ·  " + order.client, _textStyle);
                GUILayout.Label(order.kind + " · Power " + order.minimumPower + "+  ·  " + order.reward + " coins / " + order.reputationReward + " renown", _smallStyle);
                if (order.accepted)
                {
                    GUILayout.Label("Commission active  ·  " + DaysText(Game.GetOrderDaysLeft(order)) + " remaining", _smallStyle);
                    if (GUILayout.Button("Deliver this request", _buttonStyle, GUILayout.Height(38)))
                        Run((out string msg) => Game.DeliverOrder(order.id, out msg));
                }
                else
                {
                    GUILayout.Label("New request  ·  deadline begins after you accept  ·  " + order.durationDays + (order.durationDays == 1 ? " day to complete" : " days to complete"), _smallStyle);
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Accept request", _buttonStyle, GUILayout.Height(38)))
                        Run((out string msg) => Game.AcceptOrder(order.id, out msg));
                    if (GUILayout.Button("Decline", _buttonStyle, GUILayout.Height(38)))
                        Run((out string msg) => Game.DeclineOrder(order.id, out msg));
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
                GUILayout.Space(6);
            }
            if (Game.Orders.Count == 0) GUILayout.Label("The box is clear. New requests arrive with each morning's post.", _textStyle);

            GUILayout.Space(12);
            GUILayout.Label("RECENT TRADES & REQUEST HISTORY", _titleStyle);
            if (Game.ActivityLog.Count == 0) GUILayout.Label("Purchases, sales, request decisions and deliveries will be recorded here.", _smallStyle);
            for (int i = Game.ActivityLog.Count - 1; i >= 0; i--)
            {
                var entry = Game.ActivityLog[i];
                GUILayout.BeginHorizontal(_cardStyle);
                GUILayout.Label("D" + entry.day + "  " + entry.time, _smallStyle, GUILayout.Width(112));
                GUILayout.Label(entry.message, _smallStyle, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
                GUILayout.Space(4);
            }
        }

        private void DrawPlanting()
        {
            var plot = Game.PlotAt(_plotIndex);
            if (plot == null) { GUILayout.Label("This bed is missing.", _textStyle); return; }
            if (!plot.unlocked)
            {
                GUILayout.Label("Make a little more space in the yard.", _textStyle);
                ActionButton("Open this garden bed · 55 coins", () => Run((out string msg) => Game.BuyPlot(_plotIndex, out msg)));
                return;
            }
            if (!string.IsNullOrEmpty(plot.species))
            {
                var definition = GameCatalog.Plants[plot.species];
                int stage = Game.GrowthStage(plot);
                GUILayout.Label(definition.Name + "  ·  Growth stage " + (stage + 1) + " / 4", _titleStyle);
                GUILayout.Label("Watered " + plot.wateringCount + " / " + definition.RequiredWaterings + " separate days. Needs " + (definition.RequiredWater == "moonwater" ? "Moonwater" : "Dew water") + ".", _textStyle);
                if (definition.Carnivorous) GUILayout.Label("A morsel was added to its special item slot when planted.", _smallStyle);
                GUILayout.Space(12);
                ActionButton("Water this plant", () => Run((out string msg) => Game.Water(_plotIndex, out msg)), plot.wateringCount < definition.RequiredWaterings);
                ActionButton("Harvest ripe plant", () => Run((out string msg) => Game.Harvest(_plotIndex, out msg)), plot.wateringCount >= definition.RequiredWaterings);
                GUILayout.Label("Plants may be watered once per in-game day. Rest at the cottage clock to move to the next morning.", _smallStyle);
                return;
            }

            GUILayout.Label("Put a seed, optional soil and one optional special item into the three slots.", _smallStyle);
            GUILayout.Space(7);
            GUILayout.BeginHorizontal();
            GUILayout.Box("SEED\n" + SlotName(_seedSlot), _slotStyle, GUILayout.Width(184), GUILayout.Height(68));
            GUILayout.Box("SOIL\n" + SlotName(_soilSlot), _slotStyle, GUILayout.Width(184), GUILayout.Height(68));
            GUILayout.Box("SPECIAL ITEM\n" + SlotName(_specialSlot), _slotStyle, GUILayout.Width(210), GUILayout.Height(68));
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("Choose a seed", _textStyle);
            ItemButtons(ItemKind.Seed, id => _seedSlot = id, "Seed slot");
            GUILayout.Label("Choose soil (optional)", _textStyle);
            ItemButtons(ItemKind.Soil, id => _soilSlot = id, "Soil slot");
            GUILayout.Label("Choose a special item (optional; Bitebloom needs a morsel)", _textStyle);
            ItemButtons(new[] { ItemKind.Food, ItemKind.Material }, id => _specialSlot = id, "Special slot");
            GUILayout.Space(8);
            ActionButton("Plant selected seed", () =>
            {
                if (Game.Plant(_plotIndex, _seedSlot, _soilSlot, _specialSlot, out var message)) { SetToast(message); _seedSlot = _soilSlot = _specialSlot = null; }
                else SetToast(message);
            }, !string.IsNullOrEmpty(_seedSlot));
        }

        private string SlotName(string id)
        {
            if (string.IsNullOrEmpty(id)) return "Empty";
            var item = Game.FindItem(id); return item == null ? "Empty" : item.name;
        }

        private void ItemButtons(ItemKind kind, Action<string> select, string slot)
        {
            foreach (var item in Game.Inventory.Where(i => i.kind == kind.ToString()).ToList())
                if (GUILayout.Button("Add " + item.name + "  ×" + item.amount, _buttonStyle)) select(item.id);
        }

        private void ItemButtons(ItemKind[] kinds, Action<string> select, string slot)
        {
            foreach (var item in Game.Inventory.Where(i => kinds.Contains((ItemKind)Enum.Parse(typeof(ItemKind), i.kind))).ToList())
                if (GUILayout.Button("Add " + item.name + "  ×" + item.amount, _buttonStyle)) select(item.id);
        }

        private void DrawPotion()
        {
            GUILayout.Label("Choose up to three garden ingredients. Different effects combine into a stronger brew.", _smallStyle);
            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            for (int i = 0; i < 3; i++)
            {
                string name = i < _potionSlots.Count ? SlotName(_potionSlots[i]) : "Empty";
                GUILayout.Box("INGREDIENT " + (i + 1) + "\n" + name, _slotStyle, GUILayout.Width(128), GUILayout.Height(70));
            }
            GUILayout.Label("=", _titleStyle, GUILayout.Width(24));
            GUILayout.Box("RESULT\n" + PotionPreview(), _slotStyle, GUILayout.Width(176), GUILayout.Height(70));
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            foreach (var item in Game.Inventory.Where(i => i.kind == ItemKind.Ingredient.ToString()).ToList())
            {
                int selectedCount = _potionSlots.Count(id => id == item.id);
                bool canAdd = _potionSlots.Count < 3 && item.amount > selectedCount;
                if (GUILayout.Button(item.name + "  ×" + (item.amount - selectedCount) + "   ·   " + item.category + " effect", _buttonStyle) && canAdd)
                    _potionSlots.Add(item.id);
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Remove last", _buttonStyle, GUILayout.Width(150)) && _potionSlots.Count > 0) _potionSlots.RemoveAt(_potionSlots.Count - 1);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Brew potion", _buttonStyle, GUILayout.Width(205), GUILayout.Height(45)))
            {
                if (Game.BrewPotion(new List<string>(_potionSlots), out var message)) { SetToast(message); _potionSlots.Clear(); }
                else SetToast(message);
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("Bites and bitter plants can make a harmful potion. Check your ingredient effects in the satchel.", _smallStyle);
        }

        private string PotionPreview()
        {
            if (_potionSlots.Count == 0) return "Add ingredients";
            int effect = 0, score = 0;
            var categories = new HashSet<string>();
            foreach (string id in _potionSlots)
            {
                var item = Game.FindItem(id);
                if (item == null) continue;
                effect += item.effect;
                score += item.effect + item.power;
                categories.Add(item.category);
            }
            if (categories.Count > 1) score += categories.Count;
            string quality = effect < 0 ? "Toxic" : effect == 0 ? "Neutral" : effect <= 2 ? "Gentle" : effect <= 5 ? "Bright" : "Potent";
            int power = Mathf.Clamp(Mathf.CeilToInt(Mathf.Abs(score) / 2f) + _potionSlots.Count - 1, 1, 9);
            return quality + " · " + power;
        }

        private void DrawScroll()
        {
            GUILayout.Label("Turn a gathered ingredient into ink, then spend one sheet of parchment to inscribe a scroll.", _smallStyle);
            GUILayout.Space(8);
            GUILayout.Label("Distill ink from", _textStyle);
            foreach (var item in Game.Inventory.Where(i => i.kind == ItemKind.Ingredient.ToString()).ToList())
            {
                if (GUILayout.Button("Distill " + item.name + " into ink  ·  " + item.category, _buttonStyle))
                {
                    if (Game.DistillInk(item.id, out var msg)) { _inkSlot = "ink_" + item.category; SetToast(msg); }
                    else SetToast(msg);
                }
            }
            GUILayout.Space(8);
            GUILayout.Label("Choose an ink vial", _textStyle);
            var inks = Game.Inventory.Where(i => i.kind == ItemKind.Ink.ToString()).ToList();
            foreach (var ink in inks)
            {
                if (GUILayout.Button((ink.id == _inkSlot ? "✓  " : "Select  ") + ink.name + "  ×" + ink.amount + "  ·  power " + ink.power, _buttonStyle)) _inkSlot = ink.id;
            }
            GUILayout.Label("Parchment available: " + Game.Count("parchment"), _smallStyle);
            ActionButton("Write selected scroll", () =>
            {
                if (Game.WriteScroll(_inkSlot, out var msg)) { SetToast(msg); _inkSlot = null; }
                else SetToast(msg);
            }, !string.IsNullOrEmpty(_inkSlot));
        }

        private void DrawEnchant()
        {
            GUILayout.Label("Enchanting opens at 35 renown. Use a power 3 scroll, a worn blade and arcane dust.", _smallStyle);
            GUILayout.Space(8);
            GUILayout.Label("Renown  " + Game.Reputation + " / 35", _textStyle);
            var scrolls = Game.Inventory.Where(i => i.kind == ItemKind.Scroll.ToString()).ToList();
            if (scrolls.Count == 0) GUILayout.Label("No scrolls in the satchel yet. Start at the ink and scroll desk.", _smallStyle);
            foreach (var scroll in scrolls)
            {
                bool eligible = scroll.power >= 3;
                if (GUILayout.Button((scroll.id == _scrollSlot ? "✓  " : "Select  ") + scroll.name + (eligible ? "  ·  ready" : "  ·  needs power 3"), _buttonStyle)) _scrollSlot = scroll.id;
            }
            GUILayout.Label("Worn blades  " + Game.Count("worn_blade") + "     Arcane dust  " + Game.Count("arcane_dust"), _smallStyle);
            ActionButton("Enchant the blade", () => Run((out string msg) => Game.Enchant(_scrollSlot, out msg)), Game.Reputation >= 35 && !string.IsNullOrEmpty(_scrollSlot));
        }

        private void DrawTrader()
        {
            if (!Game.TraderAvailable)
            {
                GUILayout.Label("The traveling trader has packed up for the day.", _textStyle);
                GUILayout.Label(Game.NextTraderVisitText, _smallStyle);
                return;
            }
            GUILayout.Label("The merchant wanders the lane and trades useful supplies for a fair price.", _smallStyle);
            GUILayout.Space(8);
            GUILayout.Label("BUY", _titleStyle);
            string[] offers = { "moonwater", "compost", "treated_soil", "parchment", "raw_meat", "arcane_dust" };
            foreach (string id in offers)
            {
                var item = GameCatalog.Items[id];
                if (GUILayout.Button("Buy " + item.Name + "  ·  " + item.Price + " coins", _buttonStyle)) Run((out string msg) => Game.Buy(id, out msg));
            }
            GUILayout.Space(12);
            GUILayout.Label("SELL  ·  roughly 70% of the usual value", _titleStyle);
            var sellable = Game.Inventory.Where(i => i.kind != ItemKind.Seed.ToString() && i.kind != ItemKind.Water.ToString() && i.kind != ItemKind.Weapon.ToString()).ToList();
            foreach (var item in sellable)
            {
                string itemId = item.id;
                if (GUILayout.Button("Sell " + item.name + "  ×" + item.amount, _buttonStyle)) Run((out string msg) => Game.Sell(itemId, out msg));
            }
            if (sellable.Count == 0) GUILayout.Label("No goods the trader can buy right now.", _smallStyle);
        }

        private void Run(DirectorAction action)
        {
            string message;
            action(out message);
            SetToast(message);
        }

        private void Run(Action action) { action(); }

        private delegate bool DirectorAction(out string message);

        private bool ActionButton(string label, DirectorAction action, bool enabled = true)
        {
            GUI.enabled = enabled;
            bool pressed = GUILayout.Button(label, _buttonStyle, GUILayout.Height(42));
            GUI.enabled = true;
            if (pressed) Run(action);
            return pressed;
        }

        private bool ActionButton(string label, Action action, bool enabled = true)
        {
            GUI.enabled = enabled;
            bool pressed = GUILayout.Button(label, _buttonStyle, GUILayout.Height(42));
            GUI.enabled = true;
            if (pressed) action();
            return pressed;
        }
    }
}
