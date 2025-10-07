---
title: Admin Module
author: J.W. Morsink
difficulty: medium
archimate:
    layer: Business
    type: Function
    caption: Admin Module
    relates:
    - to: conversion_day
    - to: payout
    - to: investment_process
    realizes:
    - to: strategy#investment
    - to: strategy#payout
    - to: strategy#history
---
# Admin Module

The admin module consist of several components, hosted on Microsoft Azure:

```arch(plantuml)
>component "Admin Module" as Admin {
>    component "Asp.Net Core webapp" as AzWeb #Technology {
        admin_ui;
>    }
>    component "Azure function app" as AzFnc1 #Technology {
        admin_ui d calculator;
        admin_ui d event_store;
        calculator d (event_store, model_cache);
>    }
    asb;
>    component "Azure function app" as AzFnc2 #Technology {
        auto_import d (event_store, calculator);
>    }

    (event_store, model_cache) d asb;
>}

administrator r admin_ui;
donor r  website;
calculator l website;
```

## Technical Implementation

The admin module is built around an [event-sourced architecture](https://martinfowler.com/eaaDev/EventSourcing.html), ensuring that all changes to financial and meta state are recorded as discrete events. This approach provides a complete audit trail and supports advanced features such as branching and scenario testing.

- **Event Sourcing:**
  - All state changes are captured as [events](./event), which are persisted in an [event store](./event_store).
  - The event store supports branching, enabling isolated test scenarios and snapshot management.

- **Model Aggregation:**
  - Domain [models](./calculator#models) aggregate events to compute the current state at any position in the event stream.
  - The [Calculator](./calculator) service processes the event stream to derive model values.

- **Model Caching:**
  - A model cache stores computed model states at specific event positions, reducing redundant calculations and improving performance.

- **User Interfaces:**
  - The 'Admin UI' web application allows administrators to perform all necessary actions for [conversion day](./conversion_day) and other operational tasks.
  - The website provides a user-friendly interface for making donations and viewing donation data.

- **Automated Import:**
  - The [auto importer](./auto_import) module ingests donation and charity data, converting them into events for processing by the admin module.

This architecture enables robust auditing, flexible scenario management, and efficient state computation for the Give for Good platform.