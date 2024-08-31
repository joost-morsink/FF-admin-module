# CONV_ENTER

This [event](../event) signifies the point in time at which all new [donations](../donation) are made part (in cash form) of the [investment option](../option).

Although the `Invested_amount` does not seem to have anything to do with the `Enter` event, it is the only piece of data that is both not yet known and influencing the recalculation of [ownership fractions](../ownership_fractions).

| Field             | Type                | Description                                                                                                        | Value        |
| ----------------- | ------------------- | ------------------------------------------------------------------------------------------------------------------ | ------------ |
| `Type`            | A                   | Identifies the event                                                                                               | `CONV_ENTER` |
| `Timestamp`       | DateTime (ISO-8601) | The timestamp of the event                                                                                         |              |
| `Option`          | AN                  | The identifier for the option                                                                                      |              |
| `Invested_amount` | N(20,4)             | The total **invested** amount of money in the investment option (possibly calculated by current price information) |              |
