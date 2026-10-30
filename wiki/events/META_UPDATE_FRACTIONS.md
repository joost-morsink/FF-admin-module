--- 
title: META_UPDATE_FRACTIONS
author: J.W. Morsink
about: clr:/Common/FfAdmin.Common/UpdateFractions
---

# META_UPDATE_FRACTIONS

```val/ks
@include "Module.ks" .
_ = code | call check_event(eventtype:META_UPDATE_FRACTIONS, common:UpdateFractions)
```

The `META_UPDATE_FRACTIONS` event updates the reinvestment [fractions](../option_fractions) for an [investment option](../option).
The three fractions (`Reinvestment_fraction`, `FutureFund_fraction`, and `Charity_fraction`) must add up to 1.
The `Bad_year_fraction` specifies the minimal fraction of the total amount in the investment option that should always be transferred, regardless of profits.

## Fields

```table/ks
@include "Module.ks" .
@prefix cls: <clr:/Common/FfAdmin.Common/UpdateFractions/> .
docs = {
    cls:Type ff:description "Identifies the event." ;
        ff:value "`META_UPDATE_FRACTIONS`" .
    cls:Code ff:description "The identifier for the investment option." .
    cls:Reinvestment_fraction ff:description "The fraction of profits to reinvest." .
    cls:FutureFund_fraction ff:description "The fraction of profits to donate to the future fund." .
    cls:Charity_fraction ff:description "The fraction of profits to donate to the charity." .
    cls:Bad_year_fraction ff:description "The minimal fraction of the total option amount that should always be transferred." .
}
table = code | documented_event_table(common:UpdateFractions, docs)
```

## Purpose

This event is essential for updating how profits and minimum transfers are handled for an investment option.
