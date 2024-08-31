---
title: Donor
author: J.W. Morsink
---
# Donor

A donor is a natural person or organization that uses the Give for Good platform to make a [donation](./donation) for a beneficiary [charity](./charity).

A donor uses the [Give for good website](https://giveforgood.world) to make donations and to check on [allocations](./allocation) on the [Donor dashboard](./donor_dashboard).

```plantuml
!include <archimate/Archimate>

Business_Actor(Donor, "Donor")
rectangle Platform as "Give for good platform" {
  Business_Process(Donation, "Make donation")
  Business_Service(DDash, "Donor Dashboard")
}
Donor -u->> Donation
Donor <-u- DDash

url for Donation is [[donation]]
url for DDash is [[donor_dashboard]]
```