# DONA_CANCEL

This [event](../event) represents revocation of a direct debit, and is only valid if the timestamp for the cancellation lies before the execution timestamp of the original [donation](../donation).

| Field       | Type                | Description                                      | Value         |
| ----------- | ------------------- | ------------------------------------------------ | ------------- |
| `Type`      | A                   | Identifies the event                             | `DONA_CANCEL` |
| `Timestamp` | DateTime (ISO-8601) | The timestamp of the event                       |               |
| `Donation`  | AN                  | Identifies the donation that should be cancelled |               |
