---
title: Money bag
author: J.W. Morsink
difficulty: medium
---

# Money bag

A money bag is a technical data structure designed to hold amounts of money in multiple currencies.
It supports arithmetic addition, automatically grouping and summing amounts by currency.

For example:

$$ [1\ \text{EUR}] + [2\ \text{EUR}] = [3\ \text{EUR}] $$

$$ [1\ \text{USD} + 2\ \text{EUR}] + [3\ \text{GBP} + 4\ \text{EUR}] = [1\ \text{USD} + 6\ \text{EUR} + 3\ \text{GBP}] $$

Amounts in different currencies are commutative within the money bag:

$$ [1\ \text{EUR} + 2\ \text{GBP}] = [2\ \text{GBP} + 1\ \text{EUR}] $$

This structure simplifies calculations involving multiple currencies, ensuring that sums are always grouped and presented by currency.
It is commonly used in financial models where transactions or balances span several currencies.