# Business Process

The Admin module supports the business process of the conversion day.
The process consists of the following _strictly sequential_ steps:

* Export data from the online database, it comprises:
  
  - New charities
  
  - New donations

* Import this data into the event source

* Calculate liquidation and transfers

* Execute liquidation and administer results

* Execute Enter/Exit

* Execute investment of new funds and administer results

* Transfer money to charities and administer results

* Make an export for auditing purposes

* Make an export for the website

* Import the admin module export into the website

## Steps

### Donation data export online

The payment plugin support an export of new donations, we might get away with extracting 'new' charities from that export.

The exported data is described in the section [Donation data](#donation-data).

### Import donation data offline

The application will have a page allowing for the import of donation data, possibly enriched with charity information.

### Calculate liquidation and transfers

The application is able to calculate from the current state how much money should be transferred to each charity.
This is all based on the current fractions administered on the investment fund and an actual worth of the invested amount.

| Input                   | Description                                                    |
| ----------------------- | -------------------------------------------------------------- |
| Current_invested_amount | The currently invested amount of money in the investment fund. |

The calculation should take into account:

* The period since the last `Exit` (per donation).

* The ideal value (per donation).

* The bad year percentage.

### Execute liquidation

After the liquidation has been executed on the investment platform, the results of the transaction should be administered.

### Execute Enter/Exit

A probably automated process step administering the results of donations entering the investment fund and/or charity money exiting the investment fund.
The `Enter` step pertains to all the new donations.
The `Exit` step allocates money in the right proportions to all the charities that have been selected by the original donations.

### Execute investment of new funds and administer results

After new investments have been made from the cash part of an investment fund, the results of the transaction should be administered.

### Transfer money to charities and administer results

A report of all the transfers to charities is made, and after transferring the money, the results should be administered.

### Make an export for auditing purposes

After the process is done, an auditing report should be automatically made.
Also, an event store is ready to be committed with all the newly generated data.

### Make an export for the website

Aggregated results should be exported from the admin module.

The datamodel and format of the export is described in the section [Aggregated data](#aggregated-data)

### Import the admin module export into the website

The admin module's export should be imported into the website, so donors are able to view the results.

## Data interfaces

The admin module has two data interfaces with the Future Fund website.

### Donation data

> **TODO** XML? Transforms?

### Aggregated data

> **TODO** SQL?
