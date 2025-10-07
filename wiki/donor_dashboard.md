---
title: Donor dashboard
author: J.W. Morsink
difficulty: easy
archimate: 
    layer: Business
    type: Service
    serves: 
    - to: donor
---

# Donor dashboard

> **Disclaimer:** The donor dashboard is currently under development. 
> This article documents what the dashboard should become.

The donor dashboard is a front-end application designed to provide [donors](./donor) with clear insight into their [donations](./donation), the growth and changes in donation value, and the [allocations](./allocation) distributed to each [charity](./charity).

It aggregates data from the platform's event-sourced models, allowing donors to track the full lifecycle of their contributions from initial donation, through investment growth, to allocations to charities. 

Donors can view historical and current values, see how their donations have impacted different charities, and access transparent records of all transactions and allocations. 

The dashboard supports trust and engagement by making financial flows and outcomes visible, and is served by the [admin module](./admin_module) and [calculator](./calculator) for up-to-date reporting.

Features include:

- Overview of all donations and their current worth
- Detailed allocation history per donation
- Visualization of donation impact and investment growth
- User-friendly interface for exploring donation data

The motivation for having a donor dashboard is as follows:

```arch(plantuml)
$stakeholders = (motivation#donor);
$drivers = motivation#better_place;
$goals = (motivation#trust, motivation#donations, motivation#income);
$requirements = (motivation#insight, motivation#transparency);
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

