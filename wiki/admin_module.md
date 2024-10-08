---
title: Give for Good - Admin Module
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

```pumlarch
component "Admin Module" as Admin {
    component "Asp.Net Core webapp" as AzWeb #Technology {
        ~admin_ui
    }
    component "Azure function app" as AzFnc1 #Technology {
        ~admin_ui d calculator
        ~admin_ui d event_store
        ~calculator d event_store
        ~calculator d model_cache
    }
    ~asb
    component "Azure function app" as AzFnc2 #Technology {
        ~auto_import d event_store
        ~auto_import d calculator
    }

    ~event_store d asb
    ~model_cache d asb
}
~administrator r admin_ui
~donor r website
~calculator l website
```