---
title: Audit
author: J.W. Morsink
difficulty: medium
---

# Audit

The `AUDIT` event records a snapshot of the system's state for verification and compliance.
It consolidates key metrics and hash codes, allowing integrity checks of all processed events up to the audit point.

An audit event is typically created on demand.
It does not record arbitrary changes, but instead summarizes the state for auditing and compliance purposes.

Audit events are essential for maintaining trust and transparency in the Give for Good platform.

## Fields

| Field            | Type      | Description                                                      |
|------------------|-----------|------------------------------------------------------------------|
| type             | string    | Must be `AUDIT`                                                  |
| timestamp        | datetime  | When the audit was performed                                     |
| hashcode         | string    | Hash of the event history up to this point                       |
| previousHashCode | string    | Hash of the previous audit event (if any)                        |
| eventCount       | integer   | Number of events processed                                       |
| previousCount    | integer   | Number of events at previous audit (if any)                      |


## Purpose

Audit events enable:

* Verification of the system's state and event history.
* Detection of inconsistencies or tampering.
* Potentially support regulatory or compliance requirements.
