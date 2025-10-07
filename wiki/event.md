---
title: Events
author: J.W. Morsink
difficulty: medium
---

# Events

Events represent discrete changes in the state of the Give for Good administration. 
Each event records what happened and when, enabling a complete and auditable history of all actions and transactions.

All event types share two core properties:

- **type:** The category and specific kind of event
- **timestamp:** The exact moment the event is considered to have occurred

## Ordering

Events are stored in a sequential order, which may differ from strict chronological order. The [Admin Module](./admin_module) supports non-chronological event processing, though chronological ordering is recommended for optimal system behavior.

## Categories

Event types are typically prefixed to indicate their business process category:

| Prefix | Description                                                                             |
| ------ | --------------------------------------------------------------------------------------- |
| META   | Operations on meta data such as [options](./option) and [charities](./charity)          |
| DONA   | Events related to the [donation process](./donation)                                    |
| CONV   | Events for [conversion day](./conversion_day)                                           |

## Types

| Type                                                      | Description                                                  |
| --------------------------------------------------------- | ------------------------------------------------------------ |
| [NONE](./events/NONE)                                     | Technical placeholder event                                  |
| [DONA_NEW](./events/DONA_NEW)                             | A new [donation](./donation) has been made                   |
| [DONA_UPDATE_CHARITY](./events/DONA_UPDATE_CHARITY)       | An existing [donation](./donation) has been reassigned to a new [charity](./charity) |
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
| [CONV_INCREASE_CASH](./events/CONV_INCREASE_CASH)         | A non-donation increase in cash has been registered          |
| [META_CHARITY_PARTITION](./events/META_CHARITY_PARTITION) | A [theme-charity](./theme) is partitioned over other [charities](./charity) |
| [PRICE_INFO](./events/PRICE_INFO)                         | Price information has been registered                        |

Events are the foundation for all state transitions in the platform, supporting auditability, transparency, and reliable model calculations.


