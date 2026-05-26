YukiFrameWork 时间倒带器（TimeRewinder）：在固定时间窗口内持续记录游戏状态，并在需要时将对象「倒带」回历史某一时刻。

命名空间：

```csharp
using YukiFrameWork.Rewinder;   // 倒带器核心 API  // RewinderBuffer 环形缓冲区
```

---

## 概念简述

想象一段可以反复倒带的录像带：

1. **记录阶段**：在 `OnRecord` 中持续写入当前状态（位置、血量、动画参数等）。
2. **回放阶段**：调用 `OnPlayBack`，把对象恢复到「N 秒前」保存的状态。
3. **完成阶段**（可选）：调用 `Complete` 停止继续记录，但保留缓冲区数据，仍可多次回放。
4. **释放阶段**（可选）：调用 `Release` 清理资源并从缓存中移除实例。

典型用途包括：死亡回溯、技能预览、关卡调试、解谜机制等。

---

## 核心概念

| 类型 | 说明 |
|---|---|
| `TimeRewinder` | 静态入口，按类型管理倒带器实例（每种类型全局唯一） |
| `IRewinder` | 倒带器接口 |
| `Rewinder` | 倒带器抽象基类，需继承并实现记录 / 回放逻辑 |
| `RewinderMode` | 倒带器运行状态枚举 |
| `RewinderBuffer<TData>` | 基于固定帧率的环形缓冲区，用于存储历史快照 |

---

## 生命周期与状态

倒带器通过 `RewinderMode` 描述当前所处阶段：

| 状态 | 说明 |
|---|---|
| `Idle` | 未初始化，或已调用 `Release` |
| `Update` | 正在记录，每个 FixedUpdate 调用 `OnRecord` |
| `Paused` | 预留状态，当前版本未使用 |
| `Completed` | 已完成倒带：停止记录，缓冲区数据保留，仍可 `PlayBack` |

典型流转：

```
Initialize → Update ──Complete──→ Completed ──Release──→ Idle
                │                      │
                └────── Release ───────┘
```

- `Initialize` 会将状态设为 `Update` 并开始记录。
- `Complete` 仅在 `Update` 状态下生效；重复调用会被忽略。
- `Release` 可在 `Update` 或 `Completed` 状态下调用；重复调用会被忽略。
- 对已 `Complete` 的实例再次 `Initialize`，会重新进入 `Update` 并开始记录（会再次触发 `OnInit`）。

---

## 快速上手

### 第一步：定义快照数据结构

快照可以是任意值类型或引用类型，按你需要记录的内容自行设计：

```csharp
public struct TransformSnapshot
{
    public Vector3 position;
    public Quaternion rotation;
}
```

### 第二步：继承 Rewinder，实现记录与回放

```csharp
using YukiFrameWork.Rewinder;
using UnityEngine;

public class TransformRewinder : Rewinder
{
    private RewinderBuffer<TransformSnapshot> buffer;
    private Transform target;

    protected override void OnInit(float recordTime, params object[] param)
    {
        // param 由 Initialize 传入，可用来绑定目标对象
        target = (Transform)param[0];
        buffer = new RewinderBuffer<TransformSnapshot>(recordTime);
    }

    // 每个 FixedUpdate 自动调用，写入当前状态
    protected override void OnRecord()
    {
        buffer.WriteLastValue(new TransformSnapshot
        {
            position = target.position,
            rotation = target.rotation
        });
    }

    // 回放到 seconds 秒前的状态
    protected override void OnPlayBack(float seconds)
    {
        var snapshot = buffer.ReadValue(seconds);
        target.position = snapshot.position;
        target.rotation = snapshot.rotation;
    }

    // 可选：释放时清理引用
    protected override void OnRelease()
    {
        buffer = null;
        target = null;
    }
}
```

### 第三步：初始化并开始记录

```csharp
// 记录最近 5 秒的状态，并把 transform 传给 OnInit
TimeRewinder.Initialize<TransformRewinder>(5f, transform);
```

初始化完成后，框架会在每个 `FixedUpdate` 自动调用 `OnRecord()`，无需手动驱动。

### 第四步：触发回放

