create or replace view ff_export as select don.ID Donation_id
 , don.post_title GiveWP_id
 , amount.meta_value Donation_total
 , currency.meta_value Currency_code
 , case don.post_status
    when 'publish' then 'Complete'
    when 'give_subscription' then 'Subscription'
    else don.post_status end Donation_status
 , don.post_date_gmt Donation_datetime
 , formid.meta_value Form_id
 , formtitle.meta_value Form_title
 , donorid.meta_value Donor_id
 , fund.id Fund_id
 , fund.title Fund_title
 , fund.description Fund_description
 , trans.meta_value Transaction_id
 , gateway.meta_value Gateway
from wp_posts don
join wp_give_donationmeta amount on don.ID = amount.donation_id and amount.meta_key = '_give_payment_total'
join wp_give_donationmeta currency on don.ID = currency.donation_id and currency.meta_key = '_give_payment_currency'
join wp_give_donationmeta formid on don.ID = formid.donation_id and formid.meta_key = '_give_payment_form_id'
join wp_give_donationmeta formtitle on don.ID = formtitle.donation_id and formtitle.meta_key = '_give_payment_form_title'
join wp_give_donationmeta donorid on don.ID = donorid.donation_id and donorid.meta_key = '_give_payment_donor_id'
left join wp_give_donationmeta trans on don.ID = trans.donation_id and trans.meta_key = '_give_payment_transaction_id'
left join wp_give_donationmeta gateway on don.ID = gateway.donation_id and gateway.meta_key = '_give_payment_gateway'
join wp_give_fund_form_relationship ffr on formid.meta_value = ffr.form_id
join wp_give_funds fund on fund.id = ffr.fund_id
where don.post_type = 'give_payment'
order by don.post_date_gmt desc
;