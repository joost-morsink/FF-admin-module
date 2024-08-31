# DONA_UPDATE_CHARITY

This [event](../event) represents the [donor's](../donor) decision to change the beneficiary [charity](../charity) of the [donation's](../donation) profits.

| Field       | Type                | Description                                      | Value                 |
| ----------- | ------------------- | ------------------------------------------------ | --------------------- |
| `Type`      | A                   | Identifies the event                             | `DONA_UPDATE_CHARITY` |
| `Timestamp` | DateTime (ISO-8601) | The timestamp of the event                       |                       |
| `Donation`  | AN                  | Identifies the donation that should be changed   |                       |
| `Charity`   | AN                  | The identifier for the charity                   |                       |
