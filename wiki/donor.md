---
title: Donor
author: J.W. Morsink
archimate:
  layer: Business
  type: Actor
  triggers: 
  - to: make_donation
    to: donation#donate
  assigns:
  - to: website
    caption: uses
---
# Donor

A donor is a natural person or organization that uses the Give for Good platform to make a [donation](./donation) for a beneficiary [charity](./charity).

A donor uses the [Give for good website](https://giveforgood.world) to make donations and to check on [allocations](./allocation) on the [Donor dashboard](./donor_dashboard).

```arch(plantuml)
> rectangle Platform as "Give for Good platform" {
  make_donation;
  donor_dashboard;
> }
donor u (make_donation, donor_dashboard);
```

## Data

The following data should be retrievable from the dashboard:

* An overview of all donations made
* A 'current' worth of the donations
* Paid amounts to charities

Graphical visualisations based on these data should be available as well.
