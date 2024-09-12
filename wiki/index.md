---
title: Give for Good
author: J.W. Morsink
---
# Give for Good 

Welcome to the documentation wiki for Give for Good.
Described here are the architecture and business processes that pertain to Give for Good.

## Organizational motivation for a platform

Give for good is a non-profit organization that allow donors to donate money indirectly to charities.
The donated money is first invested in green/sustainable stock funds.
A part of the profits is donated to the selected charities anually, a small part is used to support the platform, and the rest is used for reinvestment.

```plantuml
@startuml
!include <archimate/Archimate>
sprite $sCap jar:archimate/strategy-capability

rectangle Platform  as "Give for good donation platform" {
    Strategy_Capability(CapDonating, "Donating")
    Strategy_Capability(CapDashboard, "Donor dashboard")
    Strategy_Capability(CapInvest, "Investing")
    Strategy_Capability(CapPayout, "Payout")
    Strategy_Capability(CapHistory, "Complete history")
} 

Motivation_Driver(Better, "Make the world \na better place")
Motivation_Goal(Income, "Recurrent income \nfor charities")
Motivation_Requirement(ReqDonating, "Donors can make donations")
Motivation_Requirement(Correctness, "Correct, auditable \nadministration of funds")
Motivation_Requirement(Transparancy, "Transparant software \nand processes")
Motivation_Outcome(Donations, "More donations for charities \nthrough the platform")
Motivation_Outcome(Trust, "Increase donors' trust")
Motivation_Outcome(Roi, "Return on investment")

Motivation_Stakeholder(Charities, "Charities")
Motivation_Stakeholder(Board, "Give for good board")
Motivation_Stakeholder(Donor, "Donors")

CapHistory .-u-|> Correctness
CapHistory .-u-|> Transparancy
CapDashboard .-|> Transparancy
CapInvest .-u--|> Roi
CapPayout .-|> Income
CapDonating .-u-|> ReqDonating
ReqDonating -u-> Donations
Charities -- Income
Donations .> Income : long-term +
Income <. Roi : +
Donor <-. Trust : +
Donor --- Donations
Board --- Donations
Board --- Trust
Trust .> Donations : +
Trust <|-. Correctness
Trust <|-. Transparancy

Better <|-. Income
Board -- Better
Charities -- Better
Donor -- Better

url for Charities is [[charity]]
url for Donor is [[donor]]
url for Platform is [[platform]]
@enduml
```


## Stakeholders

We identify the following stakeholders:

* Give for good board
* [Charities](./charity)
* [Donors](./donor)

## Drivers

To achieve the necessary trustworthiness of a good cause, we have to have an open and correct administration of everyone's donations.

### Correctness

There is a strict requirement for a correct administration. 
If funds go missing, it will affect our trustworthiness negatively.

### Transparancy

Being transparant in process, software and data convinces people of Give for Good's trustworthiness.

## User roles

```plantuml
!include <archimate/Archimate>

Business_Role(Donor, "Donor")
Business_Role(WebAdmin, "Website administrator")
Business_Role(Admin, "Donations \nadministrator")

Business_Process(CDay, "Conversion day")
Business_Process(Donating, "Donating")

Donor -->> Donating
Admin -->> CDay

Business_Service(AdminModule, "Admin module")
Business_Service(Web, "giveforgood.world")

CDay <-- AdminModule
Donating <-- Web
WebAdmin <--- Web

url for Donor is [[donor]]
url for Donating is [[donation]]
url for CDay is [[conversion_day]]
url for AdminModule is [[admin_module]]
```
