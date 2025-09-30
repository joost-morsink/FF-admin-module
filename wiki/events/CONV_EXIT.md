--- 
title: CONV_EXIT
author: J.W. Morsink
difficulty: medium
---

# CONV_EXIT

The `CONV_EXIT` event marks the allocation of funds from the cash amount of an [investment option](../option) to [charities](../charity).
This event determines the amount to be distributed, but the actual [transfer](../transfer) to charities may occur later.

## Fields

| Field       | Type                | Description                             | Value       |
| ----------- | ------------------- | --------------------------------------- | ----------- |
| `Type`      | string              | Identifies the event                    | `CONV_EXIT` |
| `Timestamp` | DateTime (ISO-8601) | The timestamp of the event              |             |
| `Option`    | string              | The identifier of the investment option |             |
| `Amount`    | decimal(20,4)       | The amount to be allocated to charities |             |

## Purpose

This event is essential for triggering the allocation process and determining how much will be distributed to charities.
It is a key step in the payout lifecycle for investment options.

## Notes

Transfers to charities may be postponed due to minimum thresholds or transfer costs.
Allocated amounts are tracked until they are eligible for transfer.