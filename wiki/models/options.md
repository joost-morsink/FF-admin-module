--- 
title: Model Options
author: J.W. Morsink
---
# Model Options

The Options model keeps track of all the registered [options](../option), that have been registered by the [META_NEW_OPTION](../events/META_NEW_OPTION).

It keeps track of the following attributes for each option:

```plantuml
@startyaml
Id: The id for the option (alphanumeric, default 1)
Name: The name of the option
Currency: The ISO currency code for the option
CharityFraction: The fraction of profits that are payed out to charities
ReinvestmentFraction: The fraction of profits that is reinvested
G4gFraction: The fraction of the profits that is payed out to Give for Good
BadYearFraction: The fraction that is guaranteed to be payed out every year
@endyaml
```

