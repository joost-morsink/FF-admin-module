---
title: Model Donations2
author: J.W. Morsink
---


# Donations2 Model

This model tracks all donations made through the platform, including donor, amount, currency, charity, and timestamps.

## Purpose

The Donations model provides a complete, auditable record of all donations, supporting reporting, allocation, and payout processes.

## Structure


Each donation entry contains the following fields:

| Field            | Datatype         | Remarks                                   |
|------------------|------------------|-------------------------------------------|
| Id               | string           | Unique identifier for the donation        |
| Timestamp        | DateTimeOffset   | Time of donation                          |
| ExecuteTimestamp | DateTimeOffset   | Time at which donation is considered made |
| OptionId         | string           | References the investment option          |
| CharityId        | string           | References the charity                    |
| Amount           | Real             | Amount donated (option's currency)        |
| OriginalCurrency | string           | Currency of the original donation         |
| OriginalAmount   | Real             | Amount in the original currency           |

## Relationships

- Linked to: [Donor](../donor), [Charity](../charity), [Allocation](../allocation)
- Used by: [Calculator](../calculator), [Payout](../payout)
- Related events: [DONA_NEW](../events/DONA_NEW), [DONA_UPDATE_CHARITY](../events/DONA_UPDATE_CHARITY), [DONA_CANCEL](../events/DONA_CANCEL)

## Example

| Donation ID | Donor | Charity | Amount | Currency | Timestamp | Status |
|-------------|-------|---------|--------|----------|-----------|--------|
| 12345       | D001  | FF      | 100.00 | EUR      | 2025-09-30T12:00:00Z | completed |

## Implementation Notes

- Donations are immutable once completed; updates are tracked via events.
- Supports multiple currencies and charities.
- Used for reporting, allocation, and payout calculations.

