/*
drop function core.import_events;
drop function core.import_event;
drop function core.import_dona_new;
drop function core.import_meta_new_charity;
drop function core.import_meta_new_option;
drop function core.import_meta_update_fractions;
drop function core.import_price_info;
drop function core.import_conv_enter;
drop function core.import_conv_invest;
drop function core.import_conv_liquidate;
drop function core.import_conv_exit;
drop function core.import_conv_transfer;
*/
create or replace function core.import_events(events jsonb[]) returns core.message as $$
DECLARE
	i int;
	n int;
	j jsonb;
	res core.message;
BEGIN
	foreach j in array events loop
		n := n + 1;
		res := (select core.import_event(j));
		if(res.status > 3) THEN
			return ROW(res.status, n || '.' || res.key, res.message);
		END IF;
	end loop;
	return res;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_event(eventdata jsonb) returns core.message as $$
DECLARE
	typ varchar(32);
	res core.message;
BEGIN
	typ := (select eventdata->>'Type');
	res := CASE typ
				WHEN 'DONA_NEW' THEN (select core.import_dona_new(eventdata))
				WHEN 'META_NEW_CHARITY' THEN (select core.import_meta_new_charity(eventdata))
				WHEN 'META_NEW_OPTION' THEN (select core.import_meta_new_option(eventdata))
				WHEN 'META_UPDATE_FRACTIONS' THEN (select core.import_meta_update_fractions(eventdata))
				WHEN 'PRICE_INFO' THEN (select core.import_price_info(eventdata))
				WHEN 'CONV_ENTER' THEN (select core.import_conv_enter(eventdata))
				WHEN 'CONV_INVEST' THEN (select core.import_conv_invest(eventdata))
				WHEN 'CONV_LIQUIDATE' THEN (select core.import_conv_liquidate(eventdata))
				WHEN 'CONV_EXIT' THEN (select core.import_conv_exit(eventdata))
				WHEN 'CONV_TRANSFER' THEN (select core.import_conv_transfer(eventdata))
				ELSE ROW(4, 'Type', 'Unknown type ' || typ)::core.message END;
	return res;
END;
$$ LANGUAGE plpgsql;

