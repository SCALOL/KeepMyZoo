# Memory Sound Game — Unity 6 Wiring Guide

> All scripts are production-ready. Follow every step in order.
> Scripts reference each other by **singleton** (`Instance`), so
> the GameObject names must match exactly.

---

## 0. Project Setup (do this first)

| Step | Where | Action |
|------|-------|--------|
| 0-1 | Package Manager | Install **Input System** (`com.unity.inputsystem`) |
| 0-2 | Package Manager | Install **TextMeshPro** (import TMP Essentials when prompted) |
| 0-3 | Project Settings → Player → **Other Settings** | Set **Active Input Handling** → `Both` (or `New Input System`) |
| 0-4 | Build Settings | Add **Scene 0** (MainMenu) at index 0, **Scene 1** (Gameplay) at index 1 |
| 0-5 | Project Settings → Player → Resolution | Set **Default Orientation** → `Portrait` |

---

## 1. Audio Mixer Setup

1. Right-click in **Project** → `Create → Audio Mixer`. Name it `MasterMixer`.
2. In the Mixer window, create two **child groups** under Master:
   - `Music`
   - `SFX`
3. Select the **Music** group → right-click its **Volume** knob → **Expose to script**.
4. Select the **SFX** group → expose its **Volume** too.
5. In the **Exposed Parameters** panel (top-right of Mixer window), rename them **exactly**:
   - `MusicVolume`
   - `SFXVolume`

---

## 2. Audio Clips Setup

Create a folder `Assets/Audio/` and import:

| File | Assign to |
|------|-----------|
| `sound_up.wav` | `gameSoundClips[0]` (Sound ID 0 — Up) |
| `sound_down.wav` | `gameSoundClips[1]` (Sound ID 1 — Down) |
| `sound_left.wav` | `gameSoundClips[2]` (Sound ID 2 — Left) |
| `sound_right.wav` | `gameSoundClips[3]` (Sound ID 3 — Right) |
| `error.wav` | `errorClip` |
| `tick.wav` | `tickClip` |
| `win.wav` | `winClip` |
| `music.mp3` | `musicClip` |

**Audio Import settings per clip:**

| Clip type | Load Type | Compression |
|-----------|-----------|-------------|
| All 4 game sounds + error + tick + win | **Decompress on Load** | PCM |
| `music.mp3` | **Compressed in Memory** | Vorbis |

---

## 3. Scene 0 — MainMenu

### 3-1. Hierarchy structure

```
MainMenu (scene root)
├── [AudioManager]          ← persists to Scene 1
│   ├── SFX AudioSource
│   └── Music AudioSource
├── MainMenuManager
├── Canvas (Screen Space – Overlay, Scale With Screen Size 1080×1920)
│   ├── MainMenuCanvas
│   │   ├── StartButton
│   │   ├── SettingsButton
│   │   └── ExitButton
│   └── SettingsCanvas (inactive by default)
│       ├── MusicSlider
│       ├── SFXSlider
│       └── BlindModeToggle
```

### 3-2. AudioManager GameObject

1. Create empty GameObject → name it **`AudioManager`** (exact spelling).
2. Add component → `AudioManager.cs`.
3. Add component → `AppInit.cs`.
4. Create two child GameObjects: **`SFX`** and **`Music`**.
   - Add an `AudioSource` to each. On the Music source: tick **Play On Awake = false** (the script controls it). On SFX: **Play On Awake = false**.
5. In AudioManager Inspector:
   - **Master Mixer** → drag `MasterMixer` asset.
   - **Sfx Source** → drag `SFX` child.
   - **Music Source** → drag `Music` child.
   - Fill all **Game Sound Clips** (4 slots) and special clips.

### 3-3. MainMenuManager GameObject

1. Create empty → name **`MainMenuManager`**.
2. Add component → `MainMenuManager.cs`.
3. Drag **MainMenuCanvas** and **SettingsCanvas** into the Inspector slots.

### 3-4. Button wiring

| Button | OnClick() target | Method |
|--------|-----------------|--------|
| StartButton | MainMenuManager | `OnStartGameButton()` |
| SettingsButton | MainMenuManager | `OnSettingsButton()` |
| ExitButton | MainMenuManager | `OnExitButton()` |

### 3-5. SettingsCanvas

1. Select **SettingsCanvas** GameObject.
2. Add component → `SettingsManager.cs`.
3. Drag `MusicSlider`, `SFXSlider`, `BlindModeToggle` into slots.
4. **Do NOT wire slider `onValueChanged` in the Inspector** — the script handles it in `OnEnable`.

---

## 4. Scene 1 — Gameplay

### 4-1. Hierarchy structure

