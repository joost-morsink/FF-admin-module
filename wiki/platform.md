---
title: Platform
author: J.W. Morsink
difficulty: easy
"#investment":
    layer: Business
    type: Process
    caption: Investment
    relates:
    - to: admin_module
    influences:
    - to: payout
      caption: long-term
---
# Platform

The Give for Good platform is composed of two main parts:

* The [Admin module](./admin_module)
* The [Give for Good website](https://www.giveforgood.world/)

Together they implement the necessary capabilities and business processes:

```arch(plantuml)
$capabilities = (strategy#donating, strategy#dashboard, strategy#investment, strategy#payout, strategy#history);
$functions = (website, admin_module);
$procs = (donating_process, investment_process, payout);

$capabilities; $functions; $procs;

$capabilities d $functions;
$functions d $procs;
```

