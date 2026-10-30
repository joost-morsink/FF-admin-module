--- 
title: CONV_EXIT
author: J.W. Morsink
difficulty: medium
about: clr:/Common/FfAdmin.Common/ConvExit
---

# CONV_EXIT

```val/ks
@include "Module.ks" .
_ = code | call check_event(eventtype:CONV_EXIT, common:ConvExit)
```

The `CONV_EXIT` event marks the allocation of funds from the cash amount of an [investment option](../option) to [charities](../charity).
This event determines the amount to be distributed, but the actual [transfer](../transfer) to charities may occur later.

## Fields

```table/ks
@include "Module.ks" .
@prefix cls: <clr:/Common/FfAdmin.Common/ConvExit/> .
docs = {
    cls:Type ff:description "Identifies the event" ;
        ff:value "`CONV_EXIT`" .
    cls:Option ff:description "The identifier of the investment option" .
    cls:Amount ff:description "The amount to be allocated to charities" .
}
table = code | documented_event_table(common:ConvExit, docs)
```

## Purpose

This event is essential for triggering the allocation process and determining how much will be distributed to charities.
It is a key step in the payout lifecycle for investment options.

## Notes

Transfers to charities may be postponed due to minimum thresholds or transfer costs.
Allocated amounts are tracked until they are eligible for transfer.