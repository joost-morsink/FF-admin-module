---
title: Model HistoryHash
author: J.W. Morsink
---

# Model HistoryHash

This model tries to assign a unique identifier to the history of the sequence up to the current event.
It uses the previous hash and a textual representation of the Event to calculate a new hash:

$$ 
HistoryHash_{t+1} = SHA256(HistoryHash_t + UTF8(Json(E)))
$$

