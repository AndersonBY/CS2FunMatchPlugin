# **CS2FunMatchPlugin CS2 Showmatch Entertainment Plugin**
A showmatch like plugin for fun

> Original Repo: [https://github.com/TitaniumLithium/CS2FunMatchPlugin](https://github.com/TitaniumLithium/CS2FunMatchPlugin)

[View in Chinese](README.md)

## Features

- Random fun mode every round
- Enable or disable modes in config
- Custom console-only(.cfg) modes loading
- FunOrder configuration
- Ability to specify the loading order of modes using FunOrder configuration.

## Available Fun/Mode

- Bullet Teleport
- HealTeammates
  - get hurt continually, your teammates can heal you.
- Health Raid
- 1000 Hp
- Infinite Grenades
- Jump Or Die
  - if you don't jump, you will get hurt.
- NoClip On
  - will noclip on for a while.
- Shoot Exchange
- To The Moon
  - low gravity, counterforce enabled (like that in space).
- WNoStop
  - if you don't forward, you will get hurt.
- Change Weapon ONShoot
- Drop Weapon ONShoot
- FootBall Mode
  - Ts aim to take soccer ball to CTspawn, while CTs will stop them.

## How to Install

1. Install metamod & CounterstrikeSharp
see [https://docs.cssharp.dev/docs/guides/getting-started.html](https://docs.cssharp.dev/docs/guides/getting-started.html)

2. Put dll to plugins folder
...\Counter-Strike Global Offensive\game\csgo\addons\counterstrikesharp\plugins\FunMatchPlugin\FunMatchPlugin.dll

## Plugin Console Commands
`fun_lists` Show all available Fun/Mode

`fun_displayhelp` will display help of every mode on round start @css/root required

`!fun_displayhelp` will NOT display help of every mode @css/root required

`fun_random` will load random mode per round automatically @css/root required

`!fun_random` will not load any random mode per round automatically @css/root required

`fun_load [num]` load certain mode by num (num can be found in command "fun_lists") won't affect random load @css/root required

`!fun_load` Unload mode you manually load (num can be found in command "fun_lists") @css/root required

## Config

if you would like to configure something, please put config here
...\Counter-Strike Global Offensive\game\csgo\addons\counterstrikesharp\configs\plugins\FunMatchPlugin

### FunMatchPlugin.json
This file will configure enable or disable modes and how modes work.

If you need to specify the loading order of modes, configure the FunOrder array, like:

```json
"FunOrder": [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13]
```

### CustomModes.json
This file will add new custom console-only mode to this plugin.
You will need .cfg files to add custom console-only mode.

see [https://github.com/AndersonBY/CS2FunMatchPlugin/tree/main/config-example](config-example)