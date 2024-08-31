# META_UPDATE_CHARITY

This [event](../event) updates information on the [charity](../charity), like bank information. Only supplied data is used for update.

| Field             | Type               | Description                                   | Value                 |
| ----------------- | ------------------ | --------------------------------------------- | --------------------- |
| `Type`            | A                  | Identifies the event                          | `META_UPDATE_CHARITY` |
| `Timestamp`       | DateTime(ISO-8601) | The time of the event                         |                       |
| `Code`            | AN                 | Identifies the charity                        |                       |
| `Name`            | AN?                | The name of the charity                       |                       |
| `Bank_account_no` | AN?                | The charity's bank account number             |                       |
| `Bank_name`       | AN?                | The charity's name, as registered by the bank |                       |
| `Bank_bic`        | AN?                | The charity's bank identification code        |                       |