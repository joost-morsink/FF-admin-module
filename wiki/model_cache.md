---
title: Model Cache
author: J.W. Morsink
difficulty: hard
archimate: 
    caption: Model Cache
    layer: Application
    type: Service
    serves:
    - to: calculator
---

# Model Cache

The model cache is a technical component responsible for caching instances of [models](./calculator#models) at specific points in the event sequence.
This caching mechanism is designed to speed up calculations by avoiding the need to recompute model states from scratch for every query.

## Purpose

Calculations in the platform often require reconstructing the state of models based on a sequence of events.
Without caching, each calculation would need to replay all relevant events from the beginning, which can be slow and resource-intensive.
The model cache stores snapshots of model states at selected event indices, allowing the system to quickly restore a model to a known state and replay only the most recent events.

## Implementation

- The cache is typically implemented as a mapping from event indices to history hash to model instances.
  - By using history hashes, the cache can efficiently store and retrieve model states for multiple branches of the event sequence  
    This enables rapid access to the correct model state, even when the event history diverges due to branching or parallel changes.
- When a calculation is requested, the system first checks the cache for the nearest prior snapshot.
- Only events after the cached snapshot need to be processed to reach the desired state.
- Snapshots are created based on a model caching strategy, which may be optimized between releases.

## Benefits

- Reduces computation time for complex calculations.
- Improves responsiveness of the [calculator](./calculator) and related services.
- Enables efficient querying of historical states and aggregated data.

## Integration

The model cache serves the [calculator](./calculator) and other components that require fast access to model states.
It is especially important for processes that involve branching event histories, partitioned models, or frequent queries on historical data.

## Considerations

- Cache invalidation and consistency must be managed carefully to ensure correctness.
  - Invalidation is only needed when changes to software have been made, and should be done manually.
- The cache should be updated whenever new events are added.
- Storage and memory usage should be balanced against performance gains.

