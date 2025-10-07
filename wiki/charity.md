---
title: Charity
author: J.W. Morsink
difficulty: easy
archimate:
    layer: Business
    type: Role
    relates:
    - to: select_charity_process
      caption: Select beneficiary charity
    - to: payout
      caption: "[[allocation Allocations]] paid out to"
---

# Charity

A charity is a non-profit organization dedicated to improving a specific aspect of society or the environment.
Charities serve as the beneficiaries of [payouts](./payout) distributed by [Give for Good](./index), receiving funds allocated through donations and investment returns.

Charities are selected by [donors](./donor) during the [donation](./donation) process and are eligible to receive [allocations](./allocation) based on their share in the platform's investment options.

Charities are registered in the platform and may be added automatically when a donation is made to a new charity, as described in the [automatic importer](./auto_import).

Allocations to charities are determined by the [allocation](./allocation) process, which calculates each charity's share of investment returns at exit events.
Transfers to charities may be postponed and aggregated until minimum thresholds are met, as explained in the [transfer](./transfer) documentation.

Charities can be viewed and managed through the admin module, and their historical allocations and transfers are available for auditing and reporting.

```arch(plantuml)
make_donation r make_donation#select_charity;
charity d (make_donation#select_charity, payout);
```

