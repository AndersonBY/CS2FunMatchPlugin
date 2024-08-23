# **CS2类表演赛娱乐插件**

一个娱乐性质的表演赛插件

> Original Repo: [https://github.com/TitaniumLithium/CS2FunMatchPlugin](https://github.com/TitaniumLithium/CS2FunMatchPlugin)

[View in English](README_EN.md)

## Features

- 每回合随机一种娱乐模式
- 可配置各个模式是否启用 配置模式参数
- 可自定义仅控制台命令的模式
- 可使用FunOrder配置来指定模式的加载顺序。

## 当前可使用的娱乐模式

- 瞬移子弹
- 攻击治疗队友
  - 持续受到伤害 只有队友能帮你活下去
- 攻击吸血
- 1000血量
- 无限手雷
- 地板烫脚
  - 不跳就会收到伤害
- 启用飞行穿墙
  - 每隔一段时间切换一次状态 包点无法起飞
- 换位子弹
- 月球模式
  - 低重力 射击会有一定反作用力
- 按住 W 不松手
  - 不按住 w 则会收到伤害
- 射击换枪
- 射击丢枪
- 足球模式
  - T 需要将足球踢进 CT 出生点

## 如何安装

1.安装 metamod 以及 CounterstrikeSharp 
see [https://docs.cssharp.dev/docs/guides/getting-started.html](https://docs.cssharp.dev/docs/guides/getting-started.html)

2.将插件放于指定目录
...\Counter-Strike Global Offensive\game\csgo\addons\counterstrikesharp\plugins\FunMatchPlugin\FunMatchPlugin.dll

## 插件控制台相关指令
`fun_lists` 显示所有当前支持的娱乐模式

`fun_displayhelp` 每回合开始播报模式帮助信息 @css/root required

`!fun_displayhelp` 停止播报模式帮助信息 @css/root required

`fun_random` 启用每回合随机模式 @css/root required

`!fun_random` 停用每回合随机模式 @css/root required

`fun_load [num]` 手动加载模式 对应的数字可以在"funlists"指令查到 与随机模式独立 @css/root required

`!fun_load` 卸载手动加载的模式 @css/root required

## Config 插件配置

修改模式默认配置 请将配置文件放在此文件夹
...\Counter-Strike Global Offensive\game\csgo\addons\counterstrikesharp\configs\plugins\FunMatchPlugin

### FunMatchPlugin.json
该文件会配置是否启用特定模式，特定模式的参数。

如果需要指定模式加载顺序，请在 FunOrder 中配置编号数组，如

```json
"FunOrder": [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13]
```

### CustomModes.json
该文件会配置仅控制台自定义新模式并加入游戏中
该模式基于.cfg文件运行，请将你需要的指令放在指定的.cfg文件中

see [https://github.com/AndersonBY/CS2FunMatchPlugin/tree/main/config-example](config-example)