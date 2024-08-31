# CONV_LIQUIDATE

This [event](../event) represents a liquidation from an [investment option](../option), for the purpose of [donating](../transfer) the withdrawn money to the [charities](../charity). 
This event only pertains to the liquidation of [invested money](../invested_amount) to the [cash amount](../cash_amount) of the investment option.

| Field                   | Type                | Description                                                            | Value            |
| ----------------------- | ------------------- | ---------------------------------------------------------------------- | ---------------- |
| `Type`                  | A                   | Identifies the event                                                   | `CONV_LIQUIDATE` |
| `Timestamp`             | DateTime (ISO-8601) | The timestamp of the event                                             |                  |
| `Option`                | AN                  | The identifier for the investment option                               |                  |
| `Invested_amount`       | N(20,4)             | The new total invested amount of money in the investment option        |                  |
| `Cash_amount`           | N(20,4)             | The new total cash amount of money in the investment option's reserves |                  |
| `Transaction_reference` | AN                  | An external reference for the withdrawal transaction                   |                  |
