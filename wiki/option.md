---
title: Investment option
author: J.W. Morsink
difficulty: medium
---

# Investment Option

An investment option represents both a real investment fund (or a group of funds managed together) and a container for [donation](./donation) administration.

All donations are assigned to an investment option, which determines how funds are invested and how profits are distributed.
Currently, the platform operates with a single investment option.

Investment options are central to the platform's financial model.
They define the rules for profit allocation, reinvestment, and guaranteed payouts to charities.
Each option is configured with specific [option fractions](./option_fractions) that control how returns are split between Give for Good, charities, and reinvestment.

Investment options also support mechanisms for handling market downturns, ensuring that charities receive a minimum payout even in poor-performing years.
This is managed through the bad year fraction, which guarantees a baseline distribution regardless of investment results.

## Events

- An investment option is created by the [`META_NEW_OPTION`](./events/META_NEW_OPTION) [event](./event).
- The option's [fractions](./option_fractions) for profit distribution can be updated using the [`META_UPDATE_FRACTIONS`](./events/META_UPDATE_FRACTIONS) event.
- Donations and financial operations are affected by [Conversion day](./conversion_day) events, which handle profit allocation and payouts.

This structure allows for flexible management of investments and transparent administration of donations and returns.

