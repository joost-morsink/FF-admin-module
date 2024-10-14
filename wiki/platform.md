---
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

```arch(plantuml)
$capabilities = (strategy#donating, strategy#dashboard, strategy#investment, strategy#payout, strategy#history);
$functions = (website, admin_module);
$procs = (donating_process, investment_process, payout);

$capabilities; $functions; $procs;

$capabilities d $functions;
$functions d $procs;
```

