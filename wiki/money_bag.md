# Money bag

A money bag is a technical data structure that contains different amounts of money in different currencies, and supports the basic arithmetic addition operator.
This operator groups amounts of money from the same currency before adding:

$$ [1 EUR] + [2 EUR] = [3 EUR] $$

$$ [1 USD + 2 EUR] + [3 GBP + 4 EUR] = [1 USD + 6 EUR + 3 GBP] $$

The amounts of different currencies are commute with one another within the money bag:

$$ [1 EUR + 2 GBP] = [2 GBP + 1 EUR] $$