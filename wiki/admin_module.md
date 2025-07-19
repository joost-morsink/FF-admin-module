---
title: Admin Module
author: J.W. Morsink
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

## Technical implementation

The admin module uses [event sourcing](https://martinfowler.com/eaaDev/EventSourcing.html) to administer changes in the (financial or meta) state of the Give for Good concept. 
It defines [events](./event) to record these state changes, and [models](./calculator#models) to aggregate these events into insightful parts of the overall state at any given position in the event sequence.
