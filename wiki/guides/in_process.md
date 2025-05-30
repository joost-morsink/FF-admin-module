---
title: The in process
author: J.W. Morsink
---

# The in process

1. [ ] Create a branch Enter_{_yyyy_}.

2. [ ] Lookup data on [saxoinvestor.nl](https://www.saxoinvestor.nl/).

>  We need to let the `Exit`/`Liquidate` happen just before the `Enter`/`Invest`. 
>  This is why we follow the same subprocedure as we do for the 'in process':
>
>  * On the 'Transacties' tab the automatic investment transactions can be found with:
>    * A total amount for the transactions (Only needed for the 'in process')
>    * An execution date for the transactions. We use a convention to administer this on the midnight before.
>  * We need the invested amount for the closing of the day before. Use the 'Historie' feature of the 'Overzicht' tab.

3. [ ] Fill in the Enter form.

    * Use the Invested amount and execution date as determined in step 2.

4. [ ] Click on the Enter button.

5. [ ] Fill in the amount for the automatic investment transactions in the Investment field.

6. [ ] Click on transfer investment, which will disable on success

  * The new Cash amount and new Invested amount are now adjusted by the investment amount.

7. [ ] Optionally enter a transaction reference.

8. [ ] Click on the Invest button to confirm the `Invest` event.

9. [ ] Merge the branch back to `Main`.