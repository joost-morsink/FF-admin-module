---
title: Option fractions
author: J.W. Morsink
---

# Option fractions

Each [investment option](./option) is assigned:

* a triple of fractions to determine how profits are divided, called the option fractions.
* a fraction that represents the guaranteed annual payout, called the bad year fraction.

## The option fractions

Each option needs to divide profits over 3 destinations:

* Give for good (`G4gFraction`)
* Charities (`CharityFraction`)
* Reinvestment in the investment fund (`ReinvestmentFraction`)

These three fractions are validated (by the [`ValidationErrors` model](./models/validation_errors)) to add up to 1:

$$ 
f_{G4g} + f_{Charity} + f_{Reinvestment} = 1 
$$

The fractions are set upon creation of the investment option with the [`META_NEW_OPTION`](./events/META_NEW_OPTION) event and can be modified by the [`META_UPDATE_FRACTIONS`](./events/META_UPDATE_FRACTIONS).

## Bad year fraction

Sometimes the stock market does not perform well over a period of time, but we still want to provide [charities](./charity) with an income. 
We use the bad year fraction to indicate the minimal part of an [investment option's](./option) funds that is payed out to charities on a yearly basis.
This percentage is used to calculate the exact percentage based on the date since the last [exit](./conversion_day#the-out-process).
