--- 
title: CONV_ENTER
author: J.W. Morsink
difficulty: medium
about: clr:/Common/FfAdmin.Common/ConvEnter
---

# CONV_ENTER

```val/ks
@include "Module.ks" .
_ = code | call check_event(eventtype:CONV_ENTER, common:ConvEnter)
```

The `CONV_ENTER` event marks the moment when all new [donations](../donation) are incorporated into an [investment option](../option) as cash.
Updating the invested amount is essential for processing the event correctly because it redefines [ownership fractions](../ownership_fractions) for the option.


## Fields

```table/ks
@include "Module.ks" .
@prefix cls: <clr:/Common/FfAdmin.Common/ConvEnter/> .

docs = {
    cls:Invested_amount ff:description "The total invested amount of money in the investment option" .
    cls:Option ff:description "The identifier for the investment option" .
    cls:Type ff:description "The type of the event" ;
        ff:value "`CONV_ENTER`" .
}
table = code | documented_event_table(common:ConvEnter, docs)
```

## Purpose

This event ensures that all donations are properly accounted for in the option and that ownership fractions are updated accordingly.
It is a key step in the lifecycle of an investment option.
