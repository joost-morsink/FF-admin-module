---
title: Model Partitioning
author: J.W. Morsink
---

# ${title}

Model partitioning is a performance optimization technique that splits a model into a header structure and multiple detail buckets.
The header contains summary information, while each bucket holds a subset of detailed entries.

The meta model defines how many buckets should exist for a given header and how to determine the bucket number for any key.

Most events either do not change the model or only affect the header and a single detail bucket.
This approach significantly reduces the amount of data that needs to be sent and retrieved during updates or queries.

To maintain balanced bucket sizes, the number of mask bits can be increased when buckets grow too large.

## Example

In the [Donations2 model](./models/donations), the header tracks the total number of donations.
This total is used to determine the number of mask bits, which in turn sets the number of buckets ($2^{maskbits}$).
Each donation's `Id` is mapped to a 32-bit integer, and the mask bits are applied to compute the bucket number for that key.
Within the corresponding bucket, the donation entry can be found.

```plantuml
[*] --> Id : Get Donation Id
Id --> Int : Convert to 32 bit integer
Int --> Bucket : Mask with maskbits to get bucket number
Bucket --> Detail : Use to get detail
Detail --> Donation : Use Id indexing
Donation --> [*]

Id : The donation's Id
Int: A stable hash code derived from the Id
Bucket: The computed bucket number
Detail: A structure containing all donations in the same bucket
Donation: The donation entry
```

## Mega event

A mega event is an operation that modifies a large number of entries in the model at once, potentially affecting multiple buckets and the header.
Mega events are typically used for bulk updates that cannot be efficiently represented as a series of individual events.

When a mega event occurs:
- The header may be updated to reflect new summary information.
- Multiple detail buckets may be changed simultaneously, requiring careful coordination to maintain consistency.
- The partitioning logic ensures that only the affected buckets and header are updated, minimizing unnecessary data transfer and computation.

Mega events should be rare in a model, as they require significant processing and can impact system performance.

Currently the only mega event is the [`CONV_ENTER`](./events/CONV_ENTER) event for the [OptionWorths2](./models/option_worths) model.

