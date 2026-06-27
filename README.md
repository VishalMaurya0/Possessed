# 👻 POSSESSED
### *A Co-op Multiplayer Horror Survival Escape Game*

<div align="center">

[![Unity](https://img.shields.io/badge/Engine-Unity_2025.x_HDRP-black?logo=unity)](https://unity.com/)
[![Netcode](https://img.shields.io/badge/Networking-Netcode_for_GameObjects-blue)](https://docs-multiplayer.unity3d.com/)
[![Status](https://img.shields.io/badge/Status-In_Development-orange)]()
[![Genre](https://img.shields.io/badge/Genre-Horror_|_Co--op_|_Survival-red)]()
[![Players](https://img.shields.io/badge/Players-1–4_Online-purple)]()

> *"It's not what's haunting the house… it's what's already inside you."* 👁️

</div>

---

## 🩸 Overview

**Possessed** is a **first-person co-op horror survival game** where 1–4 players explore a haunted house and forest, racing to perform the **3 correct rituals** out of 8 possible — before the ghost possesses you all.

Teamwork, nerves, and communication are your only weapons. There is no HUD, no waypoints, and no text chat — only voice, instinct, and each other.

---

## 🧠 At a Glance

| | |
|:--|:--|
| **Genre** | Co-op Horror / Survival / Puzzle |
| **Perspective** | First-Person |
| **Players** | 1–4 (Online Co-op) |
| **Engine** | Unity 2025.x HDRP |
| **Networking** | Unity Netcode for GameObjects |
| **Started** | December 2024 |
| **Status** | ⚙️ In Active Development |

---

## 🕯️ Core Gameplay Loop

```
Explore the haunted map
    → Find 8 ritual sites scattered through house & forest
        → Investigate clues to identify the 3 correct rituals
            → Gather required items & perform them
                → Survive ghost hunts and doll attacks
                    → Exorcise the ghost — or die trying
```

1. **Explore** the haunted house and forest for clues and ritual items.
2. **Investigate** 8 ritual sites and deduce the 3 correct ones using clue items.
3. **Gather items** through exploration and mini-tasks rewarding key materials.
4. **Perform rituals** — wrong choices have deadly consequences.
5. **Manage your Fear Meter** — panic leads to possession.
6. **Survive hunts** — the ghost has two dangerous states; dolls patrol in between.
7. **Communicate by voice only** — no map markers, no text, no HUD.

---

## 💀 Unique Selling Points

| Feature | Description |
|:--|:--|
| 🩸 **Ritual Deduction System** | 8 rituals spawn on the map; only 3 are correct. Wrong ones can kill you. Use clue items to narrow them down. |
| 🧠 **Fear Meter** | Progressive fear warps your vision, control, and sanity — panic enough and you risk possession. |
| 👁️ **Doll Mechanics** | Two doll types with inverse behaviour — one attacks when unseen, one attacks when watched. Both look identical. |
| 🔥 **Safe Point** | Deploy a team-safe zone for temporary protection. Recharges over time. |
| 🕯️ **Possession System** | Get possessed and turn against your own team. Teammates must spot the ghost to stop it. |
| 🎙️ **Voice Chat Only** | No text. No HUD markers. True immersion. |
| ⚙️ **Procedural Intensity** | Ghost behaviour, hunt timing, and patrol patterns differ every round. |

---

## 🎯 Objectives & Ritual System

8 rituals are randomly distributed across the map each match. Your team must **identify and perform exactly 3 correct rituals** to exorcise the ghost.

**How to find the correct rituals:**
- Collect **clue items** (Tape Recorder, Pen, Pendulum, Book) scattered around the map.
- Each clue item reveals a **grid of ritual × instrument readings**:
  - 🟫 **Gray** — No effect observed for that instrument on that ritual.
  - 🟨 **Yellow** — Some effect observed; this ritual *may* be relevant.
  - 🟥 **Red** — Confirmed: this ritual is strong against the ghost.
- Cross-reference clues to identify the 3 correct rituals and add them to your selection.

> Each clue item reveals 2 out of 3 correct rituals. Coordinate with your team to piece together the full picture.

---

## 🕯️ Rituals

Each ritual requires specific **items in specific quantities and states**. Example:

**Ritual 5 — Feather Fire**
| Item | Quantity | Required State |
|:--|:--:|:--|
| Feather | 1 | Normal |
| Blood Bottle | 10 | Normal |
| Pure Powder | 5 | Normal |

Rituals are performed at their designated sites on the map. Choosing wrong is costly — choose carefully.

---

## 👾 Enemies

### 👻 Ghost

The primary threat. Operates in two states:

**Normal State**
- Roams and searches for players.
- Begins **possessing** any player it spots — the victim must stay still while teammates find and look at the ghost to break the possession.

**Aggressive State (Hunt)**
- Extremely dangerous. The ghost follows your position *and* your sounds.
- Contact means instant death — your only option is to **hide**.
- Aggression ends after a set duration.

---

### 🪆 Dolls

Several dolls of two types are frozen throughout the map. **They look identical** — you can't tell them apart.

**Normal Doll**
- Activates when it sees a player.
- Walks toward its target when **no one is watching**.
- Attacks if it gets too close.

**Reversed Doll**
- Also activates on sight.
- Walks toward its target **only when someone IS watching**.
- Attacks if it gets too close.

> There is no visual difference between the two types. Good luck.

---

## 📦 Items

### Item Stats

| Item | Icon | Container | Craftable From | State / Uses | Stack |
|:--|:--:|:--:|:--|:--:|:--:|
| Burning Wood | 🪵 | No | Clothed Wood + Match | State 2 | 3 |
| Pin | 📍 | No | — | State 0 | 20 |
| Match | 🔥 | Yes | — | 7 Uses | 2 |
| Blood Bottle | 🧪 | Yes | — | 1 Use | 5 |
| Mirror | 🪞 | No | — | State 0 | 15 |

> Some items are **containers** (they hold uses/charges). Some can be **crafted** by combining other items.

---

## ⚙️ Tasks

Tasks randomly spawn around the map. Completing them rewards you with items needed for rituals.

| Task | How It Works |
|:--|:--|
| **Coins Task** | Click a cup to lock it — a shuffle happens. Track your cup and click it last to earn a coin. |
| **Pure Powder Task** | Memorize a colour sequence shown on screen, then reproduce it. Repeat 10 times. |
| **Candle Task** | A sequence of candles lights up — click them in the same order while they're completing. |

---

## 🔧 Development Progress

| System | Status | Notes |
|:--|:--:|:--|
| Multiplayer | ✅ Done | Netcode + Matchmaker integrated |
| Ghost AI | ⚙️ In Progress | State machine + possession logic |
| Doll AI | ⚙️ In Progress | Normal and Reversed doll behaviour |
| Fear Meter | ⚙️ In Progress | Progressive visual/control effects |
| Safe Point | ⚙️ In Progress | Active with recharge system |
| Clue System | ⚙️ In Progress | Grid-based instrument/ritual readings |
| Ritual System | 🔜 Planned | Effect combination + validation logic |
| Possession Maze | 🔜 Planned | Design phase |
| Collectibles | ⚙️ In Progress | Ouija Board, Mirror, Box |
| Editor Tools | ⚙️ In Progress | Custom Grid Level Builder |
| Tasks | ✅ Done | Coins, Pure Powder, Candle tasks implemented |

📜 [Game Design Document (GDD)](https://docs.google.com/document/d/1nJwiVVwhEpSuE3gG2uOO_GpviitgWdjF0BvVNUAQcPY/edit?usp=sharing)
🧾 [Development To-Do List](https://docs.google.com/document/d/1x-UWTaFbML72AKWT8B6chQJjWDfdc1s4Kgs-lAxssGQ/edit?usp=sharing)

---

## 🧩 Technical Stack

| Layer | Technology |
|:--|:--|
| Engine | Unity 2025.x HDRP |
| Networking | Unity Netcode for GameObjects |
| Matchmaking | Unity Multiplayer Center + Matchmaker |
| AI | NavMesh + State-based Logic |
| Assets | Quixel Megascans + Custom Assets |
| Level Design | Custom Grid Level Builder (Editor Extension) |

---

## 🎮 Controls

| Action | Key |
|:--|:--:|
| Move | `WASD` |
| Look | `Mouse` |
| Interact | `E` |
| Drop Item | `G` |
| Use Item | `Right Click` |
| Deploy Safe Point | `F` |
| Voice Chat | `V` |

---

## 🏗️ Building & Running

> ⚠️ For developers and testers only.

1. Clone the repository.
2. Open in **Unity 2025.x**.
3. Ensure **Netcode for GameObjects** and **Multiplayer SDK** are installed via Package Manager.
4. Open `Scenes/Development/House.unity`.
5. Press **Play** → Select **Host** or **Client** in the Multiplayer Center panel.

---

## 🧰 Level Design Tools

A custom **Grid Level Builder** editor extension is included:

- 🧱 Place prefabs on a snapped grid
- 🎨 Brush, Fill, and Line placement tools
- 🧽 Erase or reposition existing prefabs
- 🔄 Full Undo/Redo + Save/Load support
- 🎲 Prefab groups for randomized placement

---

## 👥 Credits

| Role | Name |
|:--|:--|
| Lead Developer & Designer | *Vishal Maurya* |
| Development Start | December 2024 |
| Engine | Unity URP |
| Special Thanks | Community testers and early feedback players |

---

## 📧 Contact

For collaboration, feedback, or playtesting:
📮 **vm433848@gmail.com**

---

<div align="center">

*👻 POSSESSED — Survive together. Or fall apart.*

</div>
