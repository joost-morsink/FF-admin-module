# Business Process

The Admin module mainly supports the business process of the conversion day.
The process consists of the following _strictly sequential_ steps:

* Calculate liquidation and transfers

* Execute liquidation and administer results

* Execute `Exit`

* Transfer money to charities and administer results

* Export data (donations) from the online database

* Import this data into the event source

* Execute `Enter`

* Execute investment of newly added liquid assets (cash amount) and administer results

* Make an export for auditing purposes

* Make an export for the website

* Import the admin module export into the website

## Steps

### Calculate liquidation and transfers

The application is able to calculate from the current state how much money should be transferred to each charity.
This should all be based on the current ownership fractions administered on the investment option and an actual worth of the invested amount.
The total amount to liquidate should be '_in the neighbourhood_' of the sum of all the calculated transfers.

| Input                   | Description                                                    |
| ----------------------- | -------------------------------------------------------------- |
| Current_invested_amount | The currently invested amount of money in the investment fund. |

The calculation should take into account:

* The time span since the last `Exit` (per donation).

* The ideal value (per donation).

* The investment option's current fractions.

### Execute liquidation

After the liquidation has been executed on the investment platform, the results of the transaction should be administered.

### Execute Exit

The `Exit` step allocates money in the right proportions to all the charities that have been selected by the original donations.
If there are enough liquid assets in the investment option, the total amount of calculated money can be transferred.
If there aren't, a fraction should be calculated based on the availability of liquid assets.

### Transfer money to charities and administer results

A report of all the calculated transfers to charities is made, and after transferring the money, the results should be administered.

### Donation data export online

The payment plugin supports an export of new donations, we might get away with extracting 'new' charities from that export.

The exported data is described in the section [Donation data](#donation-data).

### Import donation data offline

The application will have a page allowing for the import of donation data, possibly enriched with charity information.

### Execute Enter

The `Enter` step pertains to all the new donations, making them part of the investment option.
All ownership fractions are re-evaluated after an the `Enter`.

### Execute investment of new liquid assets and administer results

After new investments have been made from the cash amount of an investment option, the results of the transaction should be administered.

### Make an export for auditing purposes

After the process is done, an auditing report should be automatically made.
Also, an event store is ready to be committed with all the newly generated data.

### Make an export for the website

Aggregated results should be exported from the admin module.

The datamodel and format of the export is described in the section [Aggregated data](#aggregated-data)

### Import the admin module export into the website

The admin module's export should be imported into the website, so donors are able to view the results.
The data should at least contain the relevant data in its most disaggregated form (per donation).

## Data interfaces

The admin module has two data interfaces with the Future Fund website.

### Donation data

> **TODO** XML? Transforms?

### Aggregated data

> **TODO** SQL?
