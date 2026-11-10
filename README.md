# Cottage Magic Shop

A playable Unity 2D starter for a cozy pixel-art gardening and magic-shop game. The outdoor yard, cottage room, player, merchant and growing plant sprites all come from existing open-license pixel-art packs. No generated artwork is included.

## Open and play

1. Install Unity 6.3 LTS (`6000.3.0f1`) with Unity Hub.
2. Clone or download this repository and open the repository folder in Unity Hub.
3. Open `Assets/Scenes/MagicShop.unity` and press **Play**. The starter world is built automatically when the scene starts.

The project uses Unity's built-in 2D physics and IMGUI, so there are no third-party code packages to install.

## Controls

| Input | Action |
| --- | --- |
| WASD or arrow keys | Move |
| E | Use the nearby garden bed, station, trader, order board, door, or cottage clock |
| I | Open or close the satchel |
| J | Open the request ledger |
| Escape | Close a menu |
| Mouse | Choose items and use station menus |

## What is in the starter

- Smoothly accelerated, normalized 8-way movement with a collision body, interpolated physics, four-cardinal idle and walk animation from CC0 character sheets, and a following camera snapped to the pixel grid.
- A cozy yard with a full CC0 cottage sprite and working entrance, a small starting garden, six beds, paths, trees, a trader and an order board; the last three beds cost 55 coins each to open.
- Five seeds with four visible growth stages. Plants need six waterings on separate in-game days; Mooncap and Dreammint need Moonwater, and the carnivorous Bitebloom needs a morsel in its special-item slot.
- Three planting slots for seed, optional soil and an optional magical or feeding item. Rich soil and charms improve harvest output.
- A cottage interior with a potion hearth, scroll desk, enchanting cabinet, and clock. The clock advances to the next morning so plants can be watered again.
- Potion brewing with one to three ingredients, harmful/neutral/helpful outcomes, and strength based on ingredient effects.
- Ingredient-to-ink distilling, parchment scroll writing, and higher-tier weapon enchanting.
- Timed customer requests, a request ledger, faster-delivery reputation bonuses, reputation tiers, and a traveling trader for buying and selling supplies.
- Autosaved garden, inventory, money, reputation, orders, clock and location in Unity's `Application.persistentDataPath`.

An in-game day lasts 30 real-world minutes while the game is running. Orders and the clock continue while station menus are open. The clock is visible in the top-left HUD.

## Continuing development

Runtime systems are organized under `Assets/Scripts`:

- `Data/GameData.cs` contains inventory, plant, order and save records plus the starter item catalog.
- `Core/MagicShopDirector.cs` owns time, saving, planting, crafting, trading and order logic.
- `Player/PlayerMotor.cs` handles movement, animation and nearby interactions.
- `World/WorldAssembler.cs` builds the yard and home; `World/PixelArt.cs` slices the credited source atlases at runtime.
- `UI/MagicShopUI.cs` draws the HUD and station menus.

Game balance and initial stock can be changed in `GameData.cs`. The visual world and station positions can be changed in `WorldAssembler.cs`.

## Asset credits

Open [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) for links and license notes. All included pixel art comes from open-license packs; no art was generated for this project. The Kenney source tiles were only arranged into runtime atlases. The HUD and menus use a CC0 pixel UI pack, and the UI font is Silkscreen under the SIL Open Font License.
