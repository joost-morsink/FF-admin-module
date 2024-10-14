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

```arch(plantuml)
$stakeholders = (motivation#donor, motivation#board, motivation#charities);
$drivers = motivation#better_place;
$goals = (motivation#trust, motivation#donations, motivation#income, motivation#roi);
$requirements = (motivation#transparancy, motivation#correctness, motivation#donating);

$stakeholders; $drivers; $goals; $requirements;

$stakeholders d $drivers;
$stakeholders d 2 $goals;
$drivers d $goals;
$goals d $requirements;

> motivation__donor -[hidden]d-- motivation__trust
```

Motivations are realized as follows:

```arch(plantuml)
$motivation = (motivation#transparancy, motivation#correctness, motivation#donating, motivation#income, motivation#roi);
$strategy = (strategy#dashboard, strategy#history, strategy#donating, strategy#payout, strategy#investment);

$motivation;
>rectangle Platform as "Give for good donation platform" {
    $strategy u $motivation;
>}
>url for Platform is [[platform]]
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

```arch(plantuml)
$roles = (roles#donor, roles#web_admin, roles#donation_admin);
$processes = (donating_process, conversion_day);
$sites = (website, admin_module);

$roles;

$roles d $processes;
$processes d $sites;
website u 2 roles#web_admin;
```
