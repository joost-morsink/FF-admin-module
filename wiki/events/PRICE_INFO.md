---
title: PRICE_INFO
author: J.W. Morsink
about: clr:/Common/FfAdmin.Common/PriceInfo
---

# PRICE_INFO

```val/ks
@include "Module.ks" .
_ = code | call check_event(eventtype:PRICE_INFO, common:PriceInfo)
```

The `PRICE_INFO` event allows input of current pricing information for an investment fund.
This enables the system to update the value of [donations](../donation) and plot their worth over time.

## Fields

```table/ks
@include "Module.ks" .
@prefix cls: <clr:/Common/FfAdmin.Common/PriceInfo/> .
docs = {
    cls:Type ff:description "Identifies the event." ;
        ff:value "`PRICE_INFO`" .
    cls:Option ff:description "The identifier for the investment option." .
    cls:Invested_amount ff:description "The total invested amount in the fund, according to the current investment fund price." .
}
table = code | documented_event_table(common:PriceInfo, docs)
```

## Purpose

This event is essential for keeping fund valuations up to date and for providing accurate historical data for option and donation worth.
