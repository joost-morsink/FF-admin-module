---
title: Donation
author: J.W. Morsink
---

# Donation

A donation is a paid monetary amount by some [donor](./donor) in some (probably default) [investment option](./option) for some beneficiary [charity](./charity) or [theme](./theme).
The monetary amount is exchanged to the investment option's currency if needed.

## Making a donation

```pumlarch
~donor

rectangle Web as "giveforgood.world" {
    ~donor r make_donation
}
```

```plantuml
@startuml
!include <archimate/Archimate>

Business_Actor(Donor, "Donor")

rectangle Web as "giveforgood.world" {
    Business_Process(Donate, "Make donation")
    Business_Process(Charity, "Select charity")
    Business_Process(Details, "Enter details")
    Business_Process(Pay, "Payment")
    Business_Process(Thank, "Thank you")

    Donor ->> Donate
    Donate *-- Charity
    Donate *-- Details
    Donate *-- Pay
    Donate *-- Thank
    Charity ->> Details
    Details ->> Pay
    Pay ->> Thank

    Application_Service(CharityRepo, "Charities")
    Application_Service(DonationRepo, "Donating")

    CharityRepo -u-> Charity
    DonationRepo -u-> Pay

    component Wordpress #Application
    component GiveWp #Application

    Wordpress .u-|> CharityRepo
    GiveWp .u-|> DonationRepo
    GiveWp -l-> Wordpress : plugin
    CharityRepo <|-. GiveWp
}

url for Web is [[https://giveforgood.world]]
url for Donor is [[donor]]

@enduml
```

When a donation is made, it goes through several stages:

```plantuml
[*] --> Registered : Make donation
Registered --> Unentered : Imported by [[auto-import Auto import]]
Unentered --> Entered : [[events/CONV_ENTER Enter event]] on [[conversion_day Conversion day]]
Entered --> [*]
Registered : Data in GiveWP
Unentered : Data in admin module
Entered : Monetary funds part of [[option investment option]]
```
