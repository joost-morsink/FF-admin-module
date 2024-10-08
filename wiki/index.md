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
```pumlarch
~motivation#donor [hidden]d- motivation#trust

~motivation#donor|motivation#board|motivation#charities
~motivation#trust|motivation#donations|motivation#income|motivation#roi

~motivation#donor|motivation#board|motivation#charities d motivation#better_place
~motivation#donor|motivation#board|motivation#charities d motivation#trust|motivation#donations|motivation#income|motivation#roi
~motivation#better_place d motivation#trust|motivation#donations|motivation#income|motivation#roi

~motivation#trust|motivation#donations|motivation#income|motivation#roi d motivation#transparancy|motivation#correctness|motivation#donating

```

Motivations are realized as follows:

```pumlarch
~motivation#transparancy|motivation#correctness|motivation#donating|motivation#income|motivation#roi
rectangle Platform as "Give for food donation platform" {
    ~strategy#dashboard|strategy#history|strategy#donating|strategy#payout|strategy#investment u motivation#transparancy|motivation#correctness|motivation#donating|motivation#income|motivation#roi
}

url for Platform is [[platform]]
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

```pumlarch
~roles#donor|roles#web_admin|roles#donation_admin 

~roles#donor|roles#web_admin|roles#donation_admin d donating_process|conversion_day
~donating_process|conversion_day d website|admin_module

~website u roles#web_admin 
```
