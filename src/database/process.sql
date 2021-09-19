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
		RAISE INFO 'processing event %', r.event_id;
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
	n integer;
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
		RAISE INFO 'Setting processed to true on %', event.event_id;
		update core.event ev set processed = TRUE where ev.event_id = event.event_id;
		get diagnostics n = ROW_COUNT;
		raise info '% rows affected', n;
	END IF;
	return res;
END;
$$ LANGUAGE plpgsql;

create or replace function ff.process_dona_new(event core.event) returns core.message as $$
DEClARE
	res core.message;
BEGIN
	INSERT INTO ff.donation (donation_ext_id, timestamp, donor_id, currency, amount, exchanged_amount, option_id
							 , charity_id, entered)
	select event.donation_id, event.timestamp, event.donor_id, event.donation_currency, event.donation_amount,
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
	update ff.option set reinvestment_fraction = event.reinvestment_fraction
						, futurefund_fraction = event.futurefund_fraction
						, charity_fraction = event.charity_fraction
						, bad_year_fraction = event.bad_year_fraction
		where option_ext_id = event.option_id;
	IF FOUND THEN
		return ROW(0,'','OK')::core.message;
	ELSE
		return ROW(4,'','Error in event')::core.message;
	END IF;
END;
$$ LANGUAGE plpgsql;

create or replace function ff.process_price_info(event core.event) returns core.message as $$
DEClARE
	res core.message;
BEGIN
	update ff.option set invested_amount = event.invested_amount
						, cash_amount = event.cash_amount
		where option_ext_id = event.option_id;
	IF FOUND THEN
		return ROW(0,'','OK')::core.message;
	ELSE
		return ROW(4,'','Error in event')::core.message;
	END IF;
END;
$$ LANGUAGE plpgsql;

create or replace function ff.process_select_enter_candidates(option integer, moment timestamp) returns setof integer as $$
BEGIN
	return query
		select donation_id from donation
		where option_id = option
		and entered is null
		and timestamp < moment - interval '8 weeks';
END;
$$ LANGUAGE plpgsql;

create or replace function ff.process_conv_enter(event core.event) returns core.message as $$
DECLARE
	res core.message;
	opt_id int;
	candidates int[];
	
	old_amount numeric(20,4);
	donations_amount numeric(20,4);
	recalculate numeric(21,20);
	
	new_fractionset_id int;
BEGIN
	select option_id 
		into opt_id 
		from ff.option where option_ext_id = event.option_id;
    candidates := ARRAY(select * from ff.process_select_enter_candidates(opt_id,event.timestamp));
	IF array_length(candidates, 1) = 0 THEN
		return ROW(4,'','No donation candidates found')::core.message;
	END IF;
	
	select invested_amount + cash_amount 
		into old_amount 
		from ff.option where option_id = opt_id;
	select sum(exchanged_amount) into donations_amount
		from ff.donation 
		join generate_subscripts(candidates, 1) i on donation_id = i;
	
	IF old_amount = 0 THEN
		with inserted as(insert into ff.fractionset (created) values(current_timestamp) returning fractionset_id)
		select fractionset_id into new_fractionset_id 
			from inserted;
		
		insert into ff.fraction (fractionset_id, donation_id, fraction)
			select new_fractionset_id, i, exchanged_amount/donations_amount
				from ff.donation
				join generate_subscripts(candidates, 1) i on donation_id = i;
	ELSE
		with inserted as(insert into ff.fractionset (created) values(current_timestamp) returning fractionset_id)
		select fractionset_id into new_fractionset_id
			from inserted;
		
		recalculate := old_amount / (old_amount + donations_amount);
		insert into ff.fraction (fractionset_id, donation_id, fraction)
			select new_fractionset_id, f.donation_id, f.fraction * recalculate
			from ff.option o 
			join ff.fraction f on o.fractionset_id = f.fractionset_id;
		insert into ff.fraction (fractionset_id, donation_id, fraction)
			select new_fractionset_id, i, exchanged_amount/(old_amount + donations_amount)
				from ff.donation
				join generate_subscripts(candidates, 1) i on donation_id = i;
	END IF;
	
	update ff.option set
		invested_amount = event.invested_amount,
		cash_amount = cash_amount + donations_amount,
		fractionset_id = new_fractionset_id
		where option_id = opt_id;
	update ff.donation set
		entered = event.timestamp
		from generate_subscripts(candidates, 1) i
		where donation_id = i;	

	IF FOUND THEN
		return ROW(0,'','OK')::core.message;
	ELSE
		return ROW(4,'','Error in event')::core.message;
	END IF;
END;
$$ LANGUAGE plpgsql;

create or replace function ff.process_conv_invest(event core.event) returns core.message as $$
DEClARE
	res core.message;
BEGIN
	update ff.option set invested_amount = event.invested_amount
						, cash_amount = event.cash_amount
		where option_ext_id = event.option_id and cash_amount > event.cash_amount;
	IF FOUND THEN
		return ROW(0,'','OK')::core.message;
	ELSE
		return ROW(4,'','Error in event')::core.message;
	END IF;
END;
$$ LANGUAGE plpgsql;

create or replace function ff.process_conv_liquidate(event core.event) returns core.message as $$
DEClARE
	res core.message;
BEGIN
	update ff.option set invested_amount = event.invested_amount
						, cash_amount = event.cash_amount
		where option_ext_id = event.option_id and cash_amount < event.cash_amount;
	IF FOUND THEN
		return ROW(0,'','OK')::core.message;
	ELSE
		return ROW(4,'','Error in event')::core.message;
	END IF;
END;
$$ LANGUAGE plpgsql;

create or replace function ff.process_conv_exit(event core.event) returns core.message as $$
DEClARE
	res core.message;
BEGIN
	IF FOUND THEN
		return ROW(0,'','OK')::core.message;
	ELSE
		return ROW(4,'','Error in event')::core.message;
	END IF;
END;
$$ LANGUAGE plpgsql;

create or replace function ff.process_conv_transfer(event core.event) returns core.message as $$
DEClARE
	res core.message;
BEGIN
	IF FOUND THEN
		return ROW(0,'','OK')::core.message;
	ELSE
		return ROW(4,'','Error in event')::core.message;
	END IF;
END;
$$ LANGUAGE plpgsql;


/*
select * from ff.process_events('2021-11-16T07:30:00Z');
select * from core.event;
select * from ff.donation;
select * from ff.option;
select * from ff.fraction where fractionset_id = 4;

*/


