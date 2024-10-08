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


```pumlarch
~website|admin_module
~donating_process|investment_process|payout

~strategy#donating|strategy#dashboard|strategy#investment|strategy#payout|strategy#history d website|admin_module
~website|admin_module d donating_process|investment_process|payout

```
