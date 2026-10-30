--- 
title: META_NEW_OPTION
author: J.W. Morsink
about: clr:/Common/FfAdmin.Common/NewOption
---
# META_NEW_OPTION

```val/ks
@include "Module.ks" .
_ = code | call check_event(eventtype:META_NEW_OPTION, common:NewOption)
```

The `META_NEW_OPTION` event creates a new [investment option](../option) for investing donations.
The event specifies how profits should be divided using three [fractions](../option_fractions): `Reinvestment_fraction`, `FutureFund_fraction`, and `Charity_fraction`.
These three fractions must add up to 1.
The `Bad_year_fraction` is a minimal yearly fraction of the total invested amount that should always be transferred, regardless of profits.
It does not apply to profits, but to the invested value itself.

## Fields

```table/ks
@include "Module.ks" .
@prefix cls: <clr:/Common/FfAdmin.Common/NewOption/> .
docs = {
    cls:Type ff:description "Identifies the event." ;
        ff:value "`META_NEW_OPTION`" .
    cls:Code ff:description "Identifies the investment option." .
    cls:Name ff:description "The name of the investment option." .
    cls:Currency ff:description "ISO-4217 currency code of the investment option." .
    cls:Reinvestment_fraction ff:description "The fraction of profits to reinvest." .
    cls:FutureFund_fraction ff:description "The fraction of profits to donate to the future fund." .
    cls:Charity_fraction ff:description "The fraction of profits to donate to the charity." .
    cls:Bad_year_fraction ff:description "The minimal yearly fraction of the total option worth that should always be transferred." .
}
table = code | documented_event_table(common:NewOption, docs)
```

## Purpose

This event is essential for onboarding new investment options and defining how profits and minimum transfers are handled.
