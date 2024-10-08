---
title: Charity
author: J.W. Morsink
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

A charity is a non-profit entity that has a purpose of making a certain aspect of the world better.
Charities are the beneficiary of [payouts](./payout) initiated by [Give for Good](./index).

```pumlarch
~make_donation r make_donation#select_charity
~charity d make_donation#select_charity
~charity d payout
```
