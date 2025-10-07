---
title: Allocation
author: J.W. Morsink
difficulty: easy
---

# Allocation

Allocation refers to the process of assigning funds to [charities](./charity) based on their ownership share in an [investment option](./option) and the profits realized at exit.

Funds are allocated according to the exit amount specified in the [`CONV_EXIT`](./events/CONV_EXIT) event, proportionally to each charity's ownership fraction.

Once allocated, these funds become eligible for [transfer](./transfer) to the respective charities. 
However, transfers may be postponed if the allocated amount is too small or if transfer costs are disproportionately high. In such cases, the allocation is carried forward and added to the next [conversion day](./conversion_day).

This mechanism ensures that charities receive their fair share of profits while optimizing for cost-effective transfers.

## Technical Details

- Allocations are calculated per charity for each investment option at the time of exit.
- The ownership fraction is determined by the shares or fractions registered for each charity in the option.
- Allocated amounts are tracked and aggregated until they meet the minimum threshold for transfer.
- Allocation events are recorded in the event store, supporting auditability and historical analysis.
- The allocation process is integrated with the payout and transfer processes to ensure timely and accurate distribution of funds.
