---
title: Conversion day
author: J.W. Morsink 
difficulty: medium
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

# Conversion Day

Conversion day consists of three main subprocesses, which may occur together or separately:

- Cash In
- Cash Out
- Transfer Cash

Each subprocess is supported by dedicated [checklists](./checklist) to ensure correct execution.

## Overview

```arch(plantuml)
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

The conversion day process relies on several models from the [calculator module](./calculator):

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

## Cash In Process

The cash in process manages the flow of donations into investment options and includes two steps:

* [Enter](./events/CONV_ENTER)
* [Invest](./events/CONV_INVEST)

These steps are usually executed right after each other.
Enter indicates the state change of unentered donations to entered donations, which means they're going to participate in the [investment option](./option) from that moment on.
Invest is a step that allows the administration of the transfer of monetary funds from the cash part to the invested part of the investment option.
The investment option's [worth](./worth) is not modified by this step.

Users may follow the [in process checklist](./guides/in_process)


## Cash Out Process

The out process supports the cashflow of invested funds out of the investment option for allocation of monetary funds to the [charity](./charity) and consists of two steps:

* [Liquidate](./events/CONV_LIQUIDATE)
* [Exit](./events/CONV_EXIT)

These steps are usually executed right after each other.
Liquidate is a step that allows the administration of the transfer of monetary funds from the invested part to the cash part of the investment option.
The investment option's [worth](./worth) is not modified by this step.
Exit indicates the transfer of monetary funds out of the investment option for [allocation](./allocation) to [charities](./charity).

Users may follow the [out process checklist](./guides/out_process)

### Exit Calculation

The exit step uses the [Calculator](./calculator) to determine:

- The exit amount to withdraw, based on profit since the last exit and the [reinvestment fraction](./option_fractions). This value may be negative.
- The amount to withdraw based on the [bad year fraction](./option_fractions#bad-year-fraction) and elapsed time since the last exit. This value is always positive.

The final exit amount is the greater of these two, ensuring a positive withdrawal.

## Transfer Process

The transfer process uses the [CONV_TRANSFER](./events/CONV_TRANSFER) event to record the exact funds transferred to each [charity](./charity).
This process implements the [payout capability](./payout).
