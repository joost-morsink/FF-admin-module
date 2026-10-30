@include "Module.ks" .

# The descriptions of the event types, as shown on the page of the events. Checked against the code by check_event_type_descriptions.
eventtypedocs = {
    eventtype:NONE ff:description "Technical placeholder event" .
    eventtype:DONA_NEW ff:description "A new [donation](./donation) has been made" .
    eventtype:DONA_UPDATE_CHARITY ff:description "An existing [donation](./donation) has been reassigned to a new [charity](./charity)" .
    eventtype:META_NEW_OPTION ff:description "A new [option](./option) has been registered" .
    eventtype:META_NEW_CHARITY ff:description "A new [charity](./charity) has been registered" .
    eventtype:META_UPDATE_FRACTIONS ff:description "The [fractions](./option_fractions) of an [option](./option) have changed" .
    eventtype:CONV_LIQUIDATE ff:description "Stocks have been liquidated" .
    eventtype:CONV_EXIT ff:description "Funds have exited, and should be [allocated](./allocation)" .
    eventtype:CONV_TRANSFER ff:description "A [transfer](./transfer) of funds to a charity has been made" .
    eventtype:CONV_ENTER ff:description "Eligible [donations](./donation) have entered" .
    eventtype:CONV_INVEST ff:description "Funds have been invested in stocks" .
    eventtype:CONV_INFLATION ff:description "A correction for inflation has been requested" .
    eventtype:AUDIT ff:description "An audit report has been consolidated" .
    eventtype:DONA_CANCEL ff:description "A [donation](./donation) has been cancelled" .
    eventtype:META_UPDATE_CHARITY ff:description "A [charity](./charity) has been updated" .
    eventtype:CONV_INCREASE_CASH ff:description "A non-donation increase in cash has been registered" .
    eventtype:META_CHARITY_PARTITION ff:description "A [theme-charity](./theme) is partitioned over other [charities](./charity)" .
    eventtype:PRICE_INFO ff:description "Price information has been registered" .
}
