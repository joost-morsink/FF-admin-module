---
title: Shares
author: J.W. Morsink
---

# ${title}

Shares are used to represent a [fraction set](./fraction_set) in a distributed or partitioned manner.
Each share corresponds to a unit of ownership or value within the set.

When shares are only added to the total set, the individual shares themselves remain unchanged; only the divisor (the total number of shares) increases.
The divisor must be updated every time new shares are added to the fraction set, ensuring that fractional ownership calculations remain accurate.

This approach allows for scalable and precise tracking of ownership or value distribution, especially in systems where shares are frequently added but not removed or modified.