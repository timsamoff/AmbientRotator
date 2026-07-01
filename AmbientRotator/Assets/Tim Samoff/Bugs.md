Rename Pulse to Bounce.
Pulse/Bounce works, but it only affects rotation.
    It should bounce the object up and down and then lerp back to the origin when complete.
Wobble works, but it only affects rotation.
    It should wobble the object around in space as well and then lerp back to origin when complete.
Push Away and Attract affect rotation, not position.
    Should move away from or toward trigger object and then lerp back to origin when complete.
Rotate works great, but not chaotic enough - top intensity should be insane (maybe change to 0 - 1000).
Need to ADD new "Pulse" Reaction that scales the object according to its settings.
    Then it would lerp back to the original when complete.
    New order would be Bounce, Pulse, Rotate, Wobble, Push Away, Attract
BeatSync:
    Influence Radius needs an on/off checkbox
    The Max Beat Threshold top clamp value seems odd

ReactiveTrigger needs to work without colliders like BeatSync's influence works.
    This would need to be applied to the ReactiveTriggerObject script.
    Add Influence settings just like in BeatSync.

PhysicsAmbientRotator should have the exact same UI as AmbientRotator.

Still can't make AmbientGroup work.