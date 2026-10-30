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

```val/ks
@include "events/EventTypes.ks" .
_ = code | call check_event_type_descriptions(eventtypedocs)
```

```table/ks
@include "events/EventTypes.ks" .
table = code | event_type_table(eventtypedocs)
```

Events are the foundation for all state transitions in the platform, supporting auditability, transparency, and reliable model calculations.


