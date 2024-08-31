# CONV_INVEST

This [event](../event) represents the investment of an amount of cash into the actual [investment option](../option).

| Field                   | Type                | Description                                                            | Value         |
| ----------------------- | ------------------- | ---------------------------------------------------------------------- | ------------- |
| `Type`                  | A                   | Identifies the event                                                   | `CONV_INVEST` |
| `Timestamp`             | DateTime (ISO-8601) | The timestamp of the event                                             |               |
| `Option`                | AN                  | The identfier for the investment option                                |               |
| `Invested_amount`       | N(20,4)             | The new total invested amount of money in the investment fund          |               |
| `Cash_amount`           | N(20,4)             | The new total cash amount of money in the investment option's reserves |               |
| `Transaction_reference` | AN                  | An external reference for the investment transaction                   |               |
