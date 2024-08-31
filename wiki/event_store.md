---
title: Give for Good - Event Store
author: J.W. Morsink
---
# Event Store

The [Admin Module](./admin_module) stores [events](./event) in a sequential order in a store. 
Currently this store is implemented in an Azure SQL database.
It supports a simple branching mechanism with the following operations:

* Branch
* Rebase
* Fast forward
* Delete

There is no concept of a merge, a branch needs to be rebased on the target branch in order to fast forward the target branch.
This ensures a purely linear history (not an acyclic directed graph).

