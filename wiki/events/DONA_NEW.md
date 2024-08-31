---
title: DONA_NEW
author: J.W. Morsink
---

# DONA_NEW

This [event](../event) represents some [donation](../donation) made by some [donor](../donor) to some [charity](../charity) through investment in some [investment option](../option).
If the currency matches the investment option's currency, the fields `Amount` and `Exchanged_amount` are equal, otherwise the result of currency exchange is recorded in the `Exhanged_amount` field.

| Field                   | Type                 | Description                                                                                             | Value      |
| ----------------------- | -------------------- | ------------------------------------------------------------------------------------------------------- | ---------- |
| `Type`                  | A                    | Identifies the event                                                                                    | `DONA_NEW` |
| `Timestamp`             | DateTime (ISO-8601)  | The timestamp of the event                                                                              |            |
| `Execute_timestamp`     | DateTime (ISO-8601)? | The timestamp the donation should be considered valid, if not present, consider it equal to `Timestamp` |            |
| `Donation`              | AN                   | Identifies the donation                                                                                 |            |
| `Donor`                 | AN                   | The identifier for the donor                                                                            |            |
| `Charity`               | AN                   | The identifier for the charity                                                                          |            |
| `Option`                | AN                   | The identifier for the investment option                                                                |            |
| `Currency`              | AN                   | An ISO-4217 currency code                                                                               |            |
| `Amount`                | N(16,4)              | The donated amount                                                                                      |            |
| `Exchanged_amount`      | N(16,4)              | The donated amount in the currency of the investment option                                             |            |
| `Transaction_reference` | AN                   | An external reference for the donation transaction                                                      |            |
| `Exchange_reference`    | AN?                  | An optional external reference for the exchange transaction                                             |            |
