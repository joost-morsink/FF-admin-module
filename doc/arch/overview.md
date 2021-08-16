# Overview

The solution architecture consists of an online and an offline system.
A physical actor, called the _administrator_, has access to both systems and can use them to synchronize data between these systems.

![Overview](./images/overview.svg)

## Purpose

### Online database

The purpose of the online database is to:

* administer payments and (chosen) charities;

* display statistics about investment results and actual donations.

### Offline database

The purpose of the offline database is to:

* import payments;

* export financial data for usage in the online database;

* administer investments;

* perform calculations based on investments and withdrawals;

* administer actual donations;

* provide a datasource to reinitialize the database from scratch.

## Requirements 

### Online

We need an export of donations, and import of reporting data for display purposes on the website.

Although we also need a list of charities and investment options on the offline site, we do not require those data to be synchronized via an automated process.

### Offline

We need to have an auditable trail of all financial transactions and balances from day 0.

We need to be able to reliably calculate and administer all the transactions we need to make in order to run the Future Fund business case.
We break this down into the following categories:

* Administration of new donations, charities and investment options.

* Conversion day calculations and administration.

    * The first conversion day logic should pertain to the investing of newly donated money into investment options.

    * The second conversion day logic should pertain to the withdrawal of money from the funds with the goal of making a gift to the selected charities.