```
Gameplay (scene root)
├── GameManager
├── InputManager
├── UIManager
├── Canvas (Screen Space – Overlay, Scale With Screen Size 1080×1920)
│   ├── GameCanvas
│   │   ├── LevelText          (TextMeshPro)
│   │   ├── StatusText         (TextMeshPro)
│   │   ├── HeartsGroup
│   │   │   ├── Heart0 (Image)
│   │   │   ├── Heart1 (Image)
│   │   │   └── Heart2 (Image)
│   │   └── SettingsButton
│   ├── SettingsCanvas (inactive by default)
│   │   ├── MusicSlider
│   │   ├── SFXSlider
│   │   └── BlindModeToggle
│   └── GameOverCanvas (inactive by default)
│       ├── ScoreText          (TextMeshPro)
│       ├── BestScoreText      (TextMeshPro)
│       ├── RestartButton
│       └── HomeButton
```

### 4-2. GameManager GameObject

1. Create empty → name **`GameManager`** (exact spelling).
2. Add component → `GameManager.cs`.
3. Inspector values:
   - **Starting Hearts**: `3`
   - **Sound Delay Between**: `0.85`
   - **Pre Playback Delay**: `0.6`
   - **Level Complete Delay**: `1.2`
4. **Events** — leave empty for now; UIManager subscribes in code.

### 4-3. InputManager GameObject

1. Create empty → name **`InputManager`**.
2. Add component → `InputManager.cs`.
3. Inspector values:
   - **Min Swipe Distance**: `60`
   - **Double Swipe Window**: `0.3`
4. Events are optional; UIManager or a VFX script could subscribe.

### 4-4. UIManager GameObject

1. Create empty → name **`UIManager`**.
2. Add component → `UIManager.cs`.
3. Drag all canvas references into Inspector:

| Inspector slot | Drag from hierarchy |
|---------------|---------------------|
| Game Canvas | `GameCanvas` |
| Game Over Canvas | `GameOverCanvas` |
| Settings Canvas | `SettingsCanvas` |
| Level Text | `LevelText` |
| Status Text | `StatusText` |
| Heart Images [0] | `Heart0` (Image component) |
| Heart Images [1] | `Heart1` |
| Heart Images [2] | `Heart2` |
| Score Text | `ScoreText` |
| Best Score Text | `BestScoreText` |

### 4-5. Button wiring (Gameplay scene)

| Button | OnClick() target | Method |
|--------|-----------------|--------|
| SettingsButton | UIManager | `OnSettingsButton()` |
| RestartButton | UIManager | `OnRestartButton()` |
| HomeButton | UIManager | `OnHomeButton()` |

### 4-6. SettingsCanvas (Gameplay)

Same as Step 3-5:
1. Select `SettingsCanvas`.
2. Add component → `SettingsManager.cs`.
3. Fill `MusicSlider`, `SFXSlider`, `BlindModeToggle`.

---

## 5. Canvas Scaler (both scenes)

Select the root **Canvas** in each scene:
- **UI Scale Mode** → `Scale With Screen Size`
- **Reference Resolution** → `1080 × 1920`
- **Screen Match Mode** → `Match Width or Height`, slider = `0.5`

---

## 6. Execution Order (optional but recommended)

`Project Settings → Script Execution Order`:

| Script | Order |
|--------|-------|
| `AppInit` | -100 |
| `AudioManager` | -50 |
| `GameManager` | -10 |
| `InputManager` | 0 (default) |
| `UIManager` | 10 |

---

## 7. Testing Checklist

- [ ] **Play in Editor** — use mouse click-and-drag to simulate swipes
- [ ] Single swipe → you hear the sound
- [ ] Double swipe same direction within 0.3s → confirm fires
- [ ] Wrong confirm → heart disappears + error sound
- [ ] 3 wrong → GameOverCanvas appears with score
- [ ] Restart → back to Level 1, 3 hearts
- [ ] Home → Scene 0 loads
- [ ] Volume sliders change audio in real time
- [ ] Blind Mode toggle hides text elements
- [ ] High score persists after restarting Play mode

---

## 8. Common Errors & Fixes

| Error | Cause | Fix |
|-------|-------|-----|
| `NullReferenceException` on `AudioManager.Instance` | AudioManager not in scene or wrong GameObject name | Confirm object is named exactly `AudioManager` in Scene 0 and persists |
| Swipes not registering in Editor | Input System set to `Input Manager (Old)` only | Project Settings → Player → Active Input Handling → `Both` |
| Mixer `SetFloat` has no effect | Exposed parameter name typo | Check Mixer Exposed Parameters panel — must be `MusicVolume` / `SFXVolume` exactly |
| UIManager events not firing | Subscribed too late | UIManager subscribes in `Start()`; ensure GameManager's `Start()` doesn't fire events before that |
| Hearts not showing | heartImages array empty | Drag all 3 Heart Image components into the array (not the parent HeartsGroup) |
