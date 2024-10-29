---
title: Automatic importer
author: J.W. Morsink
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
