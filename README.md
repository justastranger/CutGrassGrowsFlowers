# CutGrassGrowsFlowers

Adds an extra pass during `WorldManager.nextDayChanges` so that flowers and mushrooms will grow overnight on cut grass with two chances per tile per night.

This lets you mow sections of grass to use them as wildflower and mushroom farms.

Works *very* well in Spring and Autumn.

## Configuration

 - `numberOfRolls`
      - `int` that controls the number of times per tile that we try spawning on
      - default of `2`
          - large numbers can cause loading the next day to slow
 - `spawnMushrooms`
      - `bool` that controls whether to add mushrooms to the list of acceptable spawns
      - default of `true`

## Installation

1. Install [BepInEx 6.0.0-pre1](https://github.com/BepInEx/BepInEx/releases/tag/v6.0.0-pre.1)
2. Extract `jas.Dinkum.CutGrassGrowsFlowers.dll` to `.\Dinkum\BepInEx\plugins`