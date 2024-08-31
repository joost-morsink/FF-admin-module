---
title: Model Donations
author: J.W. Morsink
---

# Model Donations

This model keeps track of all donations that have been made.

```plantuml
@startyaml
Id: The id of the donation
Timestamp: The time of donation
ExecuteTimestamp: The time at which the donation may be considered made.
OptionId: The id of the investment option
CharityId: The id of the charity
Amount: The amount of money donated, in the option's currency.
@endtyaml
```

Information about the [investment option](../option) and [charity](../charity) are also recorded.

## Events

Events that affect this model are:

* [`DONA_NEW`](../events/DONA_NEW.md) records a new donation.
* [`DONA_CANCEL`](../events/DONA_CANCEL) removes a donation.
* [`DONA_UPDATE_CHARITY`](../events/DONA_UPDATE_CHARITY) updates the charity id for the donation.

