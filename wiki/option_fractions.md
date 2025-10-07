---
title: Option Fractions
author: J.W. Morsink
difficulty: medium
---

# Option Fractions

Each [investment option](./option) is assigned a set of fractions that determine how profits are distributed and how guaranteed payouts are handled.

## Profit Distribution Fractions

Every investment option divides profits among three destinations:

* Give for Good (`G4gFraction`)
* Charities (`CharityFraction`)
* Reinvestment in the investment fund (`ReinvestmentFraction`)

These fractions are collectively known as the option fractions.
They must always sum to 1:

$$
f_{G4g} + f_{Charity} + f_{Reinvestment} = 1
$$

Fractions are set when the investment option is created using the [`META_NEW_OPTION`](./events/META_NEW_OPTION) event, and can be updated later with the [`META_UPDATE_FRACTIONS`](./events/META_UPDATE_FRACTIONS).
Validation is performed by the [`ValidationErrors` model](./models/validation_errors) to ensure correctness.

## Bad Year Fraction

The bad year fraction represents the minimum annual payout to [charities](./charity), even if the stock market performs poorly.
It guarantees that a certain percentage of an [investment option's](./option) funds will be paid out to charities each year.
The exact payout percentage is calculated based on the time elapsed since the last [exit](./conversion_day#cash-out-process).

This mechanism ensures that charities receive a stable income, regardless of market fluctuations.
