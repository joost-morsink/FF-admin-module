--- 
title: CONV_LIQUIDATE
author: J.W. Morsink
difficulty: medium
about: clr:/Common/FfAdmin.Common/ConvLiquidate
---

# CONV_LIQUIDATE

```val/ks
@include "Module.ks" .
_ = code | call check_event(eventtype:CONV_LIQUIDATE, common:ConvLiquidate)
```

The `CONV_LIQUIDATE` event records the liquidation of invested funds from an [investment option](../option).
It moves money from the invested amount to the cash reserves, typically in preparation for a future [transfer](../transfer) to [charities](../charity).
This event does not itself transfer funds to charities, but makes them available for allocation and transfer.

## Fields

```table/ks
@include "Module.ks" .
@prefix cls: <clr:/Common/FfAdmin.Common/ConvLiquidate/> .
docs = {
    cls:Type ff:description "Identifies the event" ;
        ff:value "`CONV_LIQUIDATE`" .
    cls:Option ff:description "The identifier for the investment option" .
    cls:Invested_amount ff:description "The new total invested amount in the investment option" .
    cls:Cash_amount ff:description "The new total cash amount in the option's reserves" .
    cls:Transaction_reference ff:description "An external reference for the withdrawal transaction" .
}
table = code | documented_event_table(common:ConvLiquidate, docs)
```

## Purpose

This event is essential for tracking the movement of funds within an investment option and ensuring that cash is available for future allocation and transfer.
