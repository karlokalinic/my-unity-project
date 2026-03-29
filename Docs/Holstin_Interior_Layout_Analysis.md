# Holstin-Inspired Earth Horror Layout Analysis

Date: 2026-03-28

## Source Set
- IGN feature: https://www.ign.com/articles/holstin-5-reasons-to-play-this-phenomenal-pixel-art-survival-horror-game
- Steam store page: https://store.steampowered.com/app/2235430/Holstin/
- SteamDB screenshot metadata: https://steamdb.info/app/2235430/screenshots/

## Visual Worldbuilding Cues To Keep
- Claustrophobic interiors with narrow route lines and hard occlusion.
- A readable mission spine where each room has a clear purpose.
- Layered prop dressing that suggests recent struggle, not decoration.
- Color temperature contrast: cold base lighting with contaminated warm/red accents.
- Exterior hints through doorways and windows to maintain spatial orientation.

## Earth + Callisto Tone Adaptation
- Replace clean sci-fi readability with distressed industrial-residential overlap.
- Keep hard metal modules as retrofitted infrastructure inside civilian shell spaces.
- Add contamination streaks, warning trims, medical residue, and improvised barricades.

## Mission-Driven Spatial Flow (Implemented)
1. Friendly interaction zone: secure Service Key.
2. Service gate choke point: first locked progression gate.
3. Maintenance archive pocket: retrieve Old Key.
4. Front shutter gate: second lock and escape lane payoff.

## Perspective Notes For Camera + FPS Layer
- First-person anchor must remain at eye line and not below clavicle.
- Weapon and arms are presentation-only in the camera space for readability.
- Crosshair should be visible in first-person regardless of weapon data state.
- Add looping ambient bed so first-person does not feel silent between actions.

## Runtime Design Strategy
- Build shell + mission route in code so layout survives scene resets.
- Import external packs as kit pieces, then remap placement/scale/rotation for unique composition.
- Keep interactables and locks local to mission route root for easy tuning.
