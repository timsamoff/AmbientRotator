# Changelog

All notable changes to the Ambient Rotator asset will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-06-25

### Added
#### Core System
- **AmbientRotator** component: Primary rotation engine with 6 motion profiles (Subtle, Gentle, Organic, Dynamic, Chaotic, Custom)
- **MotionReaction** system: Shared configuration for reaction behaviors (Pulse, Rotate, Wobble)
- External influence API: `AddPositionOffset()`, `AddRotationOffset()`, `SetSpeedMultiplier()`
- Smooth interpolation using Vector3.SmoothDamp for conflict-free motion

#### Beat Synchronization
- **BeatSyncObject**: AudioSource-based beat detection with configurable thresholds and spectrum analysis
- **BeatSyncModule**: Receiver component that applies beat reactions to objects with AmbientRotator
- Influence radius system for localized beat reactions
- Automatic beat strength normalization and reaction scaling

#### Reactive Triggers
- **ReactiveTriggerObject**: Trigger zone source with Push/Pull forces and reaction presets
- **ReactiveTriggerModule**: Receiver that applies trigger reactions with smooth return behavior
- Distance-based strength calculation (full at center, zero at edge)
- Layer mask filtering for selective reactions

#### Wind System
- Global wind with gusts and turbulence simulation
- Localized wind zones for environment-specific effects
- Automatic wind force application to all AmbientRotator instances
- Scene view visualization with direction arrows and zone gizmos

#### Preset & Profile System
- **Profile Library** with 5 built-in motion profiles
- Custom profile creation with animation curve editing
- **Preset Browser** window for saving and applying complete configurations
- Profile blending for complex, layered motion

#### Editor Tools
- Custom inspectors with real-time preview
- Multi-object editing support
- Quick preset buttons on component header
- Scene view gizmos for rotation ranges and wind direction
- Debug logging toggle per component

#### Example Content
- **Forest Demo**: Trees and foliage with organic wind motion
- **UI Demo**: Interface elements with subtle ambient rotation
- **Music Demo**: Beat-synced object reactions with audio visualization
- Complete preset library with 12 ready-to-use configurations

#### Documentation
- Full API reference
- Setup guides for each subsystem
- Performance optimization guide
- Extensibility examples for custom modules

### Performance
- Auto-batching for scenarios with 50+ receivers
- Cached wind calculations (updated once per frame)
- Configurable spectrum sample sizes (16-512 samples)
- Optimized gizmo rendering with LOD-based simplification
- `useUnscaledTime` support for consistent performance

### Unity Compatibility
- Unity 2021.3 LTS and newer
- Supports Built-in Render Pipeline, URP, HDRP
- Compatible with both 2D and 3D projects