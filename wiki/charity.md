# Charity

A charity is a non-profit entity that has a purpose of making a certain aspect of the world better.
Charities are the beneficiary of [payouts](./payout) initiated by [Give for Good](./index).

```plantuml
!include <archimate/Archimate>

Business_Role(Charity, "Charity")

Business_Process(Donating, "Donating")
Business_Process(Select, "Select Charity")

Donating *- Select
Charity -- Select : Select beneficiary charity

Business_Process(Payout, "Payout")

Charity -- Payout : [[allocation Allocations]] payed out

url for Donating is [[donation]]
url for Payout is [[payout]]
```
