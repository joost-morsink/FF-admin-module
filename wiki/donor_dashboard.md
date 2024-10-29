---
title: Donor dashboard
author: J.W. Morsink
archimate: 
    layer: Business
    type: Service
    serves: 
    - to: donor
---

# Donor dashboard

The donor dashboard is a piece of front-end software that aims to give [donor's](./donor) insight in their [donation](./donation), the development of its worth, and the [allocations](./allocation) made to the [charity](./charity).

The motivation for having a donor dashboard is as follows:

```arch(plantuml)
$stakeholders = (motivation#donor);
$drivers = motivation#better_place;
$goals = (motivation#trust, motivation#donations, motivation#income);
$requirements = (motivation#insight, motivation#transparancy);
$strategy = (strategy#dashboard);

$stakeholders; $drivers; $goals; $requirements; $requirements;

$stakeholders d $drivers;
$stakeholders d 2 $goals;
$drivers d $goals;
$goals d $requirements;


>rectangle Platform as "Give for good donation platform" {
    $strategy u $requirements;
>}
website u $strategy;
donor r website;
>donor -u- motivation__donor : stakeholder acts as
>url for Platform is [[platform]]
```