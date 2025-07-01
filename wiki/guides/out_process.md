---
title: The out process checklist
author: J.W. Morsink
---
# The out process checklist

1. [ ] Create a branch `Exit_`{_yyyymm_}

2. [ ] Determine cost compensation. 

>  <input style="float: right"/>
>

>  Cost compensation consists of:

>  * ETF costs automatically deducted from the cash amount of the fund
>  * Dividends are transferred to the cash part of the investment account, but aren't administered in regular Enter and Invest events.

3. [ ] Lookup data on [saxoinvestor.nl](https://www.saxoinvestor.nl/).

>  We need to let the `Exit`/`Liquidate` happen just before the `Enter`/`Invest`. 
>  This is why we follow the same subprocedure as we do for the 'in process':
>
>  * On the 'Transacties' tab the automatic investment transactions can be found with:
>    * A total amount for the transactions. (Only needed for the 'in process')
>      <input style="float: right"/>
>
>    * An execution date for the transactions. We use a convention to administer this on the midnight before.
>      <input style="float: right"/>
>
>  * We need the invested amount for the closing of the day **before**. Use the 'Historie' feature of the 'Overzicht' tab.
>      <input style="float: right"/>
>

4. [ ] Fill in the top part of the Conversion/Liquidate form

  * Use the cost compensation determined in step 2.
  * Enter the invested amount determined in step 3.
  * Enter the execution date determined in step 3 in the timestamp field (Timestamp can be omitted).
    * Must be in the _yyyy-mm-dd_ format.

5. [ ] Click on the Recalculate button and determine liquidation, fill in the bottom part

  * Recalculate determines the need for liquidation of funds for the `Exit` step.

    It will probably result in a 0-liquidation, if so:
    * Hit the Zero liquidation button to automatically fill in New invested and cash amounts.

    If not:
    * Determine and fill in the new invested and cash amounts.
  * Optionally enter a transaction reference.

6. [ ] Click on the Liquidate button, you are redirected to the `Exit` step.

7. [ ] Check the timestamp

8. [ ] Click on the Recalculate button to determine the actual `Exit` amount.

9. [ ] Click on the Exit button to confirm the `Exit`.

10. [ ] Switch to `Main` and fastforward to the working branch, if everything looks good.

11. [ ] Delete the working branch.

12. [ ] You can optionally administer transfers now.
  
 




