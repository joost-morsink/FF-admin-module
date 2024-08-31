# PRICE_INFO

This [event](../event) allows for the input of fund pricing info, so the [donations'](../donation) worth can be more easily plotted.

| Field             | Type                | Description                                                                                    | Value        |
| ----------------- | ------------------- | ---------------------------------------------------------------------------------------------- | ------------ |
| `Type`            | A                   | Identifies the event                                                                           | `PRICE_INFO` |
| `Timestamp`       | DateTime (ISO-8601) | The timestamp of the event                                                                     |              |
| `Option`          | AN                  | The identifier for the option                                                                  |              |
| `Invested_amount` | N(20,4)             | The total invested amount of money in the fund, according to the current investment fund price |              |
| `Cash_amount`     | N(20,4)             | The total cash amount of money in the investment option                                        |              |
