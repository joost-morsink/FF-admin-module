---
title: DONA_CANCEL
author: J.W. Morsink
about: clr:/Common/FfAdmin.Common/CancelDonation
---

# DONA_CANCEL

```val/ks
@include "Module.ks" .
_ = code | call check_event(eventtype:DONA_CANCEL, common:CancelDonation)
```

The `DONA_CANCEL` event represents the revocation of a direct debit donation.
It is only valid if the cancellation timestamp is before the execution timestamp of the original [donation](../donation).

## Fields

```table/ks
@include "Module.ks" .
@prefix cls: <clr:/Common/FfAdmin.Common/CancelDonation/> .
docs = {
    cls:Type ff:description "Identifies the event." ;
        ff:value "`DONA_CANCEL`" .
    cls:Donation ff:description "Identifies the donation to be cancelled." .
}
table = code | documented_event_table(common:CancelDonation, docs)
```

## Purpose

This event ensures that donations can be revoked before they are executed, maintaining accurate records and compliance with direct debit regulations.
