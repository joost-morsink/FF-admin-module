# Business Process

The Admin module mainly supports the business process of the conversion day.
The process consists of the following _strictly sequential_ steps:

* Calculate liquidation and transfers

* Execute liquidation and administer results

* Execute `Exit`

* Transfer money to charities and administer results

* Optional 2nd part of the process:
  
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
If there aren't, either:

* more stocks need to be liquidated, or

* a fraction should be calculated based on the availability of liquid assets.

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

The GiveWP plugin has a default export functionality to export donations from a certain period in CSV format.
Within the fields of the CSV some formatting has been done depending on the regional settings of the WordPress website.
We won't need all the exported fields, and will show a selection in the corresponding section.

#### Regional settings

The application should be configurable with respect to handling regional deviations from the standard. 

| Setting             | Preferred  | nl-NL      | en-US      |
| ------------------- | ---------- | ---------- | ---------- |
| List separator      | ,          | ;          | ,          |
| Thousands separator | none       | .          | ,          |
| Decimal separator   | .          | ,          | .          |
| Dateformat          | yyyy-MM-dd | dd/MM/yyyy | MM/dd/yyyy |
| Quotation           | "          | "          | "          |

If choosing a predefined culture suffices, we needn't implement custom parsing logic.

#### Columns

| Field                       | Type | Description                                                                                          |
| --------------------------- | ---- | ---------------------------------------------------------------------------------------------------- |
| Donation ID                 | N    | An identifier for the donation                                                                       |
| Donation Total              | N    | A total amount for the donation                                                                      |
| Currency Code               | AN   | An ISO-4217 currency code                                                                            |
| Donation Status             | AN   | A status for the donation, only Complete and Renewal are eligible for import                         |
| Donation Date               | AN   | The date part of the timestamp                                                                       |
| Donation Time               | AN   | The time part of the timestamp                                                                       |
| Payment Gateway, Mode, Type | AN   | These columns might be used to determine cancellation timespans and determine eligibility for import |
| Form ID                     | N    | An identifier for the charity                                                                        |
| Form Title                  | AN   | A name of the charity, can be used for _New charity_ events                                          |
| Donor ID                    | N    | An identifier for the donor                                                                          |

The missing data points (which can be found in chapter [Events](#events)) to create an event are:

| Field                 | Remark                                                                       |
| --------------------- | ---------------------------------------------------------------------------- |
| Option_id             | Might be determined by a default or a combination of items in the CSV row    |
| Exchanged_amount      | If currencies do not match up, this needs to be entered by the administrator |
| Exchange_reference    | If currencies do not match up, this can be entered by the administrator      |
| Transaction_reference | Can be empty, or matched with bank account records                           |

> #### Assumption
> 
> With only one option and accepting only the EUR currency, we are effectively not missing data points.
> If these conditions change, we need to revise the usefulness of this import on its own.

### Aggregated data

> #### Todo
> 
> We can determine how to export relevant data to the website when that design has been made.
