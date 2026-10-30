---
title: DONA_NEW
author: J.W. Morsink
about: clr:/Common/FfAdmin.Common/NewDonation
---

# DONA_NEW

```val/ks
@include "Module.ks" .
_ = code | call check_event(eventtype:DONA_NEW, common:NewDonation)
```

The `DONA_NEW` event records a [donation](../donation) made by a [donor](../donor) to a [charity](../charity) via an [investment option](../option).
If the donation currency matches the investment option's currency, the `Amount` and `Exchanged_amount` fields are equal.
If not, the `Exchanged_amount` field records the result of the currency exchange.

## Fields

```table/ks
@include "Module.ks" .
@prefix cls: <clr:/Common/FfAdmin.Common/NewDonation/> .
docs = {
    cls:Type ff:description "Identifies the event." ;
        ff:value "`DONA_NEW`" .
    cls:Execute_timestamp ff:description "The timestamp the donation should be considered valid. If not present, use `Timestamp`." .
    cls:Donation ff:description "Identifies the donation." .
    cls:Donor ff:description "The identifier for the donor." .
    cls:Charity ff:description "The identifier for the charity." .
    cls:Option ff:description "The identifier for the investment option." .
    cls:Currency ff:description "ISO-4217 currency code." .
    cls:Amount ff:description "The donated amount." .
    cls:Exchanged_amount ff:description "The donated amount in the currency of the investment option." .
    cls:Transaction_reference ff:description "An external reference for the donation transaction." .
    cls:Exchange_reference ff:description "An optional external reference for the exchange transaction." .
}
table = code | documented_event_table(common:NewDonation, docs)
```

## Purpose

This event is essential for tracking donations, handling currency exchange, and maintaining accurate records of donor, charity, and option relationships.
