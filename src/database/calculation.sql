/* 
drop function ff.calculate_ideal_valuation;
drop function ff.calculate_exit;
*/

create or replace function ff.calculate_ideal_valuation(opt_id int, current_invested_amount numeric(20,4)) returns numeric(20,4) as $$
DECLARE
	new_amount numeric(20,4);
BEGIN
	select sum(exchanged_amount) into new_amount
		from ff.option o
		cross join ff.donation d
		where o.option_id = opt_id
		and (o.last_exit is null and d.entered is not null or d.entered > o.last_exit);
	new_amount := coalesce(new_amount,0);
	
	return (select (current_invested_amount + o.cash_amount - coalesce(o.exit_actual_valuation,0) - new_amount) /* profit */
			* o.reinvestment_fraction
			+ new_amount
			+ coalesce(o.exit_ideal_valuation,0)
			from ff.option o);
END;
$$ LANGUAGE plpgsql;

create or replace function ff.calculate_exit(opt_id int, current_invested_amount numeric(20,4), attime timestamp) returns numeric(20,4) as $$
DECLARE
	current_ideal_valuation numeric(20,4);
	ideal numeric(20,4);
	minimal_exit numeric(20,4);
	
BEGIN
	select ff.calculate_ideal_valuation(opt_id, current_invested_amount) into current_ideal_valuation;
	current_ideal_valuation := coalesce(current_ideal_valuation,0);
	
	select (power(1+o.bad_year_fraction, 
				  (Date(attime) - DATE(coalesce(o.last_exit, (select min(entered) from ff.donation d where d.option_id = opt_id))))/365.0)-1)
		* (current_invested_amount+o.cash_amount)
		into minimal_exit
		from ff.option o
			where o.option_id = opt_id;
	raise info 'minimal_exit = %', minimal_exit;
	select current_invested_amount + o.cash_amount - current_ideal_valuation
		into ideal
		from ff.option o
		where o.option_id=opt_id;
	
	IF ideal < minimal_exit THEN
		return minimal_exit;
	ELSE
		return ideal;
	END IF;
END;
$$ LANGUAGE plpgsql;

do $$
BEGIN
	if not exists (select * from pg_catalog.pg_type t
		join pg_catalog.pg_namespace ns on t.typnamespace = ns.oid
		where t.typname = 'open_transfer' and ns.nspname = 'ff') THEN

		create type ff.open_transfer as (charity_id int, currency varchar(4), amount numeric(20,4));
	END IF;
END;
$$ LANGUAGE plpgsql;

create or replace function ff.calculate_open_transfers() returns SETOF ff.open_transfer as $$
BEGIN
	return query select charity_id as charity_id, currency, sum(amount)::numeric(20,4) as amount from
	(select c.charity_id, o.currency, a.amount
	from ff.charity c
	join ff.allocation a on c.charity_id = a.charity_id
	join ff.option o on a.option_id = o.option_id
	union all
	select c.charity_id, t.currency, -t.amount
	from ff.charity c
	join ff.transfer t on c.charity_id = t.charity_id) s
	group by charity_id, currency
	having sum(amount)>0.01;
END; $$ LANGUAGE plpgsql;
/*

do $$
declare
	res core.message;
begin
	call ff.process_events('2021-11-16T18:00:00Z', res);
	IF res.status = 0 THEN
		raise info 'OK';
	ELSE
		raise warning '%, %, %', res.status, res.key, res.message;
	END IF;
end;
$$ LANGUAGE plpgsql;

select * from ff.option;

select date('2021-09-15') - date('2021-11-15'::timestamp)

select ff.calculate_ideal_valuation(o.option_id, -3)
	from ff.option o;

select ff.calculate_exit(o.option_id, 3,'2021-12-16')
from ff.option o;

*/

