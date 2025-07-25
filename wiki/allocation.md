---
title: Allocation
author: J.W. Morsink
difficulty: easy
---

# Allocation

Allocation is the assignment of funds, based on profits, to be paid out to [charities](./charity). 
It is determined based on the exit amount specified in the [CONV_EXIT](./events/CONV_EXIT) event and the fraction of ownership that charities have of the [investment option](./option).

When funds are allocated to a charity, they are eligible for [transfer](./transfer) to the actual charities.
This may be postponed because of the amount being too low or the costs of transfer being too high.
When it is postponed, it is added to the allocated amount of the next [conversion day](./conversion_day).
