---
title: Model Aggregated Donations and Transfers
author: J.W. Morsink
---

# Model AggregatedDonationsAndTransfers

This model aggregates all donations and transfers per charity and year.

## Purpose

Provides a summary of donated and transferred amounts for reporting and auditing.

## Structure

Each entry is keyed by a tuple of (Year, Charity).
For each key, the model tracks:

- Donated amounts per currency
- Transferred amounts per currency

```yaml
Year:
  CharityId:
    Donated:
      Currency: Amount
    Transferred:
      Currency: Amount
```

Amounts are stored per currency using the `MoneyBag` type, which maps currency codes to amounts.
Negative amounts are used to represent cancellations or reversals.

## Relationships

- Used by: [Calculator](../calculator)
- Related models: [Donations2](./donations), [Options](./options)
- Updated by donation and transfer events

## Implementation Notes

- Aggregates data for annual reports and charity statements.
- Used for financial transparency and auditability.
- Updated when donations are made, cancelled, or transferred.
