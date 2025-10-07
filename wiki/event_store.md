---
title: Event Store
author: J.W. Morsink
difficulty: medium
archimate:
    caption: Event Store
    layer: Application
    type: Service
    serves: 
    - to: admin_ui
    - to: calculator
    - to: auto_import
    - to: asb
    - to: conversion_day#enter
    - to: conversion_day#invest
    - to: conversion_day#liquidate
    - to: conversion_day#exit
    - to: conversion_day#transfer
    - to: payout#register
---
# Event Store

The [Admin Module](./admin_module) records [events](./event) sequentially in the event store, which is currently implemented using Azure SQL.

The event store is a foundational component for event sourcing, ensuring that every change in financial or meta state is captured as an immutable event. 
This enables full auditability, historical reconstruction, and scenario testing for the platform.

Events in the store drive all model calculations in the [calculator](./calculator), support automated imports via [auto_import](./auto_import), and enable branching for isolated testing and development workflows.

The event store supports branching operations to manage different scenarios and histories:

- **Branch:** Create a new branch from an existing sequence of events.
- **Rebase:** Move a branch to a new base, aligning its history with another branch.
- **Fast forward:** Advance a branch to match the latest state of another branch.
- **Delete:** Remove a branch and its associated events.

Each branch represents a separate timeline of events, allowing for parallel development, testing, or analysis without affecting the main history. This is especially useful for validating new features, running simulations, or preparing for conversion days.

Merging is not supported; instead, branches must be rebased onto the target branch before fast forwarding. This design enforces a strictly linear event history, avoiding the complexity of directed acyclic graphs.

The event store integrates with the [admin_ui](./admin_module), [calculator](./calculator), and other platform modules to provide a consistent and reliable source of truth for all state transitions.

