# Cottage Magic Shop

A playable Unity 2D starter for a cozy pixel-art gardening and magic-shop game. The outdoor yard, cottage room, player, merchant and growing plant sprites all come from existing open-license pixel-art packs. No generated artwork is included.

## Open and play

1. Install Unity 6.3 LTS (`6000.3.0f1`) with Unity Hub.
2. Clone or download this repository and open the repository folder in Unity Hub.
3. Open `Assets/Scenes/MagicShop.unity` and press **Play**. The starter world is built automatically when the scene starts.

The project uses Unity's built-in Tilemap, 2D physics and IMGUI modules, so there are no third-party code packages to install. Ground and paths are separate tilemap layers, while characters, furniture and interactable objects use y-sorted sprites.

## Controls

| Input | Action |
| --- | --- |
| WASD or arrow keys | Move |
| E | Use the nearby garden bed, station, visiting trader, trade box, door, trail sign, or cottage clock |
| I | Open or close the satchel |
| J | Open the trade and request box from anywhere |
| Escape | Close a menu |
| Mouse | Choose items and use station menus |

## What is in the starter

- Smoothly accelerated, normalized 8-way movement with a collision body, interpolated physics, four-cardinal idle and walk animation from CC0 character sheets, and a following camera snapped to the pixel grid.
- A cottage set clear of the HUD, with a furnished room, working entrance and a garden enclosed by forest. A sand path runs from the door to the permanent trade and request chest, then ends at a collidable fence and sign. Six true soil beds flank the path; the last three cost 55 coins each to open.
- Five seeds with four visible growth stages. Plants need six waterings on separate in-game days; Mooncap and Dreammint need Moonwater, and the carnivorous Bitebloom needs a morsel in its special-item slot.
- Three planting slots for seed, optional soil and an optional magical or feeding item. Rich soil and charms improve harvest output.
- A furnished cottage interior with a fireplace, wall clock, window, storage chest, potted plant and two work tables for potion, scroll and enchantment crafting. The clock advances to the next morning so plants can be watered again.
- Potion brewing with one to three ingredients, harmful/neutral/helpful outcomes, and strength based on ingredient effects.
- Ingredient-to-ink distilling, parchment scroll writing, and higher-tier weapon enchanting.
- Customer requests arrive in the permanent trade box. Accepting starts the deadline; declining has no penalty. Deliveries, missed commissions, request decisions, purchases and sales remain in the box's history. Completed requests pay coins and reputation, with faster-delivery bonuses.
- The traveling trader visits every third in-game day, from 10:00am to 5:00pm. The merchant and stall are hidden while away, and the box shows the next visit.
- Autosaved garden, inventory, money, reputation, requests, trade history, clock and location in Unity's `Application.persistentDataPath`. Existing version 1 saves are migrated with their current requests preserved as accepted commissions.

An in-game day lasts 30 real-world minutes while the game is running. Orders and the clock continue while station menus are open. The HUD shows request counts and the trader's schedule. Art sheets are cropped to their verified source bounds at runtime, and the camera and textures use point sampling with pixel-aligned scale.

## Reference guides

The yard layout and gardening flow were informed by [2D-Game-Farming-RPG](https://github.com/Raphaelsl/2D-Game-Farming-RPG-) and [2D-Farming-Game](https://github.com/wkoziel/2D-Farming-Game). Unity's [Happy Harvest techniques](https://unity.com/blog/games/happy-harvest-demo-latest-2d-techniques) informed the separate ground and path tilemaps and layered environment. These projects are design references; no code or art was copied from them.

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
