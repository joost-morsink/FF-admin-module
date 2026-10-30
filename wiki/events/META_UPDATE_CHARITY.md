--- 
title: META_UPDATE_CHARITY
author: J.W. Morsink
about: clr:/Common/FfAdmin.Common/UpdateCharity
---
# META_UPDATE_CHARITY

```val/ks
@include "Module.ks" .
_ = code | call check_event(eventtype:META_UPDATE_CHARITY, common:UpdateCharity)
```

The `META_UPDATE_CHARITY` event updates information for an existing [charity](../charity).
Only the fields supplied in the event are updated; unspecified fields remain unchanged.
Typical updates include bank account details and charity name.

## Fields

```table/ks
@include "Module.ks" .
@prefix cls: <clr:/Common/FfAdmin.Common/UpdateCharity/> .
docs = {
    cls:Type ff:description "Identifies the event." ;
        ff:value "`META_UPDATE_CHARITY`" .
    cls:Code ff:description "Identifies the charity." .
    cls:Name ff:description "The name of the charity." .
    cls:Bank_account_no ff:description "The charity's bank account number." .
    cls:Bank_name ff:description "The charity's name as registered by the bank." .
    cls:Bank_bic ff:description "The charity's bank identification code." .
}
table = code | documented_event_table(common:UpdateCharity, docs)
```

## Purpose

This event is essential for keeping charity records up to date, especially for financial transactions and compliance.