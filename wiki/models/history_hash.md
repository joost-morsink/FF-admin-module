---
title: Model HistoryHash
author: J.W. Morsink
difficulty: hard
---

# Model HistoryHash

This model assigns a unique identifier to the history of a sequence up to the current event.

It uses the previous hash and a textual representation of the event to compute a new hash.

The calculation is as follows:

$$
HistoryHash_{t+1} = SHA256(HistoryHash_t + UTF8(Json(E)))
$$

Where:

- $HistoryHash_t$ is the hash of the history up to time $t$
- $E$ is the current event, serialized as JSON and encoded in UTF-8
- $SHA256$ is the cryptographic hash function used

This approach ensures that each possible history sequence produces a unique hash value.

## Usage in AUDIT Events

The HistoryHash model is used in the context of AUDIT events to verify the integrity of the event sequence.
By storing the computed history hash with each AUDIT event, it becomes possible to detect any tampering or reordering of events.
This mechanism provides a cryptographically secure audit trail, ensuring that the sequence of events can be trusted and independently validated.

