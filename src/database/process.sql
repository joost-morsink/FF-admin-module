/*
drop function ff.process_events;
drop function ff.process_event;
drop function ff.process_dona_new;
drop function ff.process_dona_cancel;
drop function ff.process_meta_new_charity;
drop function ff.process_meta_update_charity;
drop function ff.process_meta_new_option;
drop function ff.process_meta_update_fractions;
drop function ff.process_price_info;
drop function ff.process_select_enter_candidates;
drop function ff.process_conv_enter;
drop function ff.process_conv_invest;
drop function ff.process_conv_liquidate;
drop function ff.process_conv_exit_charity;
drop function ff.process_conv_exit;
drop function ff.process_conv_transfer;
drop function ff.process_audit;
*/
create or replace procedure ff.process_events(until timestamp, inout res core.message) as $$
DECLARE
	i int;
	n int;
	r core.event%rowtype;
	themax timestamp;
	filemax timestamp;
	first timestamp;
	firstfile timestamp;
BEGIN
	select max(file_timestamp), max(timestamp) into filemax, themax from core.event where processed=TRUE;
	select min(file_timestamp), min(timestamp) into firstfile, first from core.event where processed=FALSE;
	IF firstfile < filemax or firstfile = filemax and first < themax THEN
		res := ROW(4,'Timestamp','Events are out of chronological order.');
		return;
	END IF;
	for r in (
		select * from core.event e where processed = FALSE and e.timestamp <= until order by file_timestamp, timestamp
		) loop
		RAISE INFO 'processing event %', r.event_id;
		call ff.process_event(r,res);
		IF res.status > 3 THEN
			return;
		END IF;
	END LOOP;
END; $$ LANGUAGE plpgsql;

create or replace procedure ff.process_event(event core.event, inout res core.message) as $$
DECLARE
	typ varchar(32);
	n integer;
BEGIN
	
	res := CASE event.type
				WHEN 'DONA_NEW' THEN (select ff.process_dona_new(event))
				WHEN 'DONA_CANCEL' THEN (select ff.process_dona_cancel(event))
				WHEN 'META_NEW_CHARITY' THEN (select ff.process_meta_new_charity(event))
				WHEN 'META_UPDATE_CHARITY' THEN (select ff.process_meta_update_charity(event))
				WHEN 'META_NEW_OPTION' THEN (select ff.process_meta_new_option(event))
				WHEN 'META_UPDATE_FRACTIONS' THEN (select ff.process_meta_update_fractions(event))
				WHEN 'PRICE_INFO' THEN (select ff.process_price_info(event))
				WHEN 'CONV_ENTER' THEN (select ff.process_conv_enter(event))
				WHEN 'CONV_INVEST' THEN (select ff.process_conv_invest(event))
				WHEN 'CONV_LIQUIDATE' THEN (select ff.process_conv_liquidate(event))
				WHEN 'CONV_EXIT' THEN (select ff.process_conv_exit(event))
				WHEN 'CONV_TRANSFER' THEN (select ff.process_conv_transfer(event))
				WHEN 'AUDIT' THEN (select ff.process_audit(event))
				ELSE ROW(4, 'Type', 'Unknown type ' || typ)::core.message END;
	IF res.status < 4 THEN
		RAISE INFO 'Setting processed to true on %', event.event_id;
		update core.event ev set processed = TRUE where ev.event_id = event.event_id;
		get diagnostics n = ROW_COUNT;
		raise info '% rows affected', n;
		COMMIT;
	ELSE
		ROLLBACK;
	END IF;
	return;
END;
$$ LANGUAGE plpgsql;

create or replace function ff.process_dona_new(event core.event) returns core.message as $$
DEClARE
	res core.message;
BEGIN
	INSERT INTO ff.donation (donation_ext_id, timestamp, execute_timestamp, donor_id, currency, amount, exchanged_amount, option_id
							 , charity_id, entered)
	select event.donation_id, event.timestamp, event.execute_timestamp, event.donor_id, event.donation_currency, event.donation_amount,
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

