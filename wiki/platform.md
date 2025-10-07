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

The Give for Good platform consists of two core components:

- The [Admin module](./admin_module), which manages donations, investments, allocations, and payouts.
- The [Give for Good website](https://www.giveforgood.world/), which provides a user interface for donors and charities.

These components work together to deliver the platform's essential capabilities and business processes, including donation management, investment tracking, payout execution, and historical reporting.

```arch(plantuml)
$capabilities = (strategy#donating, strategy#dashboard, strategy#investment, strategy#payout, strategy#history);
$functions = (website, admin_module);
$procs = (donating_process, investment_process, payout);

$capabilities; $functions; $procs;

$capabilities d $functions;
$functions d $procs;
```

The platform is designed to ensure transparency, efficiency, and trust in the administration and distribution of charitable donations.
It supports long-term investment strategies and reliable payout processes, enabling donors to maximize their impact and charities to receive stable funding.

