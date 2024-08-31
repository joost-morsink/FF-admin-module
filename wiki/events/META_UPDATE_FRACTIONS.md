# META_UPDATE_FRACTIONS

This [event](../event) updates the reinvestment [ractions](../option_fractions) for an [option](./option).
The three fractions in the event should add up to 1.

| Field                   | Type                | Description                                                                                                  | Value                   |
| ----------------------- | ------------------- | ------------------------------------------------------------------------------------------------------------ | ----------------------- |
| `Type`                  | A                   | Identifies the event                                                                                         | `META_UPDATE_FRACTIONS` |
| `Timestamp`             | DateTime (ISO-8601) | The time of the event                                                                                        |                         |
| `Code`                  | AN                  | The identifier for the option                                                                                |                         |
| `Reinvestment_fraction` | N(10,10)            | The fraction of the profits to reinvest                                                                      |                         |
| `FutureFund_fraction`   | N(10,10)            | The fraction of the profits to donate to the future fund                                                     |                         |
| `Charity_fraction`      | N(10,10)            | The fraction of the profits to donate to the charity                                                         |                         |
| `Bad_year_fraction`     | N(10,10)            | The minimal fraction of the total amount of money in the investment option that should always be transferred |                         |
