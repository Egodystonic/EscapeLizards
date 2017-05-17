# Escape Lizards Source
Source Code for Escape Lizards ( http://store.steampowered.com/app/508940/Escape_Lizards/ )

Code will not compile in this current state as I have had to remove some parts of it due to:
* Sections being under NDA (e.g. steamworks)
* Sections including third-party code or libraries which do not allow redistribution

Therefore the project and solution files have also been removed- codebase is only for demonstration/learning purposes only.

Also missing are all the assets (art, sound, music, textures etc) for the game itself (for obvious reasons).

Also if/when I update Escape Lizards I won't update this codebase- this will always be just the state of the code as of 17th May 2017.

## Blog

If you're interested, I blog at http://benbowen.blog/

## Quick Overview

A lot of the code is pretty hacky and *not* a good example of quality C#- but I don't have time to filter out the good from the bad- sorry! Consider it a lesson by bad example ;)

### AssetManagement

Just some code for importing .obj models. Was originally written to be extensible for other formats- but we never needed it.

### Core

Basic engine stuff goes in here- e.g. maths lib, pipeline, logging, utils etc.

### CoreNative

Native (C++) counterparts for the core folder

### CSG

Short for **C**onstructive **S**olid **G**eometry. But I never got round to making a proper CSG library- instead this folder just allows creation of basic geometric primitives

### ELEditor

Editor for all the levels and skyboxes in the game. Pretty messy, sorry. I was burned out for a lot of it and just wrote 1000s of lines of procedural code in a couple of files

### Entities

Very poor entity system. Never got around to making anything even close to my original intentions and has a lot of nasty multithreading code in there. Don't use this for learning except learning what **not** to do

### EscapeLizards

Actual game code. Most of it is implemented rather horribly in the MenuCoordinator and GameCoordinator classes. Shameful stuff- but I was the sole developer and pretty worn out already :) Also don't ask me what the difference is between Geometry, Entity, GeometryEntity, LevelGeometryEntity, and so on is (hint: I don't know either anymore)

### LosgapTests

Unit tests for the engine + renderer

### PhysicsNative

Native code to interop with bullet (an open source 3d physics lib)

### Rendering

Where the renderer is implemented

### RenderingNative

Native shim between C# and DirectX
