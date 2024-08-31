# CONV_EXIT

This [event](../event) represents an [allocation](../allocation) of money to [charities](../charities) from the [cash amount](../cash_amount) of an investment option.
The actual [transfer](../transfer) to the charities may be delayed.

| Field       | Type                | Description                             | Value       |
| ----------- | ------------------- | --------------------------------------- | ----------- |
| `Type`      | A                   | Identifies the event                    | `CONV_EXIT` |
| `Timestamp` | DateTime (ISO-8601) | The timestamp of the event              |             |
| `Option`    | AN                  | The identifier of the investment option |             |
| `Amount`    | N(20,4)             | The amount to be donated to charities   |             |
