
# Ambient Rotator

**Organic Motion System for Unity** 

*Version 1.0.0 | Unity 2021.3+*

Ambient Rotator brings natural, reactive motion to your Unity projects. Whether you're creating immersive environments, dynamic UI, or music-driven visualizations, this framework provides a modular approach to organic movement that's both powerful and easy to use.

## Table of Contents
- [Quick Start](#quick-start)
- [Core Features](#core-features)
- [Architecture](#architecture)
- [Subsystems](#subsystems)
- [Getting Started](#getting-started)
- [Presets & Profiles](#presets--profiles)
- [Performance Tips](#performance-tips)
- [Extending the System](#extending-the-system)
- [Examples](#examples)
- [Support](#support)

## Quick Start
1. Import the Ambient Rotator package into your Unity project
2. Select any GameObject in your scene
3. Click **Add Component** → **Ambient Rotator**
4. Choose a **Motion Profile** from the dropdown
5. Adjust **Intensity** and **Speed** to taste
6. Press Play and watch your objects come alive

**That's it!** Your object now has organic, natural motion.

## Core Features

### Motion Profiles
| Profile | Use Case | Characteristics |
|---------|----------|-----------------|
| **Subtle** | UI elements, indicators | Minimal movement, barely perceptible |
| **Gentle** | Idle characters, props | Soft swaying, calming presence |
| **Organic** | Foliage, environmental | Natural, unpredictable motion |
| **Dynamic** | Interactive objects | Noticeable movement with variety |
| **Chaotic** | Stylized effects, particles | Dramatic, energetic motion |
| **Custom** | Advanced scenarios | Curve-driven, fully programmable |

### System Highlights

- **Beat Synchronization** — Objects pulse, spin, or wobble in time with music
- **Reactive Triggers** — Objects respond when players enter trigger zones
- **Wind System** — Global wind with gusts, turbulence, and localized zones
- **Preset System** — Save and reuse complete configurations
- **Profile Library** — Browse and apply curated motion profiles
- **Editor Tools** — Visual debugging, multi-object editing, and real-time preview

## Architecture
Ambient Rotator follows a **single source of truth** principle: only the `AmbientRotator` component directly modifies GameObject transforms. All other features compose their effects through it, ensuring smooth, conflict-free motion.

    ┌────────────────────────────────────────────┐  
    │ AmbientRotator                             │  
    │ (Core Engine - Single Source)              │  
    │ ┌──────────────────────────────────────┐   │  
    │ │ Base Motion + External Offsets +     │   │  
    │ │ Speed Multiplier + Wind Forces       │   │  
    │ └──────────────────────────────────────┘   │  
    └────────────────────────────────────────────┘  
                          ▲  
                          │  
          ┌───────────────┼───────────────┐  
          │               │               │  
          ▼               ▼               ▼  
    ┌──────────┐    ┌────────────┐   ┌──────────┐  
    │   Beat   │    │  Reactive  │   │   Wind   │  
    │  System  │    │   System   │   │  System  │  
    └──────────┘    └────────────┘   └──────────┘

**Key Design Principles:**

- **Composition over Conflict** — Modules add effects rather than overwriting
- **Source/Receiver Pattern** — Reactions are defined on sources, applied to receivers
- **Smooth Decay** — All external effects decay naturally over time
- **Consistent Configuration** — Shared settings ensure uniform behavior

## Subsystems

### Beat Synchronization

Connect objects to your music for reactive visual experiences.

**Setup:**

1. Add `BeatSyncObject` to your AudioSource GameObject
2. Configure detection threshold and influence radius
3. Add `BeatSyncModule` to objects with AmbientRotator
4. Objects now react to every beat

**Reaction Types:** Pulse (up/down), Rotate (continuous), Wobble (side-to-side)

### Reactive Triggers

Create interactive environments where objects respond to player presence.

**Setup:**

1. Add `ReactiveTriggerObject` to a trigger collider
2. Define reaction (Pulse/Rotate/Wobble) and Push/Pull forces
3. Add `ReactiveTriggerModule` to affected objects
4. Objects react when entering the trigger zone

**Special Effects:** Push Away, Attract, Distance-based strength falloff

### Wind System

Add environmental atmosphere with dynamic wind forces.

**Setup:**

1. Add `WindSystem` to any GameObject
2. Configure global wind direction, strength, gustiness
3. Objects with AmbientRotator automatically respond to wind
4. Optionally add Wind Zones for localized effects
**Features:** Global wind, localized zones, turbulence simulation, debug visualization

## Getting Started

### Installation

1. Download the Ambient Rotator package from the Unity Asset Store
2. Import into your project: **Assets → Import Package → Custom Package**
3. The `AmbientRotator/` folder will appear in your Project window

### First Object Setup

    csharp
    
    // Add and configure via code
    var rotator = gameObject.AddComponent<AmbientRotator>();
    rotator.Profile = AmbientProfile.Gentle;
    rotator.Intensity = 0.8f;
    rotator.Speed = 1.2f;
    
    // Or use the Inspector

### Multi-Object Configuration

Select multiple GameObjects with AmbientRotator components to edit them all at once—great for prototyping and iteration.

## Presets & Profiles

### Using Presets

1.  Open **Window → Ambient Rotator → Preset Browser**
2.  Browse through built-in presets (Forest, UI, Music, etc.)
3.  Select a preset and click **Apply to Selected**
4.  All AmbientRotator components on selected objects update instantly    

### Creating Custom Profiles

1.  Right-click in the Project window
2.  Select **Create → Ambient Rotator → Custom Profile**
3.  Name your profile and choose a template
4.  Edit the animation curves in the custom inspector
5.  Preview motion in real-time on selected objects

### Profile Blending

Complex motions can be achieved by blending multiple profiles—a professional technique for sophisticated animations.



## Performance Tips

| Tip | When to Use | Impact |
|-----|-------------|--------|
| **Use `clampMovement`** | Limit maximum displacement for predictable performance | Prevents objects from drifting too far from their origin, reducing physics and transform overhead |
| **Employ AmbientGroup** | Centralize control for many objects (forests, crowds, 50+ instances) | Significantly reduces per-object processing by batching updates and calculations |
| **Enable `useUnscaledTime`** | When using Time.timeScale for slow-motion or pause effects | Maintains consistent motion and prevents performance dips during time manipulation |
| **Reduce spectrum samples** | Mobile devices or low-end hardware | Lowers audio analysis overhead from 512 to 64-128 samples while maintaining responsive beat detection |
| **Use simpler profiles** | Non-critical objects or large quantities | Subtle and Gentle profiles require less processing than Chaotic or Dynamic profiles |
| **Batch receivers** | Scenes with 50+ AmbientRotator instances | Auto-batching handles multiple instances efficiently; AmbientGroup provides even better performance |
| **Disable unused subsystems** | When Beat Sync, Reactive Triggers, or Wind are not needed | Removes update loop overhead from inactive components |
| **Adjust update frequency** | Non-critical decorative objects | Use coroutines or InvokeRepeating instead of per-frame updates in custom modules |
| **Use triggers wisely** | When setting up Reactive Triggers | Keep collider sizes reasonable; use layer masks to limit per-frame collision checks |
| **Profile caching** | When using custom profiles | Cache profile references rather than looking them up every frame to reduce garbage collection |

## Extending the System

### Adding Custom Modules

The modular architecture supports easy extension. Create your own reaction types, sources, and receivers.

**Example: Custom Reaction Module**

    csharp
    
    public class CustomReactionModule : MonoBehaviour
    {
     private AmbientRotator rotator;
      
     void Start() => rotator = GetComponent<AmbientRotator>();
      
     void Update()
     {
     if (SomeCondition())
     {
     // Add positional movement
     rotator.AddPositionOffset(Vector3.up * 0.5f);
     // Add rotational spin
     rotator.AddRotationOffset(Vector3.up * 10f);
     }
     }
    }

**Building Custom Sources:**

1.  Define a reaction configuration
2.  Broadcast events to nearby receivers
3.  Let receivers apply the reaction via AmbientRotator API

### Extending Reaction Types

The system's `MotionReaction` class can be extended to support new behaviors:

-   Scale Reaction (pulse/scale objects)
-   Color Reaction (change colors on triggers)
-   Path Reaction (move along spline paths)
-   Particle Emission (burst particles on beats)

## Examples

The package includes three complete example scenes to demonstrate real-world usage:

-   **[Open Scene]**  `AmbientRotator/Examples/Scenes/`

### Basic Usage Demo
-   Basic demonstration of Ambient Rotator Presets

### Beat Sync Demo
-   Objects pulsing and spinning to audio beats
-   Multiple reaction types synchronized to music

### UI Demo

-   Interface elements with subtle ambient rotation
-   Collection of UI-friendly presets
-   Performance optimized for UI canvases

### Wind System Demo Demo

-   Trees, foliage, and other objects with organic wind motion
-   Multiple wind zones creating varied environments

## Support

### Documentation

**Coming Soon:** Full API reference and setup guides are available in the [Ambient Rotator Documentation].

### Troubleshooting

-   **Objects not moving?** Check that the AmbientRotator component is enabled and has a valid profile
    
-   **Beat sync not working?** Ensure your AudioSource is playing and BeatSyncObject is attached
    
-   **Triggers not reacting?** Verify layer masks and trigger collider settings
    

### Contact

-   **Email:**  samoff@gmail.com
-   **GitHub:** Report issues or request features
    

### Roadmap

Future updates will include:

-   Scale reaction support
-   Color modulation
-   Spline-based path motion
-   Particle system integration
-   Animation clip triggers

----------

_Ambient Rotator v1.0.0 | Made with ❤️ for the Unity community_