```csharp
// 回放到 2 秒前的状态
TimeRewinder.PlayBack<TransformRewinder>(2f);

// 回放到缓冲区最旧的状态（即 recordTime 秒前，此处为 5 秒前）
TimeRewinder.PlayBack<TransformRewinder>();
```

### 第五步（可选）：完成倒带或释放

```csharp
// 停止继续记录，但保留缓冲区数据，之后仍可多次 PlayBack
TimeRewinder.Complete<TransformRewinder>();

// 回放后一次性清理（适合死亡回溯等场景）
TimeRewinder.PlayBackAndRelease<TransformRewinder>(2f);

// 手动释放：停止记录、触发 OnRelease，并从缓存中移除
TimeRewinder.Release<TransformRewinder>();
```

---

## TimeRewinder 静态 API

| API | 说明 |
|---|---|
| `T Initialize<T>(float recordTime, params object[] param)` | 获取（或创建）倒带器并完成初始化，开始记录 |
| `T As<T>()` | 获取倒带器实例；不存在则自动创建 |
| `bool TryAs<T>(out T rewinder)` | 尝试获取已存在的倒带器，不存在返回 `false` |
| `T PlayBack<T>(float seconds)` | 回放到指定秒数前的状态 |
| `T PlayBack<T>()` | 回放到 `RecordTime` 秒前的最旧状态 |
| `T Complete<T>()` | 完成倒带：停止记录，保留缓冲区数据 |
| `bool TryComplete<T>(out T rewinder)` | 尝试完成倒带；实例不存在返回 `false` |
| `bool Release<T>()` | 释放倒带器并从缓存移除；实例不存在返回 `false` |
| `T PlayBackAndRelease<T>(float seconds)` | 回放到指定时刻后自动释放 |
| `T PlayBackAndRelease<T>()` | 回放到最旧状态后自动释放 |
| `T BackFlow<T>(float seconds)` | **已过时**，请改用 `PlayBack` |

---

## IRewinder 接口

| 成员 | 说明 |
|---|---|
| `float RecordTime { get; }` | 当前倒带器的记录窗口长度（秒） |
| `RewinderMode Mode { get; }` | 当前运行状态 |
| `void Initialize(float recordTime, params object[] param)` | 初始化并开始记录 |
| `void Complete()` | 完成倒带：停止记录，保留数据 |
| `void PlayBack(float seconds)` | 回放到指定秒数前的状态 |
| `void Release()` | 释放资源，之后需重新 `Initialize` |
| `T As<T>()` | 类型转换 |

---

## Rewinder 生命周期回调

| 成员 | 说明 |
|---|---|
| `float RecordTime { get; }` | 记录窗口长度（秒） |
| `RewinderMode Mode { get; }` | 当前运行状态 |
| `protected abstract void OnInit(float recordTime, params object[] param)` | 初始化回调，创建缓冲区、绑定目标等 |
| `protected abstract void OnRecord()` | 每个 FixedUpdate 调用（仅 `Update` 状态），写入当前快照 |
| `protected abstract void OnPlayBack(float seconds)` | 回放回调，读取历史快照并还原状态 |
| `protected virtual void OnComplete()` | 完成倒带时回调，此时已停止记录但数据仍可用 |
| `protected virtual void OnRelease()` | 释放时回调，用于清理缓冲区、解绑目标等 |

---

## RewinderBuffer 环形缓冲区 API

`RewinderBuffer<TData>` 按固定帧率分配容量，内部以环形数组循环覆盖最旧数据。

| API | 说明                                                 |
|---|----------------------------------------------------|
| `RewinderBuffer(float recordTime)` | 根据 `recordTime` 与 `Time.fixedDeltaTime` 计算容量并创建缓冲区 |
| `float RecordTime { get; }` | 记录窗口长度                                             |
| `void WriteLastValue(TData value)` | 写入最新一帧数据                                           |
| `bool TryReadLastValue(out TData value)` | 尝试读取最新一帧，缓冲区为空时返回 `false`                          |
| `TData ReadValue(float seconds)` | 读取 `seconds` 秒的历史数据                                |
| `bool ReadValueDifferent(float seconds,out TData value)` | 读取 `seconds` 秒的历史数据,如果读取值与上一次读取相等,则返回False         |

容量计算公式：

```
capacity = (int)(RecordTime / Time.fixedDeltaTime)
```

