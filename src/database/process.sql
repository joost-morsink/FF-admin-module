/*
drop function core.process_events;
drop function core.process_event;
drop function core.process_dona_new;
drop function core.process_meta_new_charity;
drop function core.process_meta_new_option;
drop function core.process_meta_update_fractions;
drop function core.process_price_info;
drop function core.process_conv_enter;
drop function core.process_conv_invest;
drop function core.process_conv_liquidate;
drop function core.process_conv_exit;
drop function core.process_conv_transfer;
*/
create or replace function ff.process_events(until timestamp) returns core.message as $$
DECLARE
	i int;
	n int;
	r core.event%rowtype;
	themax timestamp;
	first timestamp;
	res core.message;
BEGIN
	select max(timestamp) into themax from core.event where processed=TRUE;
	select min(timestamp) into first from core.event where processed=FALSE;
	IF first < themax THEN
		return ROW(4,'Timestamp','Events are out of chronological order.');
	END IF;
	for r in (
		select * from core.event e where processed = FALSE and e.timestamp <= until
		) loop
		res := (select ff.process_event(r));
		IF res.status > 3 THEN
			return ROW(res.status, event.event_id || '.' || res.key, res.message);
		END IF;
	END LOOP;
	return res;
END; $$ LANGUAGE plpgsql;

create or replace function ff.process_event(event core.event) returns core.message as $$
DECLARE
	typ varchar(32);
	res core.message;
BEGIN
	
	res := CASE event.type
				WHEN 'DONA_NEW' THEN (select ff.process_dona_new(event))
				WHEN 'META_NEW_CHARITY' THEN (select ff.process_meta_new_charity(event))
				WHEN 'META_NEW_OPTION' THEN (select ff.process_meta_new_option(event))
				WHEN 'META_UPDATE_FRACTIONS' THEN (select ff.process_meta_update_fractions(event))
				WHEN 'PRICE_INFO' THEN (select ff.process_price_info(event))
				WHEN 'CONV_ENTER' THEN (select ff.process_conv_enter(event))
				WHEN 'CONV_INVEST' THEN (select ff.process_conv_invest(event))
				WHEN 'CONV_LIQUIDATE' THEN (select ff.process_conv_liquidate(event))
				WHEN 'CONV_EXIT' THEN (select ff.process_conv_exit(event))
				WHEN 'CONV_TRANSFER' THEN (select ff.process_conv_transfer(event))
				ELSE ROW(4, 'Type', 'Unknown type ' || typ)::core.message END;
	IF res.status = 0 THEN
		update core.event ev set processed = TRUE where event_id = ev.event_id;
	END IF;
	return res;
END;
$$ LANGUAGE plpgsql;

create or replace function ff.process_dona_new(event core.event) returns core.message as $$
DEClARE
	res core.message;
BEGIN
	INSERT INTO ff.donation (donation_ext_id, donor_id, currency, amount, exchanged_amount, option_id
							 , charity_id, entered)
	select event.donation_id, event.donor_id, event.donation_currency, event.donation_amount,
		case when event.donation_currency = o.currency then event.donation_amount
		else event.exchanged_donation_amount end, o.option_id, c.charity_id, NULL
		from ff.option o 
		cross join ff.charity c
		where o.option_ext_id = event.option_id
		and c.charity_ext_id = event.charity_id
		limit 1;
	IF FOUND THEN
		return ROW(0,'','OK')::core.message;
	ELSE
		return ROW(4,'','Error in event')::core.message;
	END IF;
END;
$$ LANGUAGE plpgsql;

create or replace function ff.process_meta_new_charity(event core.event) returns core.message as $$
DEClARE
	res core.message;
BEGIN
	INSERT INTO ff.charity (charity_ext_id, name)
		VALUES (event.charity_id, event.name);
	IF FOUND THEN
		return ROW(0,'','OK')::core.message;
	ELSE
		return ROW(4,'','Error in event')::core.message;
	END IF;
END;
$$ LANGUAGE plpgsql;

create or replace function ff.process_meta_new_option(event core.event) returns core.message as $$
DEClARE
	res core.message;
BEGIN
	INSERT INTO ff.option (option_ext_id, reinvestment_fraction, futurefund_fraction
						   , charity_fraction, bad_year_fraction, currency, invested_amount
						   , cash_amount)
					VALUES(event.option_id, event.reinvestment_fraction, event.futurefund_fraction
						  , event.charity_fraction, event.bad_year_fraction, event.option_currency
						  , 0, 0);
	IF FOUND THEN
		return ROW(0,'','OK')::core.message;
	ELSE
		return ROW(4,'','Error in event')::core.message;
	END IF;
END;
$$ LANGUAGE plpgsql;

create or replace function ff.process_meta_update_fractions(event core.event) returns core.message as $$
DEClARE
	res core.message;
BEGIN
	return ROW(0,'','OK');
END;
$$ LANGUAGE plpgsql;

create or replace function ff.process_price_info(event core.event) returns core.message as $$
DEClARE
	res core.message;
BEGIN
	return ROW(0,'','OK');
END;
$$ LANGUAGE plpgsql;

create or replace function ff.process_conv_enter(event core.event) returns core.message as $$
DEClARE
	res core.message;
BEGIN
	return ROW(0,'','OK');
END;
$$ LANGUAGE plpgsql;

create or replace function ff.process_conv_invest(event core.event) returns core.message as $$
DEClARE
	res core.message;
BEGIN
	return ROW(0,'','OK');
END;
$$ LANGUAGE plpgsql;

create or replace function ff.process_conv_liquidate(event core.event) returns core.message as $$
DEClARE
	res core.message;
BEGIN
	return ROW(0,'','OK');
END;
$$ LANGUAGE plpgsql;

create or replace function ff.process_conv_exit(event core.event) returns core.message as $$
DEClARE
	res core.message;
BEGIN
	return ROW(0,'','OK');
END;
$$ LANGUAGE plpgsql;

create or replace function ff.process_conv_transfer(event core.event) returns core.message as $$
DEClARE
	res core.message;
BEGIN
	return ROW(0,'','OK');
END;
$$ LANGUAGE plpgsql;


/*
select * from ff.process_events('2021-09-14T19:07:00Z');
select * from ff.donation;
select * from ff.option;


*/


