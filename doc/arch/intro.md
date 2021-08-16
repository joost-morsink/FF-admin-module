# Abstract

This document describes the architecture and technical design of an administration module for the Future Fund.
Initially, the proposed solution was to administer all financial data in the website's database, but several conditions have changed the preferred architecture to be an highly offine scenario.
The key motivating drivers for this insight are:

* Security, having the administration offline gives potential less attack vectors.

* Simplicity, integrating with Wordpress is something we don't have enough experience with.

* Integrity, keeping the data offline gives us more easy ways to protect data integrity.

* Flexibility, a loosely coupled system is easier to replace.

Although the proposed solution gives us several advantages, we also need to consider the added complexity of communication between the online and offline systems.