# UltraTag
*COMP 476 - Advanced Game Development | Winter 2026 | Concordia University*
> Rock, Paper, Scissors, Lizard, Spock (Battle Arena)
---
> Repo: https://github.com/wokidoo/UltraTag---COMP-476-Quiz/tree/main
## Project Overview

This project is a Unity 3D implementation of an extended Rock, Paper, Scissors game featuring five factions: Rocks, Papers, Scissors, Lizards, and Spocks. Each faction has 10 units that autonomously seek, flee, and battle opposing factions using fuzzy logic-driven Group AI and a state machine-based Individual AI. The game ends when all units of any one faction are eliminated.

---

## How to Run (Unity Editor)

1. Open the project in Unity 2022.3 or later.
2. Open the 'Arena' scene and press Play.
3. Controls are keyboard and mouse only — no gamepad required.

---

## Controls

| Input | Action |
|---|---|
| WASD | Move camera |
| Q / E | Move camera down / up |
| Right-click + drag | Look around |
| Scroll wheel | Zoom in / out |
| Left-click (ground) | Place a unit (pre-game) |
| Right-click (unit) | Remove a friendly unit (pre-game) |

---

## Game Rules

The five factions interact according to the standard RPSLS rules:

- Rock crushes Scissors and Lizard
- Paper covers Rock and disproves Spock
- Scissors cuts Paper and decapitates Lizard
- Lizard poisons Spock and eats Paper
- Spock smashes Scissors and vaporizes Rock

A unit is eliminated when it physically collides with an enemy unit. The game ends when all units of any faction are destroyed - that faction loses.

---

## AI Architecture

### Group AI (`GroupAI.cs`)

Each faction has a single GroupAI object that runs a fuzzy logic system every frame to compute an `aggressiveness` value — the base movement speed shared by all units in that faction.

**Inputs:**
- Number of friendly units
- Number of enemy units
- Number of target units

**Fuzzification:** Each input is mapped to a `High` and `Low` degree using linear ramp membership functions. Crossover points are read from the assignment figures:

**Rule evaluation:** Nine rules combine AND (`min`), OR (`max`), and NOT (`1 - x`) operators and fire into three output buckets:

| Output | Crisp Speed |
|---|---|
| Aggressive | 10 m/update |
| Average Speed | 6 m/update |
| Move Calmly | 2 m/update |

**Defuzzification:** Normalized weighted blend:
```
aggressiveness = (aggressive*10 + average*6 + calm*2) / (aggressive + average + calm)
```

---

### Individual AI (`IndividualAI.cs`)

Each unit runs a three-state machine every frame. Base speed is set by the faction's `aggressiveness` value.

**Searching**
- Scans within 5 m for enemies.
- If an enemy is found, transitions to Fleeing.
- Otherwise finds the nearest target, transitions to Seeking.

**Seeking**
- Chases the committed target.
- Must first get within 7.5 m of the target before the abandonment check activates.
- If the target moves beyond 7.5 m after being reached, drops it and finds the next nearest.
- If an enemy enters within 5 m, transitions to Fleeing.

**Fleeing**
- Runs from the nearest enemy.
- Continues fleeing as long as an enemy is within 7.5 m.
- If a jump pad is within 5 m, steers toward it with a 2x force multiplier as an escape route.
- Returns to Searching once the threat clears.

**Speed Boost**
- Triggered when an enemy comes within 5 m.
- Adds +3 to current speed for 3 seconds.
- 3-second cooldown before the boost can trigger again.

**Movement**
- Steering forces accumulate per frame as a normalised direction vector.
- Applied via `Vector3.Lerp` against the Rigidbody velocity for smooth acceleration and deceleration.
- Gravity is preserved at all times.

---

## Game Design Features

- **Arena boundary (75 m radius):** Units are kept within the arena boundary as per the assignment requirements.
- **Obstacles:** At least 10 obstacles of varying shapes are placed in the arena. Units steer away using a repulsion force. Collision with an obstacle eliminates the unit.
- **Jump pads:** 10 jump pads are placed in the arena. On contact, units are launched forward and upward. Fleeing units actively steer toward nearby jump pads as an escape tactic. Units remain within the 75 m boundary after jumping.
- **Unit placement:** The player left-clicks the ground to place units one at a time before the game starts. Right-clicking an existing friendly unit removes it for repositioning.
- **Faction spawning:** After the player places all 10 units, non-player factions spawn in waves of one unit per faction every 0.25 seconds to stagger initial collisions.

---

## Differences from Assignment Specification

### Fuzzy Logic Rule Outputs
The assignment provides 7 base rules with suggested output categories. Two additional rules (Rule 8 and Rule 9) were added as permitted by the assignment. The output bucket assignments were adjusted from the suggested defaults to produce more interesting game dynamics: rules that fire when all unit counts are high are mapped to **calm** (slow cautious early-game behaviour) and rules that fire when counts are low are mapped to **aggressive** (fast desperate late-game behaviour).

### Jump Pad Behaviour
The assignment states units can use jump pads to escape or follow targets. In this implementation, only fleeing units actively seek nearby jump pads. Seeking units do not specifically target jump pads but will still trigger them if their path crosses one. Jump pads use `OnCollisionEnter` rather than `OnTriggerEnter`, so the unit's collider must physically land on the pad surface.

### Obstacle Elimination
Units are destroyed on obstacle contact. In addition to this,units actively avoid steering into Obstacles but can be pushed in if pressured by nearby enemies.

---
