---
title: Donor
author: J.W. Morsink
archimate:
  layer: Business
  type: Actor
  triggers: 
  - to: make_donation
  assigns:
  - to: website
    caption: uses
---
# Donor

A donor is a natural person or organization that uses the Give for Good platform to make a [donation](./donation) for a beneficiary [charity](./charity).

A donor uses the [Give for good website](https://giveforgood.world) to make donations and to check on [allocations](./allocation) on the [Donor dashboard](./donor_dashboard).

```pumlarch
rectangle Platform as "Give for Good platform" {
  ~make_donation
  ~donor_dashboard
}
~donor u make_donation
~donor u donor_dashboard
```
