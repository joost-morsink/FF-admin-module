---
title: Model Monthly donations
author: J.W. Morsink
---

# Model Monthly donations

This model keeps track of all donations that have been made, aggregated per month.

```plantuml
@startyaml

{OptionId}:
  - {Month}:
      MoneyBag: The amount of donated money grouped by original currency.
      Exchanged: The exchanged amount of donated money in the option's currency.
  - {Month}: ...
  - ...
{OptionId}:
  - {Month}: ...
  - ...
... : ...

@endyaml
```

## Events

Events that affect this model are:

* [`DONA_NEW`](../events/DONA_NEW.md) records a new donation.
* [`DONA_CANCEL`](../events/DONA_CANCEL) removes a donation.

