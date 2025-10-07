---
title: Model Charities
author: J.W. Morsink
---

# Model Charities

The charities model represents all charitable organizations registered on the platform.
It stores essential information about each charity, including identification and banking details.

## Structure

```plantuml
@startyaml
Charity:
  id: Unique identifier for the charity (string)
  name: Name of the charity (string)
  bank:
    name: Name of the bank (string)
    account: Bank account number (string)
    bic: Bank identification code (string)
  fractions: Fractional ownerships for the charity (mapping from holder to fraction, optional)
@endyaml
```

## Purpose

The charities model enables:

- Management and administration of charity records
- Linking donations and allocations to specific charities
- Supporting reporting and transparency for donors and platform administrators

