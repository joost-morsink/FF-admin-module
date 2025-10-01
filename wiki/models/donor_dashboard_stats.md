---
title: Donor Dashboard Stats
author: J.W. Morsink
archimate:
  layer: Application
  type: DataObject
  caption: Donor dashboard statistics
---


# Donor Dashboard Stats Model

This model provides a per-donation breakdown for a donor, as displayed on the donor dashboard.
For each donation, it aggregates donation details and related records (such as worth, allocations, and history).

## Purpose

The Donor Dashboard Stats model enables donors to view detailed information about each of their donations, including financial development, allocations, and charity impact.
Summary statistics (such as totals and counts) are derived from the per-donation data.

## Structure

The main structure is:

- `Donations`: Dictionary mapping Donations to `StatDetail`s

Each `StatDetail` contains:

| Field         | Datatype                | Remarks                                 |
|---------------|-------------------------|-----------------------------------------|
| Donation      | Donation object         | All donation fields                     |
| Records       | List of DonationRecord2 | History of worth, allocations, etc.     |

For dashboard display, the following per-donation fields are shown:

| Field      | Datatype         | Remarks                                 |
|------------|------------------|-----------------------------------------|
| Donated    | decimal          | Amount donated                          |
| Worth      | decimal          | Current worth of the donation           |
| Allocated  | decimal          | Total allocated to charities            |
| Profit     | decimal          | Worth + Allocated - Donated             |
| Currency   | string           | ISO-4217 currency code                  |
| Charity    | string           | Charity name or ID                      |
| Timestamp  | DateTimeOffset   | Time of the worth measurement           |

## Relationships

- Aggregates data from: [Donations](./donations), [Donation Records](./donation_records), [Allocations](./allocation), [Transfers](./transfer), [Payouts](./payout)
- Used by: [Donor Dashboard](../donor_dashboard)

## Example

| Donation ID | Donated | Worth   | Allocated | Profit | Currency | Charity | Timestamp              |
|-------------|---------|---------|-----------|--------|----------|---------|------------------------|
| 12345       | 100.00  | 120.00  | 15.00     | 35.00  | EUR      | FF      | 2025-09-30T12:00:00Z   |

## Implementation Notes

- Summary statistics (totals, counts, last donation date) are derived from the per-donation data.
- Calculated on demand for dashboard display.
- May be cached for performance.
- Supports multi-currency reporting if needed.


