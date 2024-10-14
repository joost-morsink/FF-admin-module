---
title: Payout
author: J.W. Morsink
archimate: 
    layer: Business
    type: Process
"#determine":
    layer: Business
    type: Process
    caption: Determine payouts
    composes: 
    - to: payout
    triggers:
    - to: "#make"
"#make":
    layer: Business
    type: Process
    caption: Make payments
    composes:
    - to: payout
    triggers:
    - to: "#register"
"#register":
    layer: Business
    type: Process
    caption: Register charity payments
    composes:
    - to: payout
"#banking":
    layer: Business
    type: Function
    caption: Banking function \nexternal
    serves:
    - to: "#make"
    accesses:
    - to: "#transactions"
"#att":
    layer: Application
    type: DataObject
    caption: Amounts to transfer
    url: models/amounts_to_transfer
    accesses:
    - to: calculator
"#payment_order":
    layer: Application
    type: DataObject
    caption: "Payment Order"
    accesses:
    - to: "#banking"
"#pain":
    layer: Technology
    type: Artifact
    caption: Pain file
    realizes: 
    - to: "#payment_order"
"#transactions":
    layer: Application
    type: DataObject
    caption: Transaction data
    accesses:
    - to: event_store
"#camt":
    layer: Technology
    type: Artifact
    caption: Camt file
    realizes: 
    - to: "#transactions"
---

# Payout

The payout process
```arch(plantuml)
$steps = (#determine, #make, #register);
$services = (calculator, admin_ui, event_store);
$layer4 = (#att, #payment_order, #banking, #transactions);
$artifacts = (#pain, #camt);

payout;
$steps;
payout d $steps;
$steps d ($services, #banking);
$layer4;
$services d $layer4;
$layer4 d $artifacts;
```


