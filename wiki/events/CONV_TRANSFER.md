---
title: CONV_TRANSFER
author: J.W. Morsink
about: clr:/Common/FfAdmin.Common/ConvTransfer
---

# CONV_TRANSFER

```val/ks
@include "Module.ks" .
_ = code | call check_event(eventtype:CONV_TRANSFER, common:ConvTransfer)
```

The `CONV_TRANSFER` event represents the actual transfer of allocated funds to a [charity](../charity).
It finalizes the payout process by moving money from the system to the charity.
Currency exchange may be involved if the charity receives funds in a different currency.

## Fields

```table/ks
@include "Module.ks" .
@prefix cls: <clr:/Common/FfAdmin.Common/ConvTransfer/> .
docs = {
    cls:Type ff:description "Identifies the event." ;
        ff:value "`CONV_TRANSFER`" .
    cls:Charity ff:description "The identifier of the charity." .
    cls:Currency ff:description "ISO-4217 currency code of the original amount." .
    cls:Amount ff:description "The amount transferred to the charity in the original currency." .
    cls:Exchanged_currency ff:description "ISO-4217 currency code after exchange (charity's currency)." .
    cls:Exchanged_amount ff:description "The amount transferred in the exchanged currency." .
    cls:Transaction_reference ff:description "External reference code for the transaction." .
    cls:Exchange_reference ff:description "Optional external reference for the exchange." .
}
table = code | documented_event_table(common:ConvTransfer, docs)
```

## Purpose

This event is essential for tracking the final movement of funds to charities.
It ensures accurate records of all transfers, including those involving currency exchange.
