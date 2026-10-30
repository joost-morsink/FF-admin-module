---
title: CONV_INCREASE_CASH
author: J.W. Morsink
difficulty: medium
about: clr:/Common/FfAdmin.Common/IncreaseCash
---

# CONV_INCREASE_CASH

```val/ks
@include "Module.ks" .
_ = code | call check_event(eventtype:CONV_INCREASE_CASH, common:IncreaseCash)
```

The `CONV_INCREASE_CASH` event registers a non-donation increase in the cash amount of an [investment option](../option).
This event is typically used when investment costs are covered by a third party or when other sources add cash to the option.
It only affects the cash part of the investment option and does not represent a donation.

## Fields

```table/ks
@include "Module.ks" .
@prefix cls: <clr:/Common/FfAdmin.Common/IncreaseCash/> .
docs = {
    cls:Type ff:description "Identifies the event" ;
        ff:value "`CONV_INCREASE_CASH`" .
    cls:Option ff:description "The identifier for the investment option" .
    cls:Amount ff:description "The amount of cash to increase (must be ≥ 0)" .
}
table = code | documented_event_table(common:IncreaseCash, docs)
```

## Purpose

This event allows the system to track increases in cash that are not the result of donations.
It ensures that the cash balance of an investment option is accurate and reflects all sources of funding.

## Validation

* The `Option` field must be provided.
* The `Amount` must not be negative.
