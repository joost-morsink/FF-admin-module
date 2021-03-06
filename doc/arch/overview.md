# Overview

The solution architecture consists of an online (website) and an offline (admin module) system.
A physical actor, called the _administrator_, has access to both systems and can use them to synchronize data between these systems.
This synchronization is manual, and the administrator is responsible for the execution of the synchronization.

![Overview](./images/overview.svg)

## Purpose

### Online database

The purpose of the online database is to:

* administer donations and (chosen) charities;

* display statistics about investment results and actual transfers to charity;

* keep track of individual donors and donations.

This database contains some personal data, all of which is entered by the donor. 
The website should have measures in place to comply with the GDPR.

### Offline database

The purpose of the offline database is to:

* import donations;

* export statistical data for usage in the online database;

* administer investments;

* perform calculations based on investments and liquidations;

* administer money transfers to charities;

* provide a datasource to reinitialize the database from scratch.

This database only contains identifiers, which can be correlated to the user accounts on the website.
No GDPR measures need to be taken here.

## Requirements

### Online

We need an export of donations, and an import of reporting data for display purposes on the website.  
It should have on a donation-level information about 

* the current invested value of the donation;

* money transferred to charity based on the profits/interest on the donation.

Although we also need a list of charities and investment options on the offline site, we do not require those data to be synchronized via an automated process.

### Offline

We need to have an auditable trail of all financial transactions and balances from day 0.

We need to be able to reliably calculate and administer all the transactions we need to make in order to run the Future Fund business case.
We break this down into the following categories:

* Administration of new donations, charities and investment options.
  Charities can be detected from the import, but need more information, investment options should be added manually. 
  Both operations should be supported by the application.

* Conversion day calculations and administration.
  
  * The first conversion day logic should pertain to the liquidation of stocks from the investment funds with the ultimate goal of making a money transfer to the selected charities.
  
  * The second conversion day logic should pertain to the allocation of newly donated money into investment options, and consequently investing the money in the investment funds.
    Theoretically this part can be skipped.
