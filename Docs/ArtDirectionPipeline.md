# Holstin Art Direction Pipeline

## Goal
Ship scenes in three layers:

1. **Layout layer** from `HolstinLevelDesignTemplates` (greybox geometry + gameplay markers)
2. **Mood layer** from the Production Aesthetic Pass (post, atmosphere, practical lights)
3. **Asset layer** from `HolstinArtPack` (placeholder to prefab replacement)

This keeps gameplay iteration fast while allowing high-fidelity art replacement late in production.

## New editor tools

- `Tools/Holstin Level Design Templates/Apply Production Aesthetic Pass`
- `Tools/Holstin/Art Direction/Create Default Art Pack Asset`
- `Tools/Holstin/Art Direction/Seed Default Art Pack From Scene Placeholders`
- `Tools/Holstin/Art Direction/Apply Default Art Pack To Active Scene`
- `Tools/Holstin/Art Direction/Apply Selected Art Pack To Active Scene`
- `Tools/Holstin/Art Direction/Auto-Assign Default Art Pack Prefabs (Selected Folder)`

Scene Builder utilities now include:

- `Apply Production Aesthetic Pass`
- `Apply Default Art Pack`
- `Seed Default Art Pack From Placeholders`
- `Auto-Assign Art Pack Prefabs (Selected Folder)`

## Asset Store import workflow

1. Import your environment/prop packs into the project.
2. Generate or open `Assets/Data/Art/HolstinArtPack.asset`.
3. The tool creates import folders: `Assets/Art/Prefabs` and `Assets/Art/Materials`.
4. Run `Seed Default Art Pack From Scene Placeholders` in a generated level.
5. In the Project view, select the imported prefab folder and run `Auto-Assign Default Art Pack Prefabs (Selected Folder)` for a first-pass mapping.
6. For each mapping entry:
   - assign a prefab
   - tune rotation/position offsets
   - tune scale multiplier
   - enable or disable fit-to-bounds
7. Run `Apply Default Art Pack To Active Scene`.
8. Iterate mappings until scene quality is acceptable.

## Placeholder mapping strategy

- Match by **category + asset tag** for precise control.
- Keep broad fallback mappings by category with empty tag for quick first pass.
- Use multiple assets for one tag by duplicating mapping IDs and toggling one at a time during polish.

## Recommended package targets

- Exterior architecture set
- Interior furniture and clutter set
- Foliage/vegetation set
- Decals and grime set
- Character clothing and accessories set
- Surface material library (wet asphalt, concrete, painted walls, oxidized metal, wood)

## Notes

- Art pack application preserves placeholders by default so the pipeline remains reversible.
- Reapplying a pack replaces prior generated instances linked to each placeholder.
- You can safely keep gameplay scripts on placeholders while replacing only visuals.