create or replace function core.import_dona_new(eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
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
	select eventdata->>'Timestamp'
		, eventdata->>'Donation'
		, eventdata->>'Donor'
		, eventdata->>'Charity'
		, eventdata->>'Option'
		, eventdata->>'Currency'
		, eventdata->>'Amount'
		, eventdata->>'Exchanged_amount'
		, eventdata->>'Transaction_reference'
		, eventdata->>'Exchange_reference'
		into timestamp, donation_id, donor_id, charity_id, option_id, donation_currency, donation_amount
			, exchanged_donation_amount, transaction_reference, exchange_reference;
	IF timestamp is null or donation_id is null or donor_id is null or charity_id is null or option_id is null 
		or donation_currency is null or donation_amount is null THEN
		return ROW(4,'','Missing data in DONA_NEW event')::core.message;
	END IF;
	INSERT INTO core.event(type,timestamp, donation_id, donor_id, charity_id, option_id, donation_currency, donation_amount, 
						   exchanged_donation_amount, transaction_reference, exchange_reference)
					VALUES ('DONA_NEW', timestamp, donation_id, donor_id, charity_id, option_id, donation_currency, donation_amount, 
						   exchanged_donation_amount, transaction_reference, exchange_reference);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_meta_new_charity(eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
	charity_id varchar(16);
	name varchar(256);
BEGIN
	select eventdata->>'Timestamp'
		, eventdata->>'Code'
		, eventdata->>'Name'
		into timestamp, charity_id, name;
	IF timestamp is null or charity_id is null or name is null THEN
		return ROW(4,'','Missing data in META_NEW_CHARITY event');
	END IF;
	INSERT INTO core.event(type, timestamp, name, charity_id)
		VALUES ('META_NEW_CHARITY', timestamp, name, charity_id);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_meta_new_option(eventdata jsonb) returns core.message as $$
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
	select eventdata->>'Timestamp'
		, eventdata->>'Code'
		, eventdata->>'Name'
		, eventdata->>'Currency'
		, eventdata->>'Reinvestment_fraction'
		, eventdata->>'FutureFund_fraction'
		, eventdata->>'Charity_fraction'
		, eventdata->>'Bad_year_fraction'
		into timestamp, option_id, name, option_currency, reinvestment_fraction, futurefund_fraction, charity_fraction, bad_year_fraction;
		
	IF timestamp is null or option_id is null or name is null or option_currency is null 
	or reinvestment_fraction is null or futurefund_fraction is null or charity_fraction is null 
	or bad_year_fraction is null THEN
		return ROW(4,'','Missing data in META_NEW_OPTION event');
	END IF;
	INSERT INTO core.event(type,timestamp, option_id, name, option_currency, reinvestment_fraction, 
						   futurefund_fraction, charity_fraction, bad_year_fraction)
					VALUES ('META_NEW_OPTION', timestamp, option_id, name, option_currency, reinvestment_fraction, 
							futurefund_fraction, charity_fraction, bad_year_fraction);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_meta_update_fractions(eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
	option_id varchar(16);
	reinvestment_fraction numeric(10,10);
	futurefund_fraction numeric(10,10);
	charity_fraction numeric(10,10);
	bad_year_fraction numeric(10,10);
BEGIN
	select eventdata->>'Timestamp'
		, eventdata->>'Code'
		, eventdata->>'Reinvestment_fraction'
		, eventdata->>'FutureFund_fraction'
		, eventdata->>'Charity_fraction'
		, eventdata->>'Bad_year_fraction'
		into timestamp, option_id, reinvestment_fraction, futurefund_fraction, charity_fraction, bad_year_fraction;
		
	IF timestamp is null or option_id is null
	or reinvestment_fraction is null or futurefund_fraction is null or charity_fraction is null 
	or bad_year_fraction is null THEN
		return ROW(4,'','Missing data in META_UPDATE_FRACTIONS event');
	END IF;
	INSERT INTO core.event(type,timestamp, option_id, reinvestment_fraction, 
						   futurefund_fraction, charity_fraction, bad_year_fraction)
					VALUES ('META_UPDATE_FRACTIONS', timestamp, option_id, reinvestment_fraction, 
							futurefund_fraction, charity_fraction, bad_year_fraction);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_price_info(eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
	option_id varchar(16);
	invested_amount numeric(20,4);
	cash_amount numeric(20,4);
BEGIN
	select eventdata->>'Timestamp'
		, eventdata->>'Option'
		, eventdata->>'Invested_amount'
		, eventdata->>'Cash_amount'
		into timestamp, option_id, invested_amount, cash_amount;
	IF timestamp is null or option_id is null or invested_amount is null or cash_amount is null THEN
		return ROW(4,'','Missing data in PRICE_INFO event');
	END IF;
	INSERT INTO core.event(type, timestamp, option_id, invested_amount, cash_amount)
				VALUES('PRICE_INFO', timestamp, option_id, invested_amount, cash_amount);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_conv_enter(eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
	option_id varchar(16);
	invested_amount numeric(20,4);
BEGIN
	select eventdata->>'Timestamp'
		, eventdata->>'Option'
		, eventdata->>'Invested_amount'
		into timestamp, option_id, invested_amount;
	IF timestamp is null or option_id is null or invested_amount is null  THEN
		return ROW(4,'','Missing data in CONV_ENTER event');
	END IF;
	INSERT INTO core.event(type, timestamp, option_id, invested_amount)
				VALUES('CONV_ENTER', timestamp, option_id, invested_amount);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_conv_invest(eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
	option_id varchar(16);
	invested_amount numeric(20,4);
	cash_amount numeric(20,4);
	transaction_reference varchar(128);
BEGIN
	select eventdata->>'Timestamp'
		, eventdata->>'Option'
		, eventdata->>'Invested_amount'
		, eventdata->>'Cash_amount'
		, eventdata->>'Transaction_reference'
		into timestamp, option_id, invested_amount, cash_amount;
	IF timestamp is null or option_id is null or invested_amount is null or cash_amount is null THEN
		return ROW(4,'','Missing data in CONV_INVEST event');
	END IF;
	INSERT INTO core.event(type, timestamp, option_id, invested_amount, cash_amount, transaction_reference)
				VALUES('CONV_INVEST', timestamp, option_id, invested_amount, cash_amount, transaction_reference);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_conv_liquidate(eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
	option_id varchar(16);
	invested_amount numeric(20,4);
	cash_amount numeric(20,4);
	transaction_reference varchar(128);
BEGIN
	select eventdata->>'Timestamp'
		, eventdata->>'Option'
		, eventdata->>'Invested_amount'
		, eventdata->>'Cash_amount'
		, eventdata->>'Transaction_reference'
		into timestamp, option_id, invested_amount, cash_amount;
	IF timestamp is null or option_id is null or invested_amount is null or cash_amount is null THEN
		return ROW(4,'','Missing data in CONV_LIQUIDATE event');
	END IF;
	INSERT INTO core.event(type, timestamp, option_id, invested_amount, cash_amount, transaction_reference)
				VALUES('CONV_LIQUIDATE', timestamp, option_id, invested_amount, cash_amount, transaction_reference);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_conv_exit(eventdata jsonb) returns core.message as $$
DECLARE
	res core.message;
	timestamp timestamp;
	option_id varchar(16);
	charity_id varchar(16);
	exit_amount numeric(20,4);
BEGIN
	select eventdata->>'Timestamp'
		, eventdata-->>'Option'
		, eventdata-->>'Charity'
		, eventdata-->>'Amount'
		into timestamp, option_id, charity_id, exit_amount;
	IF timestamp is null or option_id is null or charity_id is null or exit_amount is null THEN
		return ROW(4,'','Missing data in CONV_EXIT event');
	END IF;
	INSERT INTO core.event(type, timestamp, option_id, charity_id, exit_amount)
					VALUES('CONV_EXIT', timestamp, option_id, charity_id, exit_amount);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

create or replace function core.import_conv_transfer(eventdata jsonb) returns core.message as $$
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
	select eventdata->>'Timestamp'
		, eventdata->>'Charity'
		, eventdata->>'Currency'
		, eventdata->>'Amount'
		, eventdata->>'Exchanged_currency'
		, eventdata->>'Exchanged_amount'
		, eventdata->>'Transaction_reference'
		, eventdata->>'Exchange_reference'
		into timestamp, charity_id, transfer_currency, transfer_amount, exchanged_currency
						   , exchanged_amount, transaction_reference, exchange_reference;
	IF timestamp is null or charity_id is null or transfer_currency is null or transfer_amount is null THEN
		return ROW(4,'','Missing data in CONV_TRANSFER event');
	END IF;
	INSERT INTO core.event(type, timestamp, charity_id, transfer_currency, transfer_amount, exchanged_currency
						  , exchanged_amount, transaction_reference, exchange_reference)
				VALUES ('CONV_TRANSFER', timestamp, charity_id, transfer_currency, transfer_amount, exchanged_currency
						  , exchanged_amount, transaction_reference, exchange_reference);
	return ROW(0,'','OK')::core.message;
END; $$ LANGUAGE plpgsql;

/*
Dummy data:

select * from core.import_events(ARRAY[
'{ "Type": "META_NEW_OPTION", "Timestamp":"2021-09-14T07:06:00Z", "Code":"1", "Name":"Default ABN Amro fund", "Currency":"EUR", "Reinvestment_fraction":0.45, "FutureFund_fraction":0.1, "Charity_fraction":0.45, "Bad_year_fraction":0.01 }'::jsonb
,'{ "Type": "META_NEW_CHARITY", "Timestamp":"2021-09-14T07:57:00.00001Z", "Name": "World Wildlife Fund", "Code":"1" }'::jsonb
,'{ "Type": "DONA_NEW", "Timestamp":"2021-09-14T07:58:00.123456Z", "Donation":"1", "Donor":"1", "Charity":"1", 
								   "Option":"1", "Currency":"EUR", "Amount":10.00 }'::jsonb]);

select * from core.event;
*/
