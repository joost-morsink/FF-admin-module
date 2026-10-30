---
title: Audit
author: J.W. Morsink
difficulty: medium
about: clr:/Common/FfAdmin.Common/Audit
---

# Audit

```val/ks
@include "Module.ks" .
_ = code | call check_event(eventtype:AUDIT, common:Audit)
```

The `AUDIT` event records a snapshot of the system's state for verification and compliance.
It consolidates key metrics and hash codes, allowing integrity checks of all processed events up to the audit point.

An audit event is typically created on demand.
It does not record arbitrary changes, but instead summarizes the state for auditing and compliance purposes.

Audit events are essential for maintaining trust and transparency in the Give for Good platform.

## Fields

```table/ks
@include "Module.ks" .
@prefix cls: <clr:/Common/FfAdmin.Common/Audit/> .
docs = {
    cls:Type ff:description "Must be `AUDIT`" .
    cls:Hashcode ff:description "Hash of the event history up to this point" .
    cls:PreviousHashCode ff:description "Hash of the previous audit event (if any)" .
    cls:EventCount ff:description "Number of events processed" .
    cls:PreviousCount ff:description "Number of events at previous audit (if any)" .
}
table = code | documented_event_table(common:Audit, docs)
```


## Purpose

Audit events enable:

* Verification of the system's state and event history.
* Detection of inconsistencies or tampering.
* Potentially support regulatory or compliance requirements.
