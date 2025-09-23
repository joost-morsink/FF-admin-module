---
title: Model Partitioning
author: J.W. Morsink
---

# ${title}

As a performance optimalization, a model can be _partitioned_.
This means the model is split into a single header structure and multiple details.
The meta model enables determining how many details (buckets) should be present given a header and determining the bucket number given a key.

Most, if not all events either do not change the model, or only the header and a single detail.
This dramatically decreases the amount of data sent and retrieved.
The number of items in a detail/bucket should roughly stay the same, increasing the maskbits when buckets could become too big.

## Example

In the [Donations2 model](./models/donations), the header stores a number of total donations made. 
Using this number the number of _mask bits_ is determined, leading to the number of buckets ($2^{maskbits}$).
Every donation's `Id` can be mapped to a 32 bit integer, which can in turn be masked to get the bucket number for the key.
In the detail stored in the bucket, the entry for the donation can be found.

```plantuml
[*] --> Id : Get Donation Id
Id --> Int : Convert to 32 bit integer
Int --> Bucket : Mask with maskbits to get bucket number
Bucket --> Detail : Use to get detail
Detail --> Donation : Use Id indexing
Donation --> [*]

Id : The donation's Id
Int: A kind of hash code for the Id
Bucket: The bucket number
Detail: A detail structure containing all the donations in the same bucket
Donation: The donation
```

## Mega event