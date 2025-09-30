--- 
title: CONV_ENTER
author: J.W. Morsink
difficulty: medium
---

# CONV_ENTER

The `CONV_ENTER` event marks the moment when all new [donations](../donation) are incorporated into an [investment option](../option) as cash.
Updating the invested amount is essential for processing the event correctly because it redefines [ownership fractions](../ownership_fractions) for the option.


## Fields

| Field             | Type                | Description                                                                                                        | Value        |
|-------------------|---------------------|--------------------------------------------------------------------------------------------------------------------|--------------|
| `Type`            | string              | Identifies the event                                                                                               | `CONV_ENTER` |
| `Timestamp`       | DateTime (ISO-8601) | The timestamp of the event                                                                                         |              |
| `Option`          | string              | The identifier for the investment option                                                                           |              |
| `Invested_amount` | decimal(20,4)       | The total invested amount of money in the investment option                                                        |              |

## Purpose

This event ensures that all donations are properly accounted for in the option and that ownership fractions are updated accordingly.
It is a key step in the lifecycle of an investment option.
