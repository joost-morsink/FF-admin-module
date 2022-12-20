/*
drop function core.import_events;
drop function core.import_event;
drop function core.import_dona_new;
drop function core.import_dona_update_charity;
drop function core.import_dona_cancel;
drop function core.import_meta_new_charity;
drop function core.import_meta_update_charity;
drop function core.import_meta_new_option;
drop function core.import_meta_update_fractions;
drop function core.import_price_info;
drop function core.import_conv_enter;
drop function core.import_conv_invest;
drop function core.import_conv_liquidate;
drop function core.import_conv_exit;
drop function core.import_conv_transfer;
drop function core.import_conv_increase_cash;
drop function core.import_audit;
drop function core.parse_fraction_specs;
drop function core.parse_fraction_spec;
*/

create or replace function core.parse_fraction_spec(dat json) returns core.s_fraction_spec as $$
declare
    holder varchar(16);
    fraction numeric(20,10);
begin
    select dat->>'holder', cast(dat->>'fraction' as numeric(20,10))
        into holder,fraction;
    return ROW(holder,fraction)::core.s_fraction_spec;
end;
$$ LANGUAGE plpgsql;

create or replace function core.parse_fraction_specs(dat json) returns core.s_fraction_spec[] as $$
declare
    holder varchar(16);
    fraction numeric(20,10);
    result core.s_fraction_spec[];
    x json;
begin
    result := Array[]::core.s_fraction_spec[];
    for x in select value from json_array_elements(dat) loop
        result := result || core.parse_fraction_spec(x);
    end loop;

    return result;
end;
$$ LANGUAGE plpgsql;

create or replace function core.import_events(file_timestamp timestamp, events jsonb[]) returns core.message as $$
DECLARE
	i int;
	n int;
	j jsonb;
	res core.message;
BEGIN
	foreach j in array events loop
		n := n + 1;
		res := (select core.import_event(file_timestamp, j));
		if(res.status > 3) THEN
			return ROW(res.status, n || '.' || res.key, res.message)::core.message;
		END IF;
	end loop;
	return res;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_event(file_timestamp timestamp, eventdata jsonb) returns core.message as $$
DECLARE
	typ varchar(32);
	res core.message;
BEGIN
	typ := (select eventdata->>'type');
	res := CASE typ
				WHEN 'DONA_NEW' THEN (select core.import_dona_new(file_timestamp ,eventdata))
				WHEN 'DONA_UPDATE_CHARITY' THEN (select core.import_dona_update_charity(file_timestamp ,eventdata))
				WHEN 'DONA_CANCEL' THEN (select core.import_dona_cancel(file_timestamp ,eventdata))
				WHEN 'META_NEW_CHARITY' THEN (select core.import_meta_new_charity(file_timestamp ,eventdata))
				WHEN 'META_UPDATE_CHARITY' THEN (select core.import_meta_update_charity(file_timestamp ,eventdata))
	            WHEN 'META_CHARITY_PARTITION' THEN (select core.import_meta_charity_partition(file_timestamp, eventdata))
				WHEN 'META_NEW_OPTION' THEN (select core.import_meta_new_option(file_timestamp ,eventdata))
				WHEN 'META_UPDATE_FRACTIONS' THEN (select core.import_meta_update_fractions(file_timestamp ,eventdata))
				WHEN 'PRICE_INFO' THEN (select core.import_price_info(file_timestamp ,eventdata))
				WHEN 'CONV_ENTER' THEN (select core.import_conv_enter(file_timestamp ,eventdata))
				WHEN 'CONV_INVEST' THEN (select core.import_conv_invest(file_timestamp ,eventdata))
				WHEN 'CONV_LIQUIDATE' THEN (select core.import_conv_liquidate(file_timestamp ,eventdata))
				WHEN 'CONV_EXIT' THEN (select core.import_conv_exit(file_timestamp ,eventdata))
				WHEN 'CONV_TRANSFER' THEN (select core.import_conv_transfer(file_timestamp ,eventdata))
                WHEN 'CONV_INCREASE_CASH' THEN (select core.import_conv_increase_cash(file_timestamp, eventdata))
				WHEN 'AUDIT' THEN (select core.import_audit(file_timestamp ,eventdata))
				ELSE ROW(4, 'type', 'Unknown type ' || typ)::core.message END;
	return res;
END;
$$ LANGUAGE plpgsql;

