# Introduction

## Abstract

This document describes the architecture and technical design of an administration module for the Future Fund.
Initially, the proposed solution was to administer all financial data in the website's database, but several conditions have changed the preferred architecture to be an highly offine scenario.
The key motivating drivers for this insight are:

* Security, having the administration offline gives potential less attack vectors.

* Simplicity, integrating with Wordpress is something we don't have enough experience with.

* Integrity, keeping the data offline gives us more easy ways to protect data integrity.

* Flexibility, a loosely coupled system is easier to replace.

Although the proposed solution gives us several advantages, we also need to consider the added complexity of communication between the online and offline systems.

## Terminology

Some terminology is used within the Future Fund, which need a unambiguous description:

| Term                | Definition                                                                                         |
| ------------------- | -------------------------------------------------------------------------------------------------- |
| Donation            | An amount of money donated by a donor to the Future Fund.                                          |
| Investment fund     | A selected actual fund, that consists of money that is invested and a cash amount of money.        |
| Invested amount     | The amount of money allocated to an actual fund.                                                   |
| Cash amount         | The amount of money not allocated to an actual fund.                                               |
| Enter               | Making new donations part of some investment fund.                                                 |
| Exit                | Extracting (cash) money from an investment fund for the purpose of transferring it to charity.     |
| Charity             | A good cause, for NL a registered ANBI.                                                            |
| (Money) transfer    | A transfer of money (profits) to a charity.                                                        |
| Conversion day      | The day on which donations and profits are converted into a next process step.                     |
| Ownership fraction  | The fraction of an investment fund that is owned by a donation.                                    |
| Ideal valuation     | The amount of money a donation should be worth, any excess money could be transferred.             |
| Bad year percentage | The fraction of the worth that should be transferred to charity, regardless of investment results. |        