---
title: NONE
author: J.W. Morsink
about: clr:/Common/FfAdmin.Common/NoneEvent
---

# NONE

```val/ks
@include "Module.ks" .
_ = code | call check_event(eventtype:NONE, common:NoneEvent)
```

The `NONE` event is a placeholder event type.
It is used to represent the absence of a valid event or as a default value in code and models.
This event should not occur in normal operation and does not affect system state.

## Fields

```table/ks
@include "Module.ks" .
@prefix cls: <clr:/Common/FfAdmin.Common/NoneEvent/> .
docs = {
    cls:Type ff:description "Identifies the event." ;
        ff:value "`NONE`" .
}
table = code | documented_event_table(common:NoneEvent, docs)
```

## Purpose

The NONE event is used internally to mark the start of an event sequence or to fill gaps where no real event exists.
It is not intended for use in business logic or user-facing features.
