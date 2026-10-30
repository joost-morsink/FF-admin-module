--- 
title: META_CHARITY_PARTITION
author: J.W. Morsink
about: clr:/Common/FfAdmin.Common/CharityPartition
---

# META_CHARITY_PARTITION

```val/ks
@include "Module.ks" .
_ = code | call check_event(eventtype:META_CHARITY_PARTITION, common:CharityPartition)
```

The `META_CHARITY_PARTITION` event turns a [charity](../charity) into a ['donation theme'](../theme) by specifying how donations to the theme are distributed among other charities.
If the partitions are empty or contain only a self-reference, the theme is restored as a regular charity.

## Fields

```table/ks
@include "Module.ks" .
@prefix cls: <clr:/Common/FfAdmin.Common/CharityPartition/> .
docs = {
    cls:Charity ff:description "Identifies the donation theme." .
    cls:Type ff:description "Identifies the event." ;
        ff:value "`META_CHARITY_PARTITION`" .
    cls:Partitions ff:description "The distribution of the theme among charities." .
}
table = code | documented_event_table(common:CharityPartition, docs)
```

## Purpose

This event allows flexible grouping of charities under a theme, enabling donations to be split according to specified partitions.
It supports both composite themes and reverting to a single charity.
