---
title: Automatic importer
author: J.W. Morsink
difficulty: medium
archimate:
    layer: Application
    type: Process
---

# Automatic importer

The automatic importer is a component, implemented as an Azure function, that retrieves the donations made from the [giveforgood.world](https://giveforgood.world) website on a daily basis and imports them into the [admin module](./admin_module).

## Logic

For every donation the following is flowchart is followed:

```mermaid
flowchart TD
    FAPD[Fetch all paid donations] --> DR
    DR{Donation registered?} -->|No| CE{Charity exists?}
    CE -->|No| META_NEW_CHARITY --> CO{Currency = Option's currency?}
    CE -->|Yes| CO
    CO -->|Yes| DONA_NEW
    CO -->|No| PER{Payment info has exchange rate?}
    PER --->|No| F((Import failed))
    PER -->|Yes| DONA_NEW
    DR ------->|Yes| SK((Import skipped))
    DONA_NEW --> S((Import successful))

    click DONA_NEW "./events/DONA_NEW"
    click META_NEW_CHARITY "./events/META_NEW_CHARITY"
```

## Scheduling

A daily import process is scheduled that takes into account the last three days.
This way, when a job occasionally fails, the donations are imported a few days later.
Also a monthly import is scheduled to sweep anything that still hasn't been synchronized to the admin module.

Before each [enter](./conversion_day#the-in-process) the run state of these processes should be checked, to make sure all donations are taken into account.