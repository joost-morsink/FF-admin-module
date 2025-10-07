---
title: Donor
author: J.W. Morsink
difficulty: easy
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

A [donor](./donor) is an individual or organization who contributes funds to beneficiary [charities](./charity) through the Give for Good platform.

Donors interact with the platform via the [Give for Good website](https://giveforgood.world), where they can make [donations](./donation), monitor the progress and impact of their contributions, and review [allocations](./allocation) using the [Donor dashboard](./donor_dashboard).

```arch(plantuml)
> rectangle Platform as "Give for Good platform" {
  make_donation;
  donor_dashboard;
> }
donor u (make_donation, donor_dashboard);
```

## Dashboard Data

The [donor dashboard](./donor_dashboard) provides access to:

- A complete overview of all donations made
- The current value of each donation
- Records of amounts paid out to charities
- Visualizations and reports to help donors understand the impact of their contributions