---

## 完整使用示例

### 持续回放预览

```csharp
using YukiFrameWork.Rewinder;
using UnityEngine;

public class PlayerTimeRewind : MonoBehaviour
{
    private void Start()
    {
        // 初始化：记录玩家最近 3 秒的状态
        TimeRewinder.Initialize<PlayerRewinder>(3f, transform);
    }

    private void Update()
    {
        // 按住 R 键，持续回放到 1 秒前
        if (Input.GetKey(KeyCode.R))
        {
            TimeRewinder.PlayBack<PlayerRewinder>(1f);
        }
    }
}
```

### 死亡回溯（回放后自动释放）

```csharp
public class PlayerDeathHandler : MonoBehaviour
{
    private void OnPlayerDeath()
    {
        // 冻结快照，回放到 2 秒前并清理倒带器
        TimeRewinder.Complete<PlayerRewinder>();
        TimeRewinder.PlayBackAndRelease<PlayerRewinder>(2f);
    }
}
```

### Rewinder 实现

```csharp
public struct PlayerSnapshot
{
    public Vector3 position;
    public Vector3 velocity;
}

public class PlayerRewinder : Rewinder
{
    private RewinderBuffer<PlayerSnapshot> buffer;
    private Transform player;
    private Rigidbody rb;

    protected override void OnInit(float recordTime, params object[] param)
    {
        player = (Transform)param[0];
        rb = player.GetComponent<Rigidbody>();
        buffer = new RewinderBuffer<PlayerSnapshot>(recordTime);
    }

    protected override void OnRecord()
    {
        buffer.WriteLastValue(new PlayerSnapshot
        {
            position = player.position,
            velocity = rb != null ? rb.velocity : Vector3.zero
        });
    }

    protected override void OnPlayBack(float seconds)
    {
        var snap = buffer.ReadValue(seconds);
        player.position = snap.position;
        if (rb != null)
        {
            rb.velocity = snap.velocity;
        }
    }

    protected override void OnComplete()
    {
        // 可选：完成倒带时的额外逻辑，例如暂停玩家输入
    }

    protected override void OnRelease()
    {
        buffer = null;
        player = null;
        rb = null;
    }
}
```

---

## 注意事项

1. **FixedUpdate 驱动**：记录逻辑绑定在 `MonoHelper.FixedUpdate`，与物理帧同步，不适合记录每帧变化剧烈但不走物理的对象（除非自行在 `OnRecord` 中处理）。
2. **一种类型一个实例**：`TimeRewinder` 按泛型类型缓存实例。若需同时倒带多个对象，请为不同对象创建不同的 `Rewinder` 子类，或在 `OnInit` 中管理多个目标。
3. **recordTime 必须大于 0**：传入 `<= 0` 会在初始化时抛出异常。
4. **先 Initialize 再 PlayBack**：未初始化就调用 `PlayBack`，`TryAs` 会失败并返回 `default`。
5. **快照尽量轻量**：缓冲区按帧存储，记录窗口越长、快照越大，内存占用越高。只记录必要字段。
6. **默认 PlayBack 不会暂停记录**：在 `Update` 状态下调用 `PlayBack` 后，`OnRecord` 仍会继续写入。若需要冻结快照，请调用 `Complete`。
7. **Release 与 PlayBackAndRelease**：`Release` 会触发 `OnRelease` 并从 `TimeRewinder` 缓存中移除实例；`PlayBackAndRelease` 等价于先 `PlayBack` 再 `Release`。释放后再次使用需重新 `Initialize`。
8. **Complete 与 Release 的区别**：`Complete` 保留实例与缓冲区数据，适合「暂停录像、反复倒带」；`Release` 彻底清理，适合「一次性回溯」或对象销毁时。

---

## 版本

当前模块版本：**1.1**（见 `Version.txt`）

### 更新记录

| 版本 | 新增内容 |
|---|---|
| **1.1** | `Complete` / `Release` / `PlayBackAndRelease` 静态 API；`TryComplete`；`RewinderMode` 状态机；`OnComplete` / `OnRelease` 生命周期；`Rewinder` 实例方法 |
| **1.0** | 基础记录与回放：`Initialize`、`PlayBack`、`RewinderBuffer` |
