# META_CHARITY_PARTITION

This [event](../event) makes a [charity](../charity) into a ['donation theme'](../theme) by specifying the distribution of the theme.
If the partitions are empty or contain only a self-reference, a donation theme is restored as an actual charity.

| Field           | Type               | Description                                   | Value                    |
| --------------- | ------------------ | --------------------------------------------- | ------------------------ |
| `Type`          | A                  | Identifies the event                          | `META_CHARITY_PARTITION` |
| `Timestamp`     | DateTime(ISO-8601) | The time of the event                         |                          |
| `Code`          | AN                 | Identifies the donation theme                 |                          |
| `Partitions`    | (AN, N(20,10))     | The partitions of the theme                   |                          |
