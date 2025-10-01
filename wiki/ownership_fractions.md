---
title: Ownership Fractions
author: J.W. Morsink
---

# Ownership Fractions


Ownership fractions determine how much of an [investment option](./option) is owned by each [donation](./donation) or by each [charity](./charity), based on the underlying donations.
Charity ownership is calculated as the aggregate of all donation ownerships for that charity within the option.

## Aggregation of Donation Ownership


Each [donation](./donation) to an [investment option](./option) contributes a fraction of ownership to the selected [charity](./charity).
The total ownership fraction for a charity is the sum of all fractions from donations assigned to that charity for the option.
This aggregation ensures that the distribution of profits into [allocations](./allocation), and eventually [transfers](./transfer) is proportional to the actual donations received by each charity.

## Storage and Representation


Ownership fractions are stored using two values:

- **Share**: The numerator representing the number of ownership units held.
- **Divisor**: The denominator representing the total number of units for the option.

The ownership fraction for a charity (or donation) is calculated as:

$$
    OwnershipFraction = \frac{Share}{Divisor} 
$$

This representation allows for precise fractional ownership, even when donations are made in varying amounts or at different times.
Shares are never recalculated, but donations that enter the option by a [`CONV_ENTER`](./events/CONV_ENTER) event are weighed against the current option's worth to determine the share assigned.

## Example

Suppose an option has a total divisor of 1000 shares.
Charity A receives donations totaling 400 shares, and Charity B receives donations totaling 600 shares.
Their ownership fractions are:

- Charity A:
  $$ \frac{400}{1000} = 0.4 $$
- Charity B: $$ \frac{600}{1000} = 0.6 $$

## Purpose

Ownership fractions are essential for determining how profits, allocations, and transfers are distributed among charities.
They ensure fairness and transparency in the management of investment options.

It is also an essential tool in translating option-wide statistics to [donation](./donation) or [donor](./donor) level.
These translated statistics can then be used on the [Donor dashboard](./donor_dashboard).