create or replace function ff.process_dona_cancel(event core.event) returns core.message as $$
DEClARE
	res core.message;
BEGIN
	update ff.donation
		set cancelled = TRUE
		where event.timestamp <= execute_timestamp and event.donation_id = donation_ext_id;
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

create or replace function ff.process_meta_update_charity(event core.event) returns core.message as $$
DEClARE
	res core.message;
BEGIN
	update ff.charity 
	    set name = coalesce(event.name, name)
		, bank_account_no = coalesce(event.bank_account_no, bank_account_no)
		, bank_name = coalesce(event.bank_name, bank_name)
		, bank_bic = coalesce(event.bank_bic, bank_bic)
		where charity_ext_id = event.charity_id;
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
	INSERT INTO ff.option (option_ext_id, name, reinvestment_fraction, futurefund_fraction
						   , charity_fraction, bad_year_fraction, currency, invested_amount
						   , cash_amount)
					VALUES(event.option_id, event.name, event.reinvestment_fraction, event.futurefund_fraction
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
		and execute_timestamp < moment;
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
	
	select invested_amount + cash_amount 
		into old_amount 
		from ff.option where option_id = opt_id;
	select sum(exchanged_amount) into donations_amount
		from ff.donation 
		join generate_subscripts(candidates, 1) i on donation_id = candidates[i];
	donations_amount := coalesce(donations_amount,0);
	raise info 'old_amount = %, donations_amount = %', old_amount, donations_amount;
	
	IF old_amount = 0 THEN
		IF donations_amount > 0 THEN
			with inserted as(insert into ff.fractionset (created) values(current_timestamp) returning fractionset_id)
			select fractionset_id into new_fractionset_id 
				from inserted;

			insert into ff.fraction (fractionset_id, donation_id, fraction)
				select new_fractionset_id, candidates[i], exchanged_amount/donations_amount
					from ff.donation
					join generate_subscripts(candidates, 1) i on donation_id = candidates[i];
		END IF;
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
			select new_fractionset_id, candidates[i], exchanged_amount/(old_amount + donations_amount)
				from ff.donation
				join generate_subscripts(candidates, 1) i on donation_id = candidates[i];
	END IF;

	update ff.donation set
		entered = event.timestamp
		from generate_subscripts(candidates, 1) i
		where donation_id = candidates[i];	
	update ff.option set
		invested_amount = event.invested_amount,
		cash_amount = cash_amount + donations_amount,
		fractionset_id = new_fractionset_id
		where option_id = opt_id;
		
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
		where option_ext_id = event.option_id and cash_amount >= event.cash_amount;
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
		where option_ext_id = event.option_id and cash_amount <= event.cash_amount;
	IF FOUND THEN
		return ROW(0,'','OK')::core.message;
	ELSE
		return ROW(4,'','Error in event')::core.message;
	END IF;
END;
$$ LANGUAGE plpgsql;

create or replace function ff.process_conv_exit_charity(event core.event, opt_id int, char_id int, char_fraction numeric(21,20)) returns core.message as $$
DECLARE
	res core.message;
	new_fractionset_id int;
	total_fraction numeric(21,20);
BEGIN
	select sum(f.fraction) into total_fraction
		from ff.option o
		join ff.fraction f on o.fractionset_id = f.fractionset_id
		join ff.donation d on f.donation_id = d.donation_id
		join ff.charity c on d.charity_id = c.charity_id
		where d.charity_id = char_id
			and o.option_id = opt_id;
		
	with inserted as (
		insert into ff.fractionset(created) values (event.timestamp) returning fractionset_id
	)
	select fractionset_id into new_fractionset_id from inserted;
	
	insert into ff.fraction (fractionset_id, donation_id, fraction)
		select new_fractionset_id, d.donation_id, f.fraction / total_fraction
			from ff.option o
			join ff.fraction f on o.fractionset_id = f.fractionset_id
			join ff.donation d on f.donation_id = d.donation_id
			join ff.charity c on d.charity_id = c.charity_id
			where d.charity_id = char_id
				and o.option_id = opt_id;

	insert into ff.allocation (timestamp, option_id, charity_id, fractionset_id, amount, transferred)
		values (event.timestamp, opt_id, char_id, new_fractionset_id, char_fraction * total_fraction * event.exit_amount, FALSE);
	IF FOUND THEN
		return ROW(0,'','OK')::core.message;
	ELSE
		return ROW(4,'charity ' || char_id, 'Error in event')::core.message;
	END IF;
END;
$$ LANGUAGE plpgsql;

create or replace function ff.process_conv_exit(event core.event) returns core.message as $$
DECLARE
	res core.message;
	opt_id int;
	char_id int;
	ff_fraction numeric(21,20);

BEGIN
	select option_id, futurefund_fraction / (futurefund_fraction + charity_fraction)
		into opt_id, ff_fraction
		from ff.option where option_ext_id = event.option_id;

    IF (select cash_amount from ff.option where option_id = opt_id) < event.exit_amount THEN
		return ROW(4,'Amount','Not enough cash in option.');
	END IF;
	
	FOR char_id IN 
		select distinct charity_id 
			from ff.option o
			join ff.fraction f on o.fractionset_id = f.fractionset_id
			join ff.donation d on f.donation_id = d.donation_id
			where o.option_id = opt_id
	LOOP
		res := (select ff.process_conv_exit_charity(event, opt_id, char_id, 1-ff_fraction));
		IF res.status > 3 THEN
			return ROW(res.status, event.event_id || '.' || res.key, res.message);
		END IF;
	END LOOP;

	select charity_id into char_id
		from ff.charity
		where charity_ext_id = 'FF';
		
	insert into ff.allocation (timestamp, option_id, charity_id, fractionset_id, amount, transferred)
		select event.timestamp, opt_id, char_id, o.fractionset_id, ff_fraction * event.exit_amount, FALSE
			from ff.option o
			where o.option_id = opt_id;
	
	update ff.option o
		set cash_amount = o.cash_amount - event.exit_amount
		, last_exit = event.timestamp
		, exit_ideal_valuation = ff.calculate_ideal_valuation(opt_id, o.invested_amount)
		, exit_actual_valuation = o.invested_amount + o.cash_amount - event.exit_amount
		where o.option_id=opt_id;
		
	IF FOUND THEN
		return ROW(0,'','OK')::core.message;
	ELSE
		return ROW(4,'','Error in event')::core.message;
	END IF;
END;
$$ LANGUAGE plpgsql;

create or replace function ff.process_conv_transfer(event core.event) returns core.message as $$
BEGIN
	INSERT INTO ff.transfer (timestamp, charity_id, currency, amount, exchanged_currency, exchanged_amount) 
		select event.timestamp, charity_id, event.transfer_currency, event.transfer_amount, event.exchanged_transfer_currency, event.exchanged_transfer_amount
			from ff.charity c
			where c.charity_ext_id = event.charity_id; 
	IF FOUND THEN
		return ROW(0,'','OK')::core.message;
	ELSE
		return ROW(4,'','Error in event')::core.message;
	END IF;
END;
$$ LANGUAGE plpgsql;

create or replace function ff.process_audit(event core.event) returns core.message as $$
BEGIN
	perform audit.new_audit(event.hashcode, event.timestamp);
	IF FOUND THEN
		return ROW(0,'','OK')::core.message;
	ELSE
		return ROW(4,'','Error in event')::core.message;
	END IF;
END;
$$ LANGUAGE plpgsql;
/*

do $$
declare
	res core.message;
begin
	call ff.process_events('2021-12-17T19:00:00Z', res);
	IF res.status = 0 THEN
		raise info 'OK';
	ELSE
		raise warning '%, %, %', res.status, res.key, res.message;
	END IF;
end;
$$ LANGUAGE plpgsql;
		
select *
		from ff.option o
		cross join ff.donation d
		where (o.last_exit is null and d.entered is not null or d.entered > o.last_exit);

select * from core.event;
select * from ff.donation;
select * from ff.option;
select * from ff.fraction where fractionset_id = 4;
select * from ff.fraction;
select * from ff.allocation;
select * from ff.charity;
*/


