---
title: DONA_CANCEL
author: J.W. Morsink
---

# DONA_CANCEL

The `DONA_CANCEL` event represents the revocation of a direct debit donation.
It is only valid if the cancellation timestamp is before the execution timestamp of the original [donation](../donation).

## Fields

| Field       | Type                | Description                                      | Value         |
|-------------|---------------------|--------------------------------------------------|---------------|
| `Type`      | string              | Identifies the event.                            | `DONA_CANCEL` |
| `Timestamp` | DateTime (ISO-8601) | The timestamp of the event.                      |               |
| `Donation`  | string              | Identifies the donation to be cancelled.         |               |

## Purpose

This event ensures that donations can be revoked before they are executed, maintaining accurate records and compliance with direct debit regulations.
