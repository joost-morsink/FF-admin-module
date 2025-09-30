--- 
title: META_CHARITY_PARTITION
author: J.W. Morsink
---

# META_CHARITY_PARTITION

The `META_CHARITY_PARTITION` event turns a [charity](../charity) into a ['donation theme'](../theme) by specifying how donations to the theme are distributed among other charities.
If the partitions are empty or contain only a self-reference, the theme is restored as a regular charity.

## Fields

| Field           | Type               | Description                                   | Value                    |
|-----------------|--------------------|-----------------------------------------------|--------------------------|
| `Type`          | string             | Identifies the event.                         | `META_CHARITY_PARTITION` |
| `Timestamp`     | DateTime(ISO-8601) | The time of the event.                        |                          |
| `Code`          | string             | Identifies the donation theme.                |                          |
| `Partitions`    | (string, decimal)  | The distribution of the theme among charities.|                          |

## Purpose

This event allows flexible grouping of charities under a theme, enabling donations to be split according to specified partitions.
It supports both composite themes and reverting to a single charity.
