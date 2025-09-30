
---
title: NONE
author: J.W. Morsink
---

# NONE

The `NONE` event is a placeholder event type.
It is used to represent the absence of a valid event or as a default value in code and models.
This event should not occur in normal operation and does not affect system state.

## Fields

| Field      | Type                | Description                                 | Value   |
|------------|---------------------|---------------------------------------------|---------|
| `Type`     | string              | Identifies the event.                       | `NONE`  |
| `Timestamp`| DateTime (ISO-8601) | The timestamp of the event.                 |         |

## Purpose

The NONE event is used internally to mark the start of an event sequence or to fill gaps where no real event exists.
It is not intended for use in business logic or user-facing features.
