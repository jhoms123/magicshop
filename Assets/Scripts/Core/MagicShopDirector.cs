using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CottageMagic
{
    public sealed class MagicShopDirector : MonoBehaviour
    {
        public static MagicShopDirector Instance { get; private set; }
        public const int PlotCount = 6;
        public const double MinutesPerRealSecond = 0.8; // 24 in-game hours pass in 30 real minutes.
        private const string SaveFile = "magicshop-save.json";

        public SaveData Save { get; private set; }
        public WorldAssembler World { get; set; }
        public PlayerMotor Player { get; set; }
        public MagicShopUI UI { get; set; }
        public int Day => Mathf.Max(1, (int)Math.Floor(Save.worldMinutes / 1440.0) + 1);
        public int MinuteOfDay => ((int)Save.worldMinutes % 1440 + 1440) % 1440;
        public int Reputation => Save.reputation;
        public string ShopTier => Save.reputation < 25 ? "Hearthside Herb Shop" : Save.reputation < 60 ? "Glimmergarden Apothecary" : Save.reputation < 120 ? "The Moonlit Emporium" : "Enchanter's House";
        public bool TraderAvailable => Day % 3 == 2 && MinuteOfDay >= 600 && MinuteOfDay < 1020;
        public string TraderStatusText => TraderAvailable ? "Trader is here until 5:00pm" : NextTraderVisitText;
        public int PendingRequestCount => Save.orders.Count(order => !order.accepted);
        public int ActiveCommissionCount => Save.orders.Count(order => order.accepted);
        public List<ShopLogEntry> ActivityLog => Save.activityLog;
        public event Action StateChanged;
        private float _autosaveTimer;
        private int _observedDay;
        private bool? _lastTraderAvailability;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadOrStart();
            _observedDay = Day;
        }

        private void Update()
        {
            if (Save == null) return;
            Save.worldMinutes += Time.unscaledDeltaTime * MinutesPerRealSecond;
            if (Day != _observedDay) OnMorning();
            UpdateTraderPresence();
            ProcessExpiredOrders();
            _autosaveTimer += Time.unscaledDeltaTime;
            if (_autosaveTimer >= 15f) { _autosaveTimer = 0; SaveGame(false); }
        }

        private void LoadOrStart()
        {
            var file = Path.Combine(Application.persistentDataPath, SaveFile);
            if (File.Exists(file))
            {
                try { Save = JsonUtility.FromJson<SaveData>(File.ReadAllText(file)); }
                catch (Exception e) { Debug.LogWarning("Save could not be read: " + e.Message); }
            }
            if (Save == null) StartNewGame();
            else if (Save.version == 1)
            {
                if (Save.orders != null)
                {
                    foreach (var order in Save.orders)
                    {
                        order.accepted = true;
                        order.durationDays = Mathf.Max(1, Mathf.CeilToInt((float)((order.dueAt - order.issuedAt) / 1440.0)));
                    }
                }
                Save.activityLog = new List<ShopLogEntry>();
                Save.version = 2;
            }
            else if (Save.version != 2) StartNewGame();
            if (Save.inventory == null) Save.inventory = new List<ItemRecord>();
            if (Save.plots == null) Save.plots = new List<PlantRecord>();
            if (Save.orders == null) Save.orders = new List<OrderRecord>();
            if (Save.activityLog == null) Save.activityLog = new List<ShopLogEntry>();
            for (int i = Save.plots.Count; i < PlotCount; i++) Save.plots.Add(new PlantRecord { plotIndex = i, unlocked = i < 3, lastWaterDay = -1 });
            if (Save.orders.Count == 0) CreateOrder(true);
            SaveGame(false);
        }

        private void StartNewGame()
        {
            Save = new SaveData { gold = 75, reputation = 0, worldMinutes = 480, lastOrderDay = 1 };
            for (int i = 0; i < PlotCount; i++) Save.plots.Add(new PlantRecord { plotIndex = i, unlocked = i < 3, lastWaterDay = -1 });
            Add("seed_mooncap", 2); Add("seed_emberroot", 2); Add("seed_glowberry", 2);
            Add("seed_dreammint", 1); Add("seed_bitebloom", 1);
            Add("water", 12); Add("moonwater", 4); Add("compost", 3); Add("treated_soil", 1);
            Add("raw_meat", 2); Add("moon_shard", 2); Add("arcane_dust", 1);
            Add("parchment", 4); Add("empty_vial", 3); Add("worn_blade", 1);
            Add("mooncap", 2); Add("glowberry", 2); Add("emberroot", 1); Add("dreammint", 1); Add("bitebloom", 1);
            CreateOrder(true);
        }

        private void Add(string id, int amount)
        {
            if (GameCatalog.Items.TryGetValue(id, out var template)) AddItem(template.Stack(amount));
        }

        private void OnMorning()
        {
            int today = Day;
            int oldDay = _observedDay;
            _observedDay = today;
            for (int d = Math.Max(oldDay + 1, Save.lastOrderDay + 1); d <= today; d++)
            {
                if (Save.orders.Count(o => !o.penalized) < 4) CreateOrder(false);
                Save.lastOrderDay = d;
            }
            Notify();
        }

        private void CreateOrder(bool first)
        {
            string kind = "Potion";
            string request;
            string client;
            int power = 1;
            int reward = 32;
            int rep = 8;
            if (first)
            {
                client = "Miri, the fern keeper";
                request = "A small restorative brew";
            }
            else if (Save.reputation >= 65 && UnityEngine.Random.value > 0.55f)
            {
                kind = "Weapon"; request = "An enchanted blade"; client = "Bramblewatch guard"; power = 3; reward = 115; rep = 24;
            }
            else if (Save.reputation >= 20 && UnityEngine.Random.value > 0.5f)
            {
                kind = "Scroll"; request = "A useful field scroll"; client = "Lark, the wayfarer"; power = 2; reward = 72; rep = 16;
            }
            else
            {
                int variant = UnityEngine.Random.Range(0, 3);
                client = variant == 0 ? "Miri, the fern keeper" : variant == 1 ? "Pip, the mushroom forager" : "Sable, the candle maker";
                request = variant == 0 ? "A small restorative brew" : variant == 1 ? "A curious potion" : "Something to calm the nerves";
                reward = 32 + Save.reputation / 4;
                rep = 8 + Save.reputation / 30;
            }

            int durationDays = first ? 2 : Save.reputation < 35 ? 2 : 1;
            var order = new OrderRecord
            {
                id = Guid.NewGuid().ToString("N").Substring(0, 8), client = client, request = request,
                kind = kind, minimumPower = power, reward = reward, reputationReward = rep,
                durationDays = durationDays, issuedAt = Save.worldMinutes, dueAt = 0, accepted = false
            };
            Save.orders.Add(order);
            LogActivity("Request received from " + client + ": " + request + ".");
            if (UI != null) UI.SetToast("A new request arrived from " + client + ".");
        }

        private void ProcessExpiredOrders()
        {
            bool changed = false;
            for (int i = Save.orders.Count - 1; i >= 0; i--)
            {
                var order = Save.orders[i];
                if (order.accepted && !order.penalized && Save.worldMinutes > order.dueAt)
                {
                    order.penalized = true;
                    Save.reputation = Mathf.Max(0, Save.reputation - Mathf.Max(3, order.reputationReward / 2));
                    LogActivity("Request missed for " + order.client + ". Reputation fell by " + Mathf.Max(3, order.reputationReward / 2) + ".");
                    Save.orders.RemoveAt(i);
                    changed = true;
                    if (UI != null) UI.SetToast(order.client + "'s deadline passed. Reputation fell.");
                }
            }
            if (changed) Notify();
        }

        public string ClockText()
        {
            int hour = MinuteOfDay / 60;
            int minute = MinuteOfDay % 60;
            string suffix = hour >= 12 ? "pm" : "am";
            int displayHour = hour % 12; if (displayHour == 0) displayHour = 12;
            return displayHour.ToString("00") + ":" + minute.ToString("00") + suffix;
        }

        public float GetOrderDaysLeft(OrderRecord order)
        {
            return order == null || !order.accepted ? 0 : Mathf.Max(0, (float)((order.dueAt - Save.worldMinutes) / 1440.0));
        }

        public string NextTraderVisitText
        {
            get
            {
                if (TraderAvailable) return "Trader is here until 5:00pm";
                int offset = (2 - Day % 3 + 3) % 3;
                if (offset == 0 && MinuteOfDay >= 1020) offset = 3;
                if (offset == 0) return "Trader arrives today at 10:00am";
                return "Trader returns on Day " + (Day + offset) + " at 10:00am";
            }
        }

        private void UpdateTraderPresence()
        {
            bool available = TraderAvailable;
            if (_lastTraderAvailability.HasValue && _lastTraderAvailability.Value == available) return;
            bool wasAvailable = _lastTraderAvailability.GetValueOrDefault(available);
            _lastTraderAvailability = available;
            if (World != null) World.SetTraderVisible(available);
            if (!available && UI != null && UI.IsTraderPanelOpen)
            {
                UI.CloseTraderPanel();
                UI.SetToast("The traveling merchant has packed up. " + NextTraderVisitText + ".");
            }
            else if (available && !wasAvailable && UI != null)
                UI.SetToast("The traveling merchant has arrived at the garden path.");
        }

        private void LogActivity(string message)
        {
            if (Save.activityLog == null) Save.activityLog = new List<ShopLogEntry>();
            Save.activityLog.Add(new ShopLogEntry { day = Day, time = ClockText(), message = message });
        }

        public bool AcceptOrder(string id, out string message)
        {
            var order = Save.orders.FirstOrDefault(candidate => candidate.id == id);
            if (order == null) { message = "That request is no longer in the box."; return false; }
            if (order.accepted) { message = "You already accepted this request."; return false; }
            order.accepted = true;
            order.issuedAt = Save.worldMinutes;
            order.dueAt = Save.worldMinutes + Mathf.Max(1, order.durationDays) * 1440.0;
            message = "Accepted " + order.client + "'s request. You have " + order.durationDays + (order.durationDays == 1 ? " day." : " days.");
            LogActivity(message);
            Notify();
            return true;
        }

        public bool DeclineOrder(string id, out string message)
        {
            var order = Save.orders.FirstOrDefault(candidate => candidate.id == id);
            if (order == null) { message = "That request is no longer in the box."; return false; }
            if (order.accepted) { message = "An accepted commission cannot be declined."; return false; }
            Save.orders.Remove(order);
            message = "Declined " + order.client + "'s request.";
            LogActivity(message);
            Notify();
            return true;
        }

        public List<ItemRecord> Inventory => Save.inventory;
        public List<PlantRecord> Plots => Save.plots;
        public List<OrderRecord> Orders => Save.orders;

        public ItemRecord FindItem(string id) { return Save.inventory.FirstOrDefault(i => i.id == id && i.amount > 0); }
        public int Count(string id) { var item = FindItem(id); return item == null ? 0 : item.amount; }

        public void AddItem(ItemRecord item)
        {
            if (item == null || item.amount <= 0) return;
            var existing = Save.inventory.FirstOrDefault(i => i.id == item.id);
            if (existing != null) existing.amount += item.amount;
            else Save.inventory.Add(item.Copy(item.amount));
        }

        public bool Remove(string id, int amount = 1)
        {
            var item = FindItem(id);
            if (item == null || item.amount < amount) return false;
            item.amount -= amount;
            if (item.amount <= 0) Save.inventory.Remove(item);
            return true;
        }

        public void Interact(WorldInteractable target)
        {
            if (target == null || UI == null) return;
            switch (target.Kind)
            {
                case StationKind.Planting: UI.OpenPlanting(target.PlotIndex); break;
                case StationKind.Potion: UI.OpenPotion(); break;
                case StationKind.Scroll: UI.OpenScroll(); break;
                case StationKind.Enchant: UI.OpenEnchant(); break;
                case StationKind.OrderBoard: UI.OpenOrders(); break;
                case StationKind.Trader: UI.OpenTrader(); break;
                case StationKind.Rest: RestUntilMorning(); break;
                case StationKind.DoorIn: EnterHouse(); break;
                case StationKind.DoorOut: ExitHouse(); break;
                case StationKind.Sign: UI.SetToast("The path ends here. The forest beyond is too thick to pass."); break;
            }
        }

        public void EnterHouse()
        {
            if (World == null || Player == null) return;
            Save.indoors = true; World.SetInterior(true); Player.Teleport(new Vector2(0, -3.4f)); Notify();
        }

        public void ExitHouse()
        {
            if (World == null || Player == null) return;
            Save.indoors = false; World.SetInterior(false); Player.Teleport(World.YardDoorPosition + new Vector2(0.35f, -0.75f)); Notify();
        }

        public void RestoreScene()
        {
            if (World == null || Player == null) return;
            World.SetInterior(Save.indoors);
            Vector2 fallback = Save.indoors ? new Vector2(0, -3.4f) : new Vector2(0, -0.3f);
            Vector2 savedPosition = new Vector2(Save.playerX, Save.playerY).sqrMagnitude < 0.01f ? fallback : new Vector2(Save.playerX, Save.playerY);
            Player.Teleport(Save.indoors ? World.ClampInteriorPosition(savedPosition) : World.ClampYardPosition(savedPosition));
            World.SetTraderVisible(TraderAvailable);
            _lastTraderAvailability = TraderAvailable;
        }

        public bool Plant(int plotIndex, string seedId, string soilId, string charmId, out string message)
        {
            if (plotIndex < 0 || plotIndex >= PlotCount) { message = "That garden place is not available."; return false; }
            var plot = Save.plots[plotIndex];
            if (!plot.unlocked) { message = "This garden place is locked."; return false; }
            if (!string.IsNullOrEmpty(plot.species)) { message = "Something is already growing here."; return false; }
            if (string.IsNullOrEmpty(seedId) || !GameCatalog.Items.TryGetValue(seedId, out var seed) || seed.Kind != ItemKind.Seed || Count(seedId) < 1)
            { message = "Choose a seed from your bag first."; return false; }
            string species = seed.Category;
            if (!GameCatalog.Plants.TryGetValue(species, out var definition)) { message = "That seed has no growing notes yet."; return false; }
            if (definition.Carnivorous && charmId != "raw_meat") { message = "Bitebloom needs a morsel in its special item slot."; return false; }
            if (!Remove(seedId)) { message = "That seed is no longer in your bag."; return false; }
            if (!string.IsNullOrEmpty(soilId) && !Remove(soilId)) { AddItem(seed.Stack(1)); message = "The selected soil is missing."; return false; }
            if (!string.IsNullOrEmpty(charmId) && !Remove(charmId))
            {
                AddItem(seed.Stack(1)); if (!string.IsNullOrEmpty(soilId)) Add(soilId, 1);
                message = "The special item is missing."; return false;
            }
            plot.species = species; plot.soil = soilId; plot.charm = charmId; plot.wateringCount = 0; plot.lastWaterDay = -1;
            plot.bonusPower = (!string.IsNullOrEmpty(soilId) ? 1 : 0) + (!string.IsNullOrEmpty(charmId) && charmId != "raw_meat" ? 1 : 0);
            message = definition.Name + " planted. It needs " + definition.RequiredWater + " and " + definition.RequiredWaterings + " separate waterings.";
            Notify(); return true;
        }

        public bool Water(int plotIndex, out string message)
        {
            var plot = PlotAt(plotIndex);
            if (plot == null || string.IsNullOrEmpty(plot.species)) { message = "Plant a seed here first."; return false; }
            var plant = GameCatalog.Plants[plot.species];
            if (plot.wateringCount >= plant.RequiredWaterings) { message = "This plant is ready to harvest."; return false; }
            if (plot.lastWaterDay == Day) { message = "This plant has had its water today. Come back tomorrow."; return false; }
            if (!Remove(plant.RequiredWater)) { message = plant.Name + " needs " + (plant.RequiredWater == "moonwater" ? "Moonwater" : "Dew water") + "."; return false; }
            plot.lastWaterDay = Day;
            plot.wateringCount++;
            int stage = GrowthStage(plot);
            message = plant.Name + " drank deeply. Growth stage " + (stage + 1) + " of 4. Water again on another day.";
            Notify(); return true;
        }

        public bool Harvest(int plotIndex, out string message)
        {
            var plot = PlotAt(plotIndex);
            if (plot == null || string.IsNullOrEmpty(plot.species)) { message = "Nothing is ready to harvest."; return false; }
            var plant = GameCatalog.Plants[plot.species];
            if (plot.wateringCount < plant.RequiredWaterings) { message = "Give it more water on separate days first."; return false; }
            string species = plot.species;
            Add(species, plant.HarvestYield + plot.bonusPower);
            Add("seed_" + species, 1);
            plot.species = null; plot.soil = null; plot.charm = null; plot.wateringCount = 0; plot.lastWaterDay = -1; plot.bonusPower = 0;
            message = "Gathered " + plant.HarvestYield + " " + plant.Name + " and saved a seed.";
            Notify(); return true;
        }

        public bool BuyPlot(int plotIndex, out string message)
        {
            var plot = PlotAt(plotIndex);
            if (plot == null) { message = "That garden place is not available."; return false; }
            if (plot.unlocked) { message = "This garden place is already open."; return false; }
            const int price = 55;
            if (Save.gold < price) { message = "You need 55 coins to open another garden place."; return false; }
            Save.gold -= price; plot.unlocked = true; message = "A new garden place is ready for planting."; Notify(); return true;
        }

        public PlantRecord PlotAt(int index) => index >= 0 && index < Save.plots.Count ? Save.plots[index] : null;
        public int GrowthStage(PlantRecord plot)
        {
            if (plot == null || string.IsNullOrEmpty(plot.species)) return 0;
            var definition = GameCatalog.Plants[plot.species];
            return Mathf.Clamp(plot.wateringCount * 4 / Mathf.Max(1, definition.RequiredWaterings + 1), 0, 3);
        }

        public bool BrewPotion(List<string> ingredients, out string message)
        {
            ingredients = ingredients ?? new List<string>();
            if (ingredients.Count < 1 || ingredients.Count > 3) { message = "Choose one to three garden ingredients."; return false; }
            int score = 0, effect = 0;
            string theme = "mixed";
            var kinds = new HashSet<string>();
            foreach (string id in ingredients)
            {
                var item = FindItem(id);
                if (item == null || item.kind != ItemKind.Ingredient.ToString()) { message = "One of those ingredients is no longer in your bag."; return false; }
                score += item.effect + item.power;
                effect += item.effect;
                theme = item.category;
                kinds.Add(item.category);
            }
            foreach (var id in ingredients) Remove(id);
            if (kinds.Count > 1) score += kinds.Count;
            string quality = effect < 0 ? "toxic" : effect == 0 ? "steady" : effect <= 2 ? "gentle" : effect <= 5 ? "bright" : "potent";
            int power = Mathf.Clamp(Mathf.CeilToInt(Mathf.Abs(score) / 2f) + ingredients.Count - 1, 1, 9);
            string title = char.ToUpper(quality[0]) + quality.Substring(1) + " " + char.ToUpper(theme[0]) + theme.Substring(1) + " Potion";
            string category = effect < 0 ? "poison" : effect == 0 ? "neutral" : "restorative";
            AddItem(GameCatalog.MakeDynamic("potion_" + category + "_" + power, title + " · Power " + power, ItemKind.Potion, category, power, effect));
            message = "Brewed " + title + " (power " + power + ").";
            Notify(); return true;
        }

        public bool DistillInk(string ingredientId, out string message)
        {
            var plant = FindItem(ingredientId);
            if (plant == null || plant.kind != ItemKind.Ingredient.ToString()) { message = "Choose a gathered plant ingredient."; return false; }
            Remove(ingredientId);
            AddItem(GameCatalog.MakeDynamic("ink_" + plant.category, plant.name + " ink", ItemKind.Ink, plant.category, plant.power + 1, plant.effect));
            message = "Distilled a vial of " + plant.name + " ink."; Notify(); return true;
        }

        public bool WriteScroll(string inkId, out string message)
        {
            var ink = FindItem(inkId);
            if (ink == null || ink.kind != ItemKind.Ink.ToString()) { message = "Choose a vial of ink first."; return false; }
            if (Count("parchment") < 1) { message = "You need spell parchment."; return false; }
            Remove(inkId); Remove("parchment");
            int power = Mathf.Clamp(ink.power + (Save.reputation >= 30 ? 1 : 0), 1, 8);
            string name = char.ToUpper(ink.category[0]) + ink.category.Substring(1) + " field scroll · Power " + power;
            AddItem(GameCatalog.MakeDynamic("scroll_" + ink.category + "_" + power, name, ItemKind.Scroll, ink.category, power, ink.effect));
            message = "Inscribed " + name + "."; Notify(); return true;
        }

        public bool Enchant(string scrollId, out string message)
        {
            var scroll = FindItem(scrollId);
            if (Save.reputation < 35) { message = "Reach reputation 35 to unlock high craft enchantments."; return false; }
            if (scroll == null || scroll.kind != ItemKind.Scroll.ToString() || scroll.power < 3) { message = "A power 3 or stronger scroll is needed."; return false; }
            if (Count("worn_blade") < 1 || Count("arcane_dust") < 1) { message = "A worn blade and arcane dust are required."; return false; }
            Remove(scrollId); Remove("worn_blade"); Remove("arcane_dust");
            AddItem(GameCatalog.MakeDynamic("enchanted_blade_" + scroll.category, "Enchanted Blade · " + scroll.category, ItemKind.Weapon, "enchanted", scroll.power, scroll.effect));
            message = "The blade took on a " + scroll.category + " enchantment."; Notify(); return true;
        }

        public bool DeliverBest(out string message)
        {
            var order = Save.orders.Where(o => o.accepted).OrderBy(o => o.dueAt).FirstOrDefault(o => CanFill(o));
            if (order == null) { message = "No active request matches anything you have ready."; return false; }
            return DeliverOrder(order.id, out message);
        }

        public bool DeliverOrder(string id, out string message)
        {
            var order = Save.orders.FirstOrDefault(candidate => candidate.id == id && candidate.accepted);
            if (order == null) { message = "That active request is no longer in the box."; return false; }
            if (!CanFill(order)) { message = "You do not have the requested item ready yet."; return false; }
            var item = Save.inventory.First(i => i.kind == order.kind && i.power >= order.minimumPower && (order.kind != "Weapon" || i.category == "enchanted"));
            Remove(item.id);
            double duration = Math.Max(1, order.dueAt - order.issuedAt);
            float remaining = Mathf.Clamp01((float)((order.dueAt - Save.worldMinutes) / duration));
            int bonus = Mathf.RoundToInt(order.reputationReward * 0.75f * remaining);
            int earned = order.reputationReward + bonus;
            Save.gold += order.reward;
            Save.reputation += earned;
            Save.orders.Remove(order);
            message = "Delivered to " + order.client + ". " + order.reward + " coins and " + earned + " reputation" + (bonus > 0 ? " (includes a speedy-delivery bonus)." : ".");
            LogActivity("Delivered " + order.request + " for " + order.client + ". Earned " + order.reward + " coins and " + earned + " reputation.");
            Notify(); return true;
        }

        private bool CanFill(OrderRecord order)
        {
            return order != null && order.accepted && !order.penalized && Save.inventory.Any(i => i.kind == order.kind && i.power >= order.minimumPower && (order.kind != "Weapon" || i.category == "enchanted"));
        }

        public bool Buy(string id, out string message)
        {
            if (!TraderAvailable) { message = "The trader is away. " + NextTraderVisitText + "."; return false; }
            if (!GameCatalog.Items.TryGetValue(id, out var item)) { message = "That item is not on the trader's list."; return false; }
            if (Save.gold < item.Price) { message = "You don't have enough coins."; return false; }
            Save.gold -= item.Price; AddItem(item.Stack(1));
            message = "Bought " + item.Name + ".";
            LogActivity("Bought " + item.Name + " for " + item.Price + " coins.");
            Notify(); return true;
        }

        public bool Sell(string id, out string message)
        {
            if (!TraderAvailable) { message = "The trader is away. " + NextTraderVisitText + "."; return false; }
            var item = FindItem(id);
            if (item == null) { message = "That item is no longer in your bag."; return false; }
            if (item.kind == ItemKind.Seed.ToString() || item.kind == ItemKind.Water.ToString() || item.kind == ItemKind.Weapon.ToString())
            { message = "The trader won't buy that item."; return false; }
            int worth = GameCatalog.Items.TryGetValue(id, out var baseItem) ? baseItem.Price : Mathf.Max(5, item.power * 8);
            if (!Remove(id)) { message = "The item is no longer in your bag."; return false; }
            int payment = Mathf.Max(1, Mathf.RoundToInt(worth * 0.7f));
            Save.gold += payment; message = "Sold " + item.name + " for " + payment + " coins.";
            LogActivity("Sold " + item.name + " for " + payment + " coins.");
            Notify(); return true;
        }

        public void RestUntilMorning()
        {
            double nextMorning = (Math.Floor(Save.worldMinutes / 1440.0) + 1) * 1440.0 + 480.0;
            Save.worldMinutes = nextMorning;
            ProcessExpiredOrders();
            OnMorning();
            UpdateTraderPresence();
            UI?.SetToast("A new morning begins. Plants can be watered again.");
            SaveGame(false);
        }

        public void Notify()
        {
            SaveGame(false);
            StateChanged?.Invoke();
        }

        public void SaveGame(bool force)
        {
            if (Save == null) return;
            if (Player != null) { Save.playerX = Player.transform.position.x; Save.playerY = Player.transform.position.y; }
            Save.indoors = World != null ? World.IsInterior : Save.indoors;
            try
            {
                File.WriteAllText(Path.Combine(Application.persistentDataPath, SaveFile), JsonUtility.ToJson(Save, true));
            }
            catch (Exception e) { Debug.LogWarning("Could not save the game: " + e.Message); }
        }

        public void NewGame()
        {
            StartNewGame(); _observedDay = Day;
            SaveGame(true);
            if (World != null) World.RefreshPlots();
            UI?.SetToast("A fresh garden and shop are yours.");
            Notify();
        }

        private void OnApplicationPause(bool paused) { if (paused) SaveGame(true); }
        private void OnApplicationQuit() { SaveGame(true); }
    }
}
