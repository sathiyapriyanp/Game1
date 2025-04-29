# Game1
# 🔶 Unity Card Match Game

A simple and modular **Card Matching Memory Game** built with Unity. This project demonstrates clean code practices, smooth animations, UI management, and dynamic grid configuration for casual puzzle gameplay.

## 🎮 Features

- Flip card animation using scale-based transition
- Configurable grid sizes (2x2, 2x3, 3x4, etc.)
- Dynamic sprite pairing and randomization
- Score and combo tracking system
- Timer-based win/loss logic
- Sound effects for flip, match, and mismatch
- UI panels for menu, win, lose, and in-game HUD

---

## 🚀 Getting Started

### Requirements

- Unity 2020.3 LTS or later
- TextMeshPro (import via Package Manager if needed)
- Unity UI system (built-in)

### Installation

1. Clone or download the repository
2. Open it with Unity Hub
3. Import required UI assets or sprites into `Assets/Sprites/`
4. Assign your sprite list via the `CardManager` inspector

---

## 🧩 How It Works

### Core Scripts

- **CardManager.cs**  
  Handles all gameplay logic, including:
  - Grid generation
  - Card matching
  - Score updates
  - Timer & win/loss conditions

- **Card.cs**  
  Attached to each card prefab. Handles:
  - Flip animation
  - Sprite switching
  - Click interaction
  - Match check

### SoundManager.cs (Optional)

You'll need a `SoundManager` singleton with public `AudioClip` references like:
- `flipSound`
- `matchSound`
- `mismatchSound`

Add `PlaySound(AudioClip clip)` method to play effects globally.

---

## 🛠 Customization

- **Grid Sizes:**  
  You can call `CardManager.StartGame(rows, columns)` to define your own layout.

- **Scoring Rules:**  
  Modify `matchScore` and `mismatchPenalty` in `CardManager`.

- **Card Prefab:**  
  Use your own UI prefab, but make sure it has:
  - `Image` component for flipping
  - `Button` component for interaction

- **Sprite Pool:**  
  Add your sprite pairs to the `spriteList` in the inspector.

---

## 📁 Folder Structure
Assets/ ├── Scripts/ │ ├── Card.cs │ └── CardManager.cs ├── Prefabs/ │ └── Card.prefab ├── Sprites/ │ └── [Your images here] ├── UI/ │ └── Panels, Buttons, Text elements


 
