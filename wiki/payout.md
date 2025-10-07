---
title: Payout
author: J.W. Morsink
difficulty: easy
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

The payout capability refers to the platform's ability to execute and manage the payout process, ensuring that allocated funds are distributed to charities as intended.

The payout process handles the payment of amounts [allocated](./allocation) to [charities](./charity) and records these payments for deduction from the allocation totals.
Payments are executed through [`CONV_TRANSFER`](./events/CONV_TRANSFER) events, which update the [Amounts to transfer model](./models/amounts_to_transfer) by reducing the outstanding allocation.

## Process Overview

The payout process consists of three main steps:

1. **Determine payouts**: Calculate the amounts to be paid to each charity based on allocations.
2. **Make payments**: Initiate and execute the actual payments to charities.
3. **Register payments**: Record completed payments for audit and reconciliation.

These steps are supported by the calculator, admin UI, and event store services, and interact with banking and payment systems.

```arch(plantuml)
$steps = (#determine, #make, #register);
$services = (calculator, admin_ui, event_store);
$layer4 = (models/amounts_to_transfer, #payment_order, #banking, #transactions);
$artifacts = (#pain, #camt);

payout;
$steps;
payout d $steps;
$steps d ($services, #banking);
$layer4;
$services d $layer4;
$layer4 d $artifacts;
```

## Transfer Timing

Allocated funds may not be transferred every year, depending on the amount and the costs involved in processing payments.
If a transfer is postponed, the unpaid amount is automatically added to the next scheduled transfer, ensuring that charities eventually receive all allocated funds.

This process ensures accurate, auditable, and efficient distribution of funds to charities, while minimizing administrative overhead and transaction costs.
