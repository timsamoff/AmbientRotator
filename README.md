
# Ambient Rotator

Bring your scenes to life with organic, intelligent motion. Zero animation work required. Features 5 motion profiles, custom curves, wind system, beat sync, and reactive triggers. Just drag, drop, and watch your objects come alive!

[![Unity Version](https://img.shields.io/badge/Unity-2019.4+-blue.svg)](https://unity.com)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)

---

## Quick Start

### Installation

**Unity Package Manager:**

**Manual:**
1. Download `.unitypackage` from Releases
2. Import: Assets → Import Package → Custom Package

### Basic Usage

1. Select any GameObject
2. Add Component → Ambient Rotator
3. Choose a Profile (Subtle, Gentle, Organic, Dynamic, Chaotic)
4. Adjust Intensity and Speed
5. Press Play!

### Apply Presets

1. Window → Ambient Rotator → Preset Browser
2. Select a preset (Nature, UI, Fantasy, SciFi)
3. Click "Apply to Selected"

---

## Features

- 🎯 5 Motion Profiles: Subtle, Gentle, Organic, Dynamic, Chaotic
- 🎨 Custom Profiles with Animation Curves
- 🌬️ Wind System with Turbulence
- 👥 Group Control with Wave Motion
- 🎵 Beat Sync for Music Integration
- 🎮 Reactive Triggers for Player Interaction
- 🖥️ UI Support (Position, Scale, Color)
- ⚛️ Physics Integration
- 📦 Preset System
- 🛠️ Full Editor Tools

---

## Examples

```csharp
// Basic setup
var rotator = gameObject.AddComponent<AmbientRotator>();
rotator.SetProfile(MotionProfile.Organic);
rotator.SetIntensity(1.5f);

// Group control
var group = gameObject.AddComponent<AmbientGroup>();
group.AutoFindMembers();
group.SetMasterIntensity(1.2f);

// React to player
var trigger = gameObject.AddComponent<ReactiveTrigger>();
trigger.SetReactionRadius(5f);

// Beat sync
var beatSync = gameObject.AddComponent<BeatSyncModule>();
beatSync.SetAudioSource(musicSource);