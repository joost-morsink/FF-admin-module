---
title: Give for Good - Event Store
author: J.W. Morsink
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

The [Admin Module](./admin_module) stores [events](./event) in a sequential order in a store. 
Currently this store is implemented in an Azure SQL database.
It supports a simple branching mechanism with the following operations:

* Branch
* Rebase
* Fast forward
* Delete

There is no concept of a merge, a branch needs to be rebased on the target branch in order to fast forward the target branch.
This ensures a purely linear history (not an acyclic directed graph).

