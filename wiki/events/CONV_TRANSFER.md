# CONV_TRANSFER

This [event](../event) represents the actual [transfer](../transfer) of [allocated money](../allocation) to the [charity](../charity).

| Field                   | Type                | Description                                                                    | Value           |
| ----------------------- | ------------------- | ------------------------------------------------------------------------------ | --------------- |
| `Type`                  | A                   | Identifies the event                                                           | `CONV_TRANSFER` |
| `Timestamp`             | DateTime (ISO-8601) | The timestamp of the event                                                     |                 |
| `Charity`               | AN                  | The identifier of the charity                                                  |                 |
| `Currency`              | AN                  | An ISO-4217 currency code                                                      |                 |
| `Amount`                | N(20,4)             | The amount donated to the charity                                              |                 |
| `Exhanged_Currency`     | AN                  | An ISO-4217 currency code for the currency after exchange (charity's currency) |                 |
| `Exchanged_Amount`      | N(20,4)             | The amount donated to the charity in the exchanged currency.                   |                 |
| `Transaction_reference` | AN                  | External reference code for transaction                                        |                 |
| `Exchange_reference`    | AN?                 | An optional external reference for the exchange                                |                 |