create or replace function core.import_dona_new(file_timestamp timestamp,eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
	execute_timestamp timestamp;
	donation_id varchar(16);
	donor_id varchar(16);
	charity_id varchar(16);
	option_id varchar(16);
	donation_currency varchar(4);
	donation_amount numeric(16,4);
	exchanged_donation_amount numeric(16,4);
	transaction_reference varchar(128);
	exchange_reference varchar(128);
BEGIN
	select eventdata->>'timestamp'
		, eventdata->>'execute_timestamp'
		, eventdata->>'donation'
		, eventdata->>'donor'
		, eventdata->>'charity'
		, eventdata->>'option'
		, eventdata->>'currency'
		, eventdata->>'amount'
		, eventdata->>'exchanged_amount'
		, eventdata->>'transaction_reference'
		, eventdata->>'exchange_reference'
		into timestamp, execute_timestamp, donation_id, donor_id, charity_id, option_id, donation_currency, donation_amount
			, exchanged_donation_amount, transaction_reference, exchange_reference;
	IF timestamp is null or donation_id is null or donor_id is null or charity_id is null or option_id is null 
		or donation_currency is null or donation_amount is null THEN
		return ROW(4,'','Missing data in DONA_NEW event')::core.message;
	END IF;
	INSERT INTO core.event(type,timestamp, file_timestamp, execute_timestamp, donation_id, donor_id, charity_id, option_id, donation_currency, donation_amount, 
						   exchanged_donation_amount, transaction_reference, exchange_reference)
					VALUES ('DONA_NEW', timestamp, file_timestamp, coalesce(execute_timestamp, timestamp), donation_id, donor_id, charity_id, option_id, donation_currency, donation_amount, 
						   exchanged_donation_amount, transaction_reference, exchange_reference);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_dona_update_charity(file_timestamp timestamp, eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
	donation_id varchar(16);
	charity_id varchar(16);
BEGIN
	select eventdata->>'timestamp'
		, eventdata->>'donation'
		, eventdata->>'charity'
		into timestamp, donation_id, charity_id;
	IF timestamp is null or donation_id is null or charity_id is null THEN
		return ROW(4,'','Missing data in DONA_UPDATE_CHARITY event')::core.message;
	END IF;
	INSERT INTO core.event(type,timestamp, file_timestamp, donation_id, charity_id)
					VALUES ('DONA_UPDATE_CHARITY', timestamp, file_timestamp, donation_id, charity_id);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_meta_charity_partition(file_timestamp timestamp, eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
	charity_id varchar(16);
    partitions core.s_fraction_spec[];
BEGIN
	select eventdata->>'timestamp'
		, eventdata->>'charity'
	    , core.parse_fraction_specs(eventdata->>'partitions')
		into timestamp, charity_id, partitions;
	IF timestamp is null or charity_id is null or partitions is null THEN
		return ROW(4,'','Missing data in META_CHARITY_PARTITION event')::core.message;
	END IF;
	INSERT INTO core.event(type,timestamp, file_timestamp, charity_id, partitions)
					VALUES ('META_CHARITY_PARTITION', timestamp, file_timestamp, donation_id, charity_id, partitions);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_dona_cancel(file_timestamp timestamp,eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
	donation_id varchar(16);
BEGIN
	select eventdata->>'timestamp'
		, eventdata->>'donation'
		into timestamp, donation_id;
	IF timestamp is null or donation_id is null THEN
		return ROW(4,'','Missing data in DONA_CANCEL event')::core.message;
	END IF;
	INSERT INTO core.event(type, timestamp, file_timestamp, donation_id)
					VALUES ('DONA_CANCEL', timestamp, file_timestamp, donation_id);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_meta_new_charity(file_timestamp timestamp, eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
	charity_id varchar(16);
	name varchar(256);
BEGIN
	select eventdata->>'timestamp'
		, eventdata->>'code'
		, eventdata->>'name'
		into timestamp, charity_id, name;
	IF timestamp is null or charity_id is null or name is null THEN
		return ROW(4,'','Missing data in META_NEW_CHARITY event')::core.message;
	END IF;
	INSERT INTO core.event(type, timestamp, file_timestamp, name, charity_id)
		VALUES ('META_NEW_CHARITY', timestamp, file_timestamp, name, charity_id);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_meta_update_charity(file_timestamp timestamp, eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
	charity_id varchar(16);
	bank_name varchar(256);
	bank_account_no varchar(64);
	bank_bic varchar(32);
	name varchar(256);
BEGIN
	select eventdata->>'timestamp'
		, eventdata->>'code'
		, eventdata->>'name'
		, eventdata->>'bank_account_no'
		, eventdata->>'bank_name'
		, eventdata->>'bank_bic'
		into timestamp, charity_id, name, bank_account_no, bank_name, bank_bic;
	IF timestamp is null or charity_id is null THEN
		return ROW(4,'','Missing data in META_NEW_CHARITY event')::core.message;
	END IF;
	INSERT INTO core.event(type, timestamp, file_timestamp, name, charity_id, bank_account_no, bank_name, bank_bic)
		VALUES ('META_UPDATE_CHARITY', timestamp, file_timestamp, name, charity_id, bank_account_no, bank_name, bank_bic);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_meta_new_option(file_timestamp timestamp, eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
	option_id varchar(16);
	name varchar(256);
	option_currency varchar(4);
	reinvestment_fraction numeric(10,10);
	futurefund_fraction numeric(10,10);
	charity_fraction numeric(10,10);
	bad_year_fraction numeric(10,10);
BEGIN
	select eventdata->>'timestamp'
		, eventdata->>'code'
		, eventdata->>'name'
		, eventdata->>'currency'
		, eventdata->>'reinvestment_fraction'
		, eventdata->>'futureFund_fraction'
		, eventdata->>'charity_fraction'
		, eventdata->>'bad_year_fraction'
		into timestamp, option_id, name, option_currency, reinvestment_fraction, futurefund_fraction, charity_fraction, bad_year_fraction;
		
	IF timestamp is null or option_id is null or name is null or option_currency is null 
	or reinvestment_fraction is null or futurefund_fraction is null or charity_fraction is null 
	or bad_year_fraction is null THEN
		return ROW(4,'','Missing data in META_NEW_OPTION event')::core.message;
	END IF;
	INSERT INTO core.event(type,timestamp, file_timestamp, option_id, name, option_currency, reinvestment_fraction, 
						   futurefund_fraction, charity_fraction, bad_year_fraction)
					VALUES ('META_NEW_OPTION', timestamp, file_timestamp, option_id, name, option_currency, reinvestment_fraction, 
							futurefund_fraction, charity_fraction, bad_year_fraction);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_meta_update_fractions(file_timestamp timestamp, eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
	option_id varchar(16);
	reinvestment_fraction numeric(10,10);
	futurefund_fraction numeric(10,10);
	charity_fraction numeric(10,10);
	bad_year_fraction numeric(10,10);
BEGIN
	select eventdata->>'timestamp'
		, eventdata->>'code'
		, eventdata->>'reinvestment_fraction'
		, eventdata->>'futureFund_fraction'
		, eventdata->>'charity_fraction'
		, eventdata->>'bad_year_fraction'
		into timestamp, option_id, reinvestment_fraction, futurefund_fraction, charity_fraction, bad_year_fraction;
		
	IF timestamp is null or option_id is null
	or reinvestment_fraction is null or futurefund_fraction is null or charity_fraction is null 
	or bad_year_fraction is null THEN
		return ROW(4,'','Missing data in META_UPDATE_FRACTIONS event')::core.message;
	END IF;
	INSERT INTO core.event(type,timestamp, file_timestamp, option_id, reinvestment_fraction, 
						   futurefund_fraction, charity_fraction, bad_year_fraction)
					VALUES ('META_UPDATE_FRACTIONS', timestamp, file_timestamp, option_id, reinvestment_fraction, 
							futurefund_fraction, charity_fraction, bad_year_fraction);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_price_info(file_timestamp timestamp, eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
	option_id varchar(16);
	invested_amount numeric(20,4);
	cash_amount numeric(20,4);
BEGIN
	select eventdata->>'timestamp'
		, eventdata->>'option'
		, eventdata->>'invested_amount'
		, eventdata->>'cash_amount'
		into timestamp, option_id, invested_amount, cash_amount;
	IF timestamp is null or option_id is null or invested_amount is null or cash_amount is null THEN
		return ROW(4,'','Missing data in PRICE_INFO event')::core.message;
	END IF;
	INSERT INTO core.event(type, timestamp, file_timestamp, option_id, invested_amount, cash_amount)
				VALUES('PRICE_INFO', timestamp, file_timestamp, option_id, invested_amount, cash_amount);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_conv_enter(file_timestamp timestamp, eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
	option_id varchar(16);
	invested_amount numeric(20,4);
BEGIN
	select eventdata->>'timestamp'
		, eventdata->>'option'
		, eventdata->>'invested_amount'
		into timestamp, option_id, invested_amount;
	IF timestamp is null or option_id is null or invested_amount is null  THEN
		return ROW(4,'','Missing data in CONV_ENTER event')::core.message;
	END IF;
	INSERT INTO core.event(type, timestamp, file_timestamp, option_id, invested_amount)
				VALUES('CONV_ENTER', timestamp, file_timestamp, option_id, invested_amount);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_conv_increase_cash(file_timestamp timestamp, eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
	option_id varchar(16);
	amount numeric(20,4);
BEGIN
	select eventdata->>'timestamp'
		, eventdata->>'option'
		, eventdata->>'amount'
		into timestamp, option_id, amount;
	IF timestamp is null or option_id is null or amount is null  THEN
		return ROW(4,'','Missing data in CONV_INCREASE_CASH event')::core.message;
	END IF;
	INSERT INTO core.event(type, timestamp, file_timestamp, option_id, cash_amount)
				VALUES('CONV_INCREASE_CASH', timestamp, file_timestamp, option_id, amount);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_conv_invest(file_timestamp timestamp, eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
	option_id varchar(16);
	invested_amount numeric(20,4);
	cash_amount numeric(20,4);
	transaction_reference varchar(128);
BEGIN
	select eventdata->>'timestamp'
		, eventdata->>'option'
		, eventdata->>'invested_amount'
		, eventdata->>'cash_amount'
		, eventdata->>'transaction_reference'
		into timestamp, option_id, invested_amount, cash_amount;
	IF timestamp is null or option_id is null or invested_amount is null or cash_amount is null THEN
		return ROW(4,'','Missing data in CONV_INVEST event')::core.message;
	END IF;
	INSERT INTO core.event(type, timestamp, file_timestamp, option_id, invested_amount, cash_amount, transaction_reference)
				VALUES('CONV_INVEST', timestamp, file_timestamp, option_id, invested_amount, cash_amount, transaction_reference);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_conv_liquidate(file_timestamp timestamp, eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
	option_id varchar(16);
	invested_amount numeric(20,4);
	cash_amount numeric(20,4);
	transaction_reference varchar(128);
BEGIN
	select eventdata->>'timestamp'
		, eventdata->>'option'
		, eventdata->>'invested_amount'
		, eventdata->>'cash_amount'
		, eventdata->>'transaction_reference'
		into timestamp, option_id, invested_amount, cash_amount, transaction_reference;
	IF timestamp is null or option_id is null or invested_amount is null or cash_amount is null THEN
		return ROW(4,'','Missing data in CONV_LIQUIDATE event')::core.message;
	END IF;
	INSERT INTO core.event(type, timestamp, file_timestamp, option_id, invested_amount, cash_amount, transaction_reference)
				VALUES('CONV_LIQUIDATE', timestamp, file_timestamp, option_id, invested_amount, cash_amount, transaction_reference);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_conv_exit(file_timestamp timestamp, eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
	option_id varchar(16);
	exit_amount numeric(20,4);
BEGIN
	select eventdata->>'timestamp'
		, eventdata->>'option'
		, eventdata->>'amount'
		into timestamp, option_id, exit_amount;
	IF timestamp is null or option_id is null or exit_amount is null THEN
		return ROW(4,'','Missing data in CONV_EXIT event')::core.message;
	END IF;
	INSERT INTO core.event(type, timestamp, file_timestamp, option_id, exit_amount)
					VALUES('CONV_EXIT', timestamp, file_timestamp, option_id, exit_amount);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_conv_transfer(file_timestamp timestamp, eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
	charity_id varchar(16);
	transfer_currency varchar(4);
	transfer_amount numeric(20,4);
	exchanged_currency varchar(4);
	exchanged_amount numeric(20,4);
	transaction_reference varchar(128);
	exchange_reference varchar(128);
BEGIN
	select eventdata->>'timestamp'
		, eventdata->>'charity'
		, eventdata->>'currency'
		, eventdata->>'amount'
		, eventdata->>'exchanged_currency'
		, eventdata->>'exchanged_amount'
		, eventdata->>'transaction_reference'
		, eventdata->>'exchange_reference'
		into timestamp, charity_id, transfer_currency, transfer_amount, exchanged_currency
						   , exchanged_amount, transaction_reference, exchange_reference;
	IF timestamp is null or charity_id is null or transfer_currency is null or transfer_amount is null THEN
		return ROW(4,'','Missing data in CONV_TRANSFER event')::core.message;
	END IF;
	INSERT INTO core.event(type, timestamp, file_timestamp, charity_id, transfer_currency, transfer_amount, exchanged_transfer_currency
						  , exchanged_transfer_amount, transaction_reference, exchange_reference)
				VALUES ('CONV_TRANSFER', timestamp, file_timestamp, charity_id, transfer_currency, transfer_amount, exchanged_currency
						  , exchanged_amount, transaction_reference, exchange_reference);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_audit(file_timestamp timestamp, eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
	hashcode varchar(128);
BEGIN
	select eventdata->>'timestamp'
		, eventdata->>'hashcode'
		into timestamp, hashcode;
	IF timestamp is null or hashcode is null THEN
		return ROW(4,'','Missing data in AUDIT event')::core.message;
	END IF;
	INSERT INTO core.event(type, timestamp, file_timestamp, hashcode)
				VALUES ('AUDIT', timestamp, file_timestamp, hashcode);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

/*
Dummy data:

select * from core.import_events(ARRAY[
'{ "Type": "META_NEW_OPTION", "Timestamp":"2021-09-14T07:06:00Z", "Code":"1", "Name":"Default ABN Amro fund", "Currency":"EUR", "Reinvestment_fraction":0.45, "FutureFund_fraction":0.1, "Charity_fraction":0.45, "Bad_year_fraction":0.01 }'::jsonb
,'{ "Type": "META_NEW_CHARITY", "Timestamp":"2021-09-14T07:57:00.00001Z", "Name": "Toekomstfonds", "Code":"FF" }'::jsonb
,'{ "Type": "META_NEW_CHARITY", "Timestamp":"2021-09-14T07:57:00.00001Z", "Name": "World Wildlife Fund", "Code":"1" }'::jsonb
,'{ "Type": "META_NEW_CHARITY", "Timestamp":"2021-09-14T07:57:00.00001Z", "Name": "Amnesty international", "Code":"2" }'::jsonb
,'{ "Type": "DONA_NEW", "Timestamp":"2021-09-14T07:58:00.123456Z", "Donation":"1", "Donor":"1", "Charity":"1", 
								   "Option":"1", "Currency":"EUR", "Amount":10.00 }'::jsonb
,'{ "Type": "DONA_NEW", "Timestamp":"2021-09-14T07:58:00.123456Z", "Donation":"2", "Donor":"2", "Charity":"1", 
								   "Option":"1", "Currency":"EUR", "Amount":5.00 }'::jsonb
,'{ "Type": "DONA_NEW", "Timestamp":"2021-09-14T07:58:00.123456Z", "Donation":"3", "Donor":"1", "Charity":"2", 
								   "Option":"1", "Currency":"EUR", "Amount":5.00 }'::jsonb
,'{ "Type": "META_UPDATE_FRACTIONS", "Timestamp":"2021-09-15T18:12:00Z", "Code": "1", "Reinvestment_fraction":0.55, "FutureFund_fraction":0.1, "Charity_fraction":0.35, "Bad_year_fraction":0.01 }'
,'{ "Type": "CONV_ENTER", "Timestamp":"2021-11-15T19:00:00Z", "Option":"1", "Invested_amount": 0 }'
,'{ "Type": "CONV_EXIT", "Timestamp":"2021-11-16T19:00:00Z", "Option":"1", "Amount": 2.00 }'
,'{ "Type": "CONV_TRANSFER", "Timestamp": "2021-11-17T12:00:00Z", "Charity":"1", "Currency": "EUR", "Amount": 1.10, "Exchanged_Currency": "EUR", "Exchanged_Amount": 1.50 }'
,'{ "Type": "DONA_NEW", "Timestamp": "2021-10-14T08:00:00Z", "Donation":"4", "Donor":"3", "Charity":"1",
									"Option":"1", "Currency":"EUR", "Amount": 5.00 }'
,'{ "Type": "DONA_NEW", "Timestamp": "2021-10-14T08:00:00Z", "Donation":"5", "Donor":"3", "Charity":"2",
									"Option":"1", "Currency":"EUR", "Amount": 25.00 }'
,'{ "Type": "DONA_NEW", "Timestamp": "2021-10-14T08:00:00Z", "Donation":"6", "Donor":"3", "Charity":"1",
									"Option":"1", "Currency":"EUR", "Amount": 10.00 }'
,'{ "Type": "CONV_ENTER", "Timestamp":"2021-11-16T19:00:01Z", "Option":"1", "Invested_amount": 25 }'
,'{ "Type": "PRICE_INFO", "Timestamp":"2021-11-15T20:00:00Z", "Option":"1", "Invested_amount": 3, "Cash_amount":20 }'
,'{ "Type": "CONV_EXIT", "Timestamp":"2021-12-16T19:00:00Z", "Option":"1", "Amount": 2.00 }'
,'{ "Type": "CONV_ENTER", "Timestamp":"2021-12-16T19:00:01Z", "Option":"1", "Invested_amount": 30 }'

]);
select * from core.event;
*/
