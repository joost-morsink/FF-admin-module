---
title: Fraction set
author: J.W. Morsink
difficulty: medium
---

# Fraction Set

A fraction set is a data structure that represents the proportional shares of components within a whole. 
Each fraction indicates the relative size of a part, and all fractions together must sum to 1.

Whenever entries are added or updated, the fraction set is automatically renormalized to maintain this invariant. 
To prevent cumulative errors from repeated division and rounding, a `Divisor` is maintained, ensuring numerical stability.

## Partitioned Models

In [partitioned models](./calculator#partitioning), fraction sets are split between the header (storing the `Divisor`) and the details (storing the individual fractions, scaled by the `Divisor`). 
This design supports efficient storage and accurate calculations across distributed model partitions.
The fraction multiplied by the `Divisor` is called a `Share`.
