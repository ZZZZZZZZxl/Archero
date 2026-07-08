# Archero

## 项目简介

本项目是一个基于 Unity 制作的 3D 弓箭传说类 Demo，核心目标是复现弓箭传说的基础战斗体验：玩家通过虚拟摇杆控制角色移动，停止移动后自动索敌并射箭，清空房间敌人后进入下一关。

项目目前已经形成一个可运行的最小玩法闭环，包含移动、转向、索敌、射箭、敌人追击、房间生成、奖励收集、经验升级、强化选择、音效与 UI 等基础系统。

## 项目亮点

- 实现移动端虚拟摇杆，支持根据摇杆推动强度区分快慢走与奔跑。
- 使用 Animator Blend Tree 将 `MoveSpeed` 映射到不同移动动画。
- 参考弓箭传说的站桩攻击逻辑，移动时切换目标，停止后锁定目标持续攻击。
- 支持自动索敌、自动转向、自动射箭和多箭强化。
- 使用 ScriptableObject 管理玩家、武器、敌人、房间、经验、强化等配置。
- 使用对象池管理箭矢、奖励和伤害飘字，减少运行时频繁创建销毁。
- 房间关卡通过配置生成，支持多关卡推进。
- 敌人使用 NavMeshAgent 追击玩家，障碍物通过 prefab 自身配置 Collider/NavMeshObstacle。
- 玩家受击、敌人受击、伤害飘字、屏幕泛红、音效等反馈已接入。

## 已实现功能

### 玩家移动

玩家通过 `VirtualJoystick` 获取输入。

摇杆输入会拆分为两个概念：

- `Intensity`：摇杆真实推动比例。
- `SpeedFactor`：经过 AnimationCurve 映射后的移动速度意图。

`PlayerController` 将 `SpeedFactor` 写入 Animator 的 `MoveSpeed` 参数，由 Blend Tree 控制慢走、快走、奔跑等动画表现。

### 自动索敌与射箭

玩家移动时会根据范围内敌人更新目标。停止移动后，角色会锁定当前目标并持续攻击，直到目标死亡或失效。

射箭逻辑由 `PlayerCombatController` 控制，武器数值由 `PlayerWeaponConfig` 配置。

当前支持：

- 普通箭矢
- 攻击间隔
- 箭矢飞行速度
- 最大射程
- 攻击伤害
- 前方额外箭
- 斜向箭

### 敌人系统

敌人由多个独立组件组成：

- `EnemyController`：敌人基础控制与选中标记。
- `EnemyHealthController`：敌人血量、死亡和经验奖励。
- `EnemyChaseController`：敌人追击玩家。
- `EnemyCombatController`：敌人受击、击退和反馈。

敌人行为参数拆分到配置文件：

- `EnemyHealthBase`
- `EnemyChaseConfig`

### 房间与关卡

房间关卡由 `RoomLevelConfig` 配置。

`RoomLevelSpawner` 负责根据配置生成当前关卡的障碍物和敌人。`RoomController` 负责管理房间状态，当房间内敌人全部死亡后触发房间清空流程。

### 经验与升级

玩家击杀敌人后获得经验。

升级时会：

1. 回复一定比例生命值。
2. 暂停战斗流程。
3. 打开三选一强化面板。
4. 玩家选择强化后继续游戏。

当前强化类型包括：

- 提升攻击速度
- 提升攻击力
- 增加前方箭数量
- 增加斜向箭
- 增加生命上限和当前生命

### 奖励系统

敌人死亡后会掉落奖励。房间清空后，奖励会飞向玩家并被收集。

奖励对象通过对象池复用，避免频繁实例化。

### 陷阱系统

当前尖刺陷阱 `PF_Trap_Spikes` 使用 Trigger Collider。

玩家进入陷阱区域后，每隔 0.5 秒受到 10 点伤害，并复用玩家受击反馈。

### UI 系统

项目当前包含：

- 主菜单界面
- 游戏 HUD
- 玩家血量 UI
- 经验条和等级显示
- 升级强化选择面板
- 暂停菜单
- 游戏结束界面
- 摇杆调试面板

摇杆调试面板用于展示当前摇杆输入比例、转换后的 `MoveSpeed` 以及当前动作状态，方便演示“通过摇杆识别玩家移动意图”的过程。

### 音效系统

项目包含 `AudioManager` 管理背景音乐与音效。

当前音效包括：

- 背景音乐
- UI 点击
- 射箭
- 敌人受击
- 玩家受击

### 对象池

`PoolManager` 是通用对象池，可以支持不同 prefab 的复用。

当前接入对象池的对象包括：

- 箭矢
- 奖励
- 伤害飘字

## 技术实现

### ScriptableObject 配置化

项目将大量数值从脚本中拆出，放入 ScriptableObject 中管理，方便后续调参。

主要配置包括：

- `PlayerSO`
- `PlayerWeaponConfig`
- `PlayerHealthConfig`
- `PlayerExperienceConfig`
- `PlayerUpgradeConfig`
- `EnemyHealthBase`
- `EnemyChaseConfig`
- `RoomLevelConfig`
- `RoomLevelCatalog`
- `CameraFollowConfig`

### 事件解耦

项目使用 `GameEventManager` 处理部分跨系统通信。

例如：

- 敌人或陷阱通过 `PlayerHit` 事件伤害玩家。
- 敌人死亡后广播死亡事件。
- 房间清空后触发门和奖励流程。

这样可以减少敌人、陷阱、玩家血量、房间管理之间的直接依赖。

### Prefab 自持组件

当前项目约定：常规 gameplay prefab 应该自己配置好关键组件。

例如：

- 敌人 prefab 自己带 `NavMeshAgent`、`Rigidbody`、Collider 和敌人脚本。
- 障碍物 prefab 自己带 Collider 和 NavMeshObstacle。
- 陷阱 prefab 自己带 Trigger Collider 和 `TrapDamageController`。

运行时代码主要负责生成和校验，而不是静默安装组件。

## 主要目录

```text
Assets/
  Art/                 美术资源
  Audio/               音频资源与授权记录
  Prefabs/             玩家、敌人、武器、奖励、环境 prefab
  Resources/           运行时加载资源
  Scenes/              主菜单、游戏场景等
  Scripts/             项目脚本
  ScriptableObjects/   游戏配置资源
Docs/                  项目说明文档
Packages/              Unity 包依赖
ProjectSettings/       Unity 项目设置
```

## 运行环境

- Unity：2022.3.62f3c1
- 平台：Windows Editor

## 素材说明

项目使用了若干第三方美术、动画和音频资源，用于课程 Demo 和学习展示。具体授权信息可查看项目内音频授权记录与资源目录说明。
