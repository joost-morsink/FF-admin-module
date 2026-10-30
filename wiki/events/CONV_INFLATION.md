---
title: CONV_INFLATION
author: J.W. Morsink
difficulty: medium
about: clr:/Common/FfAdmin.Common/ConvInflation
---

# CONV_INFLATION

```val/ks
@include "Module.ks" .
_ = code | call check_event(eventtype:CONV_INFLATION, common:ConvInflation)
```

The `CONV_INFLATION` event requests an inflation correction for a specific [investment option](../option).
This event ensures that inflation is properly administered before profits are measured or distributed.
It directly influences the [ideal valuation](../models/ideal_option_valuations) of the option.

## Fields

```table/ks
@include "Module.ks" .
@prefix cls: <clr:/Common/FfAdmin.Common/ConvInflation/> .
docs = {
    cls:Type ff:description "Identifies the event" ;
        ff:value "`CONV_INFLATION`" .
    cls:Option ff:description "The identifier for the investment option" .
    cls:Invested_amount ff:description "The total invested amount in the investment fund" .
    cls:Inflation_factor ff:description "The factor by which monetary amounts should be multiplied" .
}
table = code | documented_event_table(common:ConvInflation, docs)
```

## Inflation Factor

The inflation factor is administered as a multiplier rather than a percentage.
This approach keeps calculation logic simple and consistent.
For example, a `2%` inflation is represented by a factor of `1.02`.

## Purpose

This event allows the system to adjust valuations for inflation, ensuring fair and accurate profit calculations.