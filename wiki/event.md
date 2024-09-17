---
title: Give for Good - Events
author: J.W. Morsink
---

# Events

There are different types of events that can happen and alter the state of the Give for Good administration.
All the event types have 2 properties in common:

* type
* timestamp

The first is an enumeration of the different types of events and the other is the timestamp of the exact moment the event is supposed to have happened.

## Ordering

The events are ordered sequentially, but not necessarily chronologically.
This means the [Admin Module](./admin_module) supports non-chronological event processing.

## Categories

Almost all event types are prefixed with a string indicating the category to which the event belongs, roughly translating to the business processes that are supported by the application.

| Prefix | Description                                                                             |
| ------ | --------------------------------------------------------------------------------------- |
| META   | Operations on meta data suchs as [options](./option) and [charities](./charity)         |
| DONA   | Events for [donation processes](./donation)                                             |
| CONV   | Events for [conversion day](./conversion_day)                                           |

## Types

| Type                                                      | Description                                                  |
| --------------------------------------------------------- | ------------------------------------------------------------ |
| [NONE](./events/NONE)                                     | Dummy event needed for technical reasons                     |
| [DONA_NEW](./events/DONA_NEW)                             | A new [donation](./donation) has been made                   |
| [DONA_UPDATE_CHARITY](./events/DONA_UPDATE_CHARITY)       | An existing [donation](.donation) has been reassigned to a new [charity](./charity) |
| [META_NEW_OPTION](./events/META_NEW_OPTION)               | A new [option](./option) has been registered                 |
| [META_NEW_CHARITY](./events/META_NEW_CHARITY)             | A new [charity](./charity) has been registered               |
| [META_UPDATE_FRACTIONS](./events/META_UPDATE_FRACTIONS)   | The [fractions](./option_fractions) of an [option](./option) have changed |
| [CONV_LIQUIDATE](./events/CONV_LIQUIDATE)                 | Stocks have been liquidated                                  |
| [CONV_EXIT](./events/CONV_EXIT)                           | Funds have exited, and should be [allocated](./allocation)   |
| [CONV_TRANSFER](./events/CONV_TRANSFER)                   | A [transfer](./transfer) of funds to a charity has been made |
| [CONV_ENTER](./events/CONV_ENTER)                         | Eligible [donations](./donation) have entered                |
| [CONV_INVEST](./events/CONV_INVEST)                       | Funds have been invested in stocks                           |
| [CONV_INFLATION](./events/CONV_INFLATION)                 | A correction for inflation has been requested                |
| [AUDIT](./events/AUDIT)                                   | An audit report has been consolidated                        |
| [DONA_CANCEL](./events/DONA_CANCEL)                       | A [donation](./donation) has been cancelled                  |
| [META_UPDATE_CHARITY](./events/META_UPDATE_CHARITY)       | A [charity](./charity) has been updated                      |
| [CONV_INCREASE_CASH](./events/CONV_INCREASE_CASH)         | An non-donation increase in cash has been registered         |
| [META_CHARITY_PARTITION](./events/META_CHARITY_PARTITION) | A [theme-charity](./theme) is partitioned over other [charities](./charity) |
| [PRICE_INFO](./events/PRICE_INFO)                         | A price information is registered                            |


