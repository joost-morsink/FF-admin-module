---
title: Donation
author: J.W. Morsink
difficulty: medium
"#donate":
    layer: Business
    type: Process
    caption: Make donation
"#select":
    layer: Business
    type: Process
    caption: Select charity
    triggers:
    - to: "#details"
    composes:
    - to: "#donate"
"#details":
    layer: Business
    type: Process
    caption: Enter details
    triggers:
    - to: "#payment"
    composes:
    - to: "#donate"
"#payment":
    layer: Business
    type: Process
    caption: Payment
    triggers:
    - to: "#thanks"
    composes:
    - to: "#donate"
"#thanks":
    layer: Business
    type: Process
    caption: Thank you    
    composes:
    - to: "#donate"

"#charity_repo":
    layer: Application
    type: Service
    caption: Charities
    serves:
    - to: "#select"
"#donating_svc":
    layer: Application
    type: Service
    caption: Donating
    serves:
    - to: "#payment"
---

# Donation

A donation is a monetary contribution made by a [donor](./donor) to a selected [charity](./charity) or [theme](./theme) via a specific [investment option](./option).
Donations may be made in various currencies and are exchanged to the investment option's currency if necessary.

## Donation Process

The donation process consists of several stages:

- **Select charity:** The donor chooses a beneficiary charity.
- **Enter details:** The donor provides donation information and preferences.
- **Payment:** The donor completes the payment transaction.
- **Thank you:** The donor receives confirmation and appreciation for their contribution.

These stages are supported by services for charity selection and donation processing.

```arch(plantuml)
$steps = (#select, #details, #payment, #thanks);
$services = (#charity_repo, #donating_svc);
donor;

> rectangle Web as "giveforgood.world" {
    donor r #donate;
    $steps;
    #donate d $steps;
    $steps d $services;

>    component Wordpress #Application
>    component GiveWp #Application

>    Wordpress .u-|> donation__charity_repo
>    GiveWp .u-|> donation__donating_svc
>    GiveWp -l-> Wordpress : plugin
>    donation__charity_repo <|-. GiveWp

> }
```

## Donation Lifecycle

After a donation is made, it transitions through several states:

- **Registered:** The donation is recorded in the GiveWP system.
- **Unentered:** The donation is imported into the admin module by the [automatic importer](./auto_import).
- **Cancelled:** The donation is marked as cancelled if a cancellation event occurs before entry.
- **Entered:** The donation is processed by an [Enter event](./events/CONV_ENTER) on [conversion day](./conversion_day), making the funds part of the [investment option](./option).

```plantuml
[*] --> Registered : Make donation
Registered --> Unentered : Imported by [[auto_import Auto import]]
Unentered --> Cancelled : [[events/DONA_CANCEL Cancel event]]
Unentered --> Entered : [[events/CONV_ENTER Enter event]] on [[conversion_day Conversion day]]
Cancelled --> [*]
Entered --> [*]
Registered : Data in GiveWP
Unentered : Data in admin module
Cancelled : Donation cancelled before entry
Entered : Monetary funds part of [[option investment option]]
```
