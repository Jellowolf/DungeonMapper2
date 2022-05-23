# Dungeon Mapper 2
A WPF based Windows application that can be used as a companion application for playing an old school dungeon crawler
with no auto-mapping. Basically just for using when you don't have graph paper lying around or the will to freehand a map
together.

![DungeonMapperCap](https://user-images.githubusercontent.com/6111060/169908588-82ea6446-b0bb-42e8-a115-be282fdff9c8.png)

## Current State
On a basic level this application is functional, there are still a few bugs and unfinished features that would have polished
it off.

issues/features that still need work
- Movement when the map is larger than the view is rough.
- It could really use a key somewhere for explaining the map symbols.
- Indicators for when something like hallway mode is turned on would be nice.
- There's a bug or two for various usage states laying around, so it could use a little more general QA.

## How To Use
Since there's not an indication of the controls in the application yet, the keyboard controls are as follows:
- Directional Keys - Move the position marker up/down/left/right from current position and marks the tile 
  as having been travelled.
- CTRL + Directional Key - Adds a wall marker to a tile at the direction pressed.
- Shift + Directional Key - Adds a door marker to a tile at the direction pressed.
- H - Hallway mode, automatically generates walls while using the directional keys to move.
- D - Delete all markers on the tile at the current position.
- M - Mark tile as traveled. This occurs on movement by default, but not on subsequent visits to the same tile if cleared.
- T - Creates a black circle to indicate a tile transports the player.
