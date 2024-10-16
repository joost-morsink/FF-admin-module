---
title: Conversion day
author: J.W. Morsink    
archimate:
    layer: Business
    type: Process
"#in":
    layer: Business
    type: Process
    caption: Cash In
    url: "#the-in-process"
    triggers:
    - to: "#out"
      caption: optional
    aggregates:
    - to: conversion_day
"#out":
    layer: Business
    type: Process
    caption: Cash Out
    url: "#the-out-process"
    triggers:
    - to: "#transfer"
      caption: optional
    aggregates:
    - to: conversion_day
"#transfer":
    layer: Business
    type: Process
    caption: Transfer Cash
    url: "#the-transfer-process"
    aggregates:
    - to: conversion_day
    influences:
    - to: models/amounts_to_transfer
      bidirectional: true
"#enter":
    layer: Business
    type: Process
    caption: Enter
    url: "#the-in-process"
    composes:
    - to: "#in"
    triggers:
    - to: "#invest"
    influences:
    - to: models/option_worths
"#invest":
    layer: Business
    type: Process
    caption: Invest
    url: "#the-in-process"
    composes:
    - to: "#in"
    triggers:
    - to: "#liquidate"
      caption: optional
"#liquidate":
    layer: Business
    type: Process
    caption: Liquidate
    url: "#the-out-process"
    composes:
    - to: "#out"
    triggers:
    - to: "#exit"
"#exit":
    layer: Business
    type: Process
    caption: Exit
    url: "#the-out-process"
    composes:
    - to: "#out"
    triggers:
    - to: "#transfer"
      caption: optional
    influences:
    - to: models/option_worths
    - to: models/amounts_to_transfer
---

# Conversion day

Conversion day exist of three different subprocesses, which may or may not occur on the same day. 

## Overview

``` arch(plantuml)
$subprocess = (#in, #out, #transfer);
$insteps = (#enter, #invest);
$outsteps = (#liquidate, #exit);
$steps = (#enter, #invest, #liquidate, #exit, #transfer);

conversion_day d $subprocess;
$subprocess;
#in d $insteps;
#out d $outsteps;
$insteps;
$outsteps;

(event_store, investments) u $steps;

admin_ui u 3 conversion_day;

> component "G4g Admin Module" as Web 
> component "Investment bank website" as Bank 
> investments <|-. Bank
> event_store <|-. Web
> admin_ui <|-. Web
```

## Models

The conversion day process makes use of some models in the [calculator module](./calculator).
```arch(plantuml)
$steps = (#enter, #invest, #liquidate, #exit, #transfer);

$steps;
> component Calculator {
  $models = (models/option_worths, 
             models/ideal_option_valuations,
             models/minimal_exits,
             models/amounts_to_transfer);
  $models;
> }

$steps d $models;
```

## The in process

The in process supports the cashflow of donations into the investmet option and consists of two steps:

* [Enter](./events/CONV_ENTER)
* [Invest](./events/CONV_INVEST)

These steps are usually executed right after each other.
Enter indicates the state change of unentered donations to entered donations, which means they're going to participate in the [investment option](./option) from that moment on.
Invest is a step that allows the administration of the transfer of monetary funds from the cash part to the invested part of the investment option.
The investment option's [worth](./worth) is not modified by this step.

## The out process

The out process supports the cashflow of invested funds out of the investment option for allocation of monetary funds to the [charity](./charity) and consists of two steps:

* [Liquidate](./events/CONV_LIQUIDATE)
* [Exit](./events/CONV_EXIT)

These steps are usually executed right after each other.
Liquidate is a step that allows the administration of the transfer of monetary funds from the invested part to the cash part of the investment option.
The investment option's [worth](./worth) is not modified by this step.
Exit indicates the transfer of monetary funds out of the investment option for [allocation](./allocation) to [charities](./charity).

### Exit

The exit step uses the [Calculator](./calculator) for calculating two specific values:

* The exit amount that should be withdrawn based on the profit since the last exit and the [reinvestment fraction](./option_fractions). This value **could** be negative.
* The amount that should be withdrawn based on the [bad year fraction](./option_fractions#bad-year-fraction) and the amount of time since the last exit. This value is **always** positive.

The exit amount should be the largest of these two, always resulting in a positive amount.

## The transfer process

The transfer process uses the [CONV_TRANSFER](./events/CONV_TRANSFER) event to administer exactly how much funds have been transferred to each [charity](./charity).
