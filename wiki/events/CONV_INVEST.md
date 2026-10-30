---
title: CONV_INVEST
author: J.W. Morsink
difficulty: medium
about: clr:/Common/FfAdmin.Common/ConvInvest
---

# CONV_INVEST

```val/ks
@include "Module.ks" .
_ = code | call check_event(eventtype:CONV_INVEST, common:ConvInvest)
```

The `CONV_INVEST` event records the investment of cash into an [investment option](../option).
It updates both the total invested amount and the cash reserves for the option.

## Fields

```table/ks
@include "Module.ks" .
@prefix cls: <clr:/Common/FfAdmin.Common/ConvInvest/> .
docs = {
    cls:Type ff:description "Identifies the event" ;
        ff:value "`CONV_INVEST`" .
    cls:Option ff:description "The identifier for the investment option" .
    cls:Invested_amount ff:description "The new total invested amount in the investment fund" .
    cls:Cash_amount ff:description "The new total cash amount in the option's reserves" .
    cls:Transaction_reference ff:description "An external reference for the investment transaction" .
}
table = code | documented_event_table(common:ConvInvest, docs)
```

## Purpose

This event is essential for tracking investments and maintaining accurate records of both invested and cash amounts for each option.
