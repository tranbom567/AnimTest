# Spring Bone(using Unity Job System)

  

## Overview

  

This Unity project contains a spring-bone simulation for a single child-bone chain. It updates bone transforms in `LateUpdate` using a Burst-compiled  job, with optional wind, spherical collision checks, and twist locking.

The project is using Unity `6000.6.4f1`.
## Setup

  

1. Add a `SpringBone` component to the first transform in the chain.

2. Ensure that transform has a parent; the parent is used as the simulation root.

3. Keep the chain under the spring-bone transform. The current setup follows child index `0` at each level.

4. Add a `SpringBoneColliderManager` to a scene object and place `SpringBoneCollider` components on objects that should push the simulated tips away.

5. Add `WindZoneData` to a scene object if Wind Zone is used in scene. It is expected to exist because `SpringBone` reads its singleton in `LateUpdate`.

6. Configure the `SpringBone` component's serialized fields in the Inspector.

  

## SpringBone settings

  

-  **Stiffness**: strength of the spring that pulls the simulated tip toward its animated/rest tip.

-  **Drag Force**: damping applied against the simulated velocity.

-  **Gravity** and **Mass**: gravity acceleration and its multiplier in the force calculation.

-  **Bone Axis**: the local axis that should point along the bone.

-  **Wind Effect**: enables or disables wind force for this chain.

## Current constraints

- Collider radius is multiplied by `transform.localScale.magnitude`; this is an approximation, particularly for non-uniform scale.

- The collider manager and wind manager are accessed through singletons and should be present and initialized in the scene(wind zones and colliders that added in runtime is unused).
