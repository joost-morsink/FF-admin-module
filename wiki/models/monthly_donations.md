---
title: Model Monthly donations
author: J.W. Morsink
---

# Model Monthly donations

This model records all donations, aggregated by month for each option.

```plantuml
@startyaml
{OptionId}:
  - {Month}:
      MoneyBag: The total donated amount, grouped by original currency.
      Exchanged: The total donated amount, converted to the option's currency.
  - {Month}: ...
  - ...
{OptionId}:
  - {Month}: ...
  - ...
... : ...
@endyaml
```

## Events

The following events affect this model:

* [`DONA_NEW`](../events/DONA_NEW): records a new donation.
* [`DONA_CANCEL`](../events/DONA_CANCEL): removes a donation.

