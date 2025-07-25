---
title: Complete History
author: J.W. Morsink
difficulty: medium
---

# ${title}

Complete history is a capability that realizes a complete and auditable adminstration of funds and enhances transparancy.

```arch(plantuml)
strategy#history u (motivation#transparancy, motivation#correctness);
strategy#history d admin_module;
```

Complete history is implemented by using [event](./event) sourcing and deriving all meaningful state from this event stream.
This event stream is stored in the [event store](./event_store) and used by the [calculator](./calculator).

