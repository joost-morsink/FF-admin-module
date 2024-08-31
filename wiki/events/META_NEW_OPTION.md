# META_NEW_OPTION

This [event](../event) creates a new [investment option](../option) that can be used for investing the donations.
The three [fractions](../option_fractions) (`Reinvestment_fraction` + `FutureFund_fraction` + `Charity_fraction`) in the event should add up to 1.
The `Bad_year_fraction` does not apply to profits, like the other three fractions, but instead it applies to the invested value of the donation.

| Field                   | Type                | Description                                                                                                  | Value             |
| ----------------------- | ------------------- | ------------------------------------------------------------------------------------------------------------ | ----------------- |
| `Type`                  | A                   | Identifies the event                                                                                         | `META_NEW_OPTION` |
| `Timestamp`             | DateTime (ISO-8601) | The time of the event                                                                                        |                   |
| `Code`                  | AN                  | Identifies the investment option                                                                             |                   |
| `Name`                  | AN                  | The name of the investment option                                                                            |                   |
| `Currency`              | AN                  | The ISO-4217 currency code of the investment option                                                          |                   |
| `Reinvestment_fraction` | N(10,10)            | The fraction of the profits to reinvest                                                                      |                   |
| `FutureFund_fraction`   | N(10,10)            | The fraction of the profits to donate to the future fund                                                     |                   |
| `Charity_fraction`      | N(10,10)            | The fraction of the profits to donate to the charity                                                         |                   |
| `Bad_year_fraction`     | N(10,10)            | The minimal fraction of the total amount of money in the investment option that should always be transferred |                   |
