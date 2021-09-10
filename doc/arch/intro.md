# Introduction

The Future Fund (in Dutch: [Toekomstfonds](https://toekomstfonds.nl)) is an organization that aims to provide a sustainable cash flow for charitable organizations by investing donations made, and transferring a part of the profits to the charity of the donor's choice.
A 50-50 rule is advertised on the website, meaning half of profits go to the charity, and the other half is reinvested to increase possible transfers to charity in the future even more.
On top of that the donation is invested in an ethical investment fund.
The reader is encouraged to read up on the Future Fund's advertised model on the website.

## Abstract

This document describes the architecture and technical design of an administration module for the Future Fund.
Initially, the proposed solution was to administer all financial data in the website's database, but several conditions have changed the preferred architecture to be an highly offline scenario.

The proposed solution is an event sourcing solution.
The key motivating drivers for this insight are:

* Security, having the administration offline gives potential less attack vectors.

* Simplicity, integrating with Wordpress is something we don't have enough experience with. On top of that we don't want any big unnecessary dependencies.

* Integrity, keeping the data offline gives us more easy ways to protect data integrity.

* Flexibility, a loosely coupled system is easier to replace or modify.

Although the proposed solution gives us several advantages, we also need to consider the added complexity of communication between the online and offline systems.

## Terminology

Some terminology is used within the Future Fund, which need a unambiguous description:

| Term                  | Definition                                                                                                                                                                                            |
| --------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Donation              | An amount of money donated by a donor to the Future Fund.                                                                                                                                             |
| Investment **option** | An aggregate of both an investment fund and liquid assets.                                                                                                                                            |
| Investment **fund**   | An actual investment fund, consisting of stocks, obligations, etc.                                                                                                                                    |
| Invested amount       | The amount of money allocated to an investment fund.                                                                                                                                                  |
| Cash amount           | The amount of money not allocated to an investment fund, the liquid assets.                                                                                                                           |
| `Invest`              | A step that transfers liquid assets to an investment fund.                                                                                                                                            |
| `Liquidate`           | A step that liquidates investment fund investments to liquid assets.                                                                                                                                  |
| `Enter`               | Making new donations part of some investment option.                                                                                                                                                  |
| `Exit`                | Extracting liquid assets from an investment option for the purpose of transferring it to charity.                                                                                                     |
| Charity               | A good cause, ()potential) beneficiary of money transfers from the Future Fund. For NL a registered ANBI.                                                                                             |
| (Money) transfer      | A transfer of money (profits) to a charity.                                                                                                                                                           |
| Conversion day        | The day on which donations and profits are converted into a next process step.                                                                                                                        |
| Ownership fraction    | The fraction of an investment option that is owned by a _donation_. Fractions are owned by donations and not by donors, since donations are the lowest level of disaggregation                        |
| Ideal valuation       | The amount of money a donation should be worth, any excess money is transferrable.                                                                                                                    |
| Reinvestment fraction | The fraction of the profits that should remain in the investment option.                                                                                                                              |
| Future Fund fraction  | The fraction of the profits that should be transferred to the future fund.                                                                                                                            |
| Charity fraction      | The fraction of the profits that should be transferred to the chosen charity.                                                                                                                         |
| Bad year fraction     | The fraction of the worth that should be transferred to charity in a year when the investments results are negative or the Future Fund is still correcting for previous years with negative interest. |