--- 
title: Theme
author: J.W. Morsink
difficulty: easy
---

# ${title}

A theme is a weighted group of [charities](./charity) that can be selected as the beneficiary of a [donation](./donation).
When a [donor](./donor) makes a donation to a theme, the donation is distributed among the charities in the theme according to their assigned weights.

The [allocations](./allocation) for donations made to a theme are divided among the selected charities, reflecting the theme's current composition.
If the selection of charities within a theme changes, the ultimate beneficiaries of all donations made to that theme will also change accordingly.

Theme composition can be updated by the [`META_CHARITY_PARTITION`](./events/META_CHARITY_PARTITION) event, which is managed in the [Admin Module](./admin_module).

This flexible structure allows donors to support groups of charities with a single donation, while enabling dynamic adjustment of charity selections and weights over time.

