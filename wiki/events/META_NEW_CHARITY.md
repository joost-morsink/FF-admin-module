--- 
title: META_NEW_CHARITY
author: J.W. Morsink
about: clr:/Common/FfAdmin.Common/NewCharity
---

# META_NEW_CHARITY

```val/ks
@include "Module.ks" .
_ = code | call check_event(eventtype:META_NEW_CHARITY, common:NewCharity)
```

The `META_NEW_CHARITY` event creates a new [charity](../charity) in the system.
This charity can then be selected by a [donor](../donor) as a beneficiary for future [donations](../donation).

## Fields

```table/ks
@include "Module.ks" .
@prefix cls: <clr:/Common/FfAdmin.Common/NewCharity/> .
docs = {
    cls:Type ff:description "Identifies the event." ;
        ff:value "`META_NEW_CHARITY`" .
    cls:Code ff:description "Identifies the charity." .
    cls:Name ff:description "The name of the charity." .
}
table = code | documented_event_table(common:NewCharity, docs)
```

## Purpose

This event is essential for onboarding new charities and supporting donations made to them.