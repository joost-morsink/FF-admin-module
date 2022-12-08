/*
drop function if exists ff.calculate_ideal_valuation cascade;
drop function if exists ff.calculate_exit cascade;
drop type if exists ff.open_transfer cascade;
drop function if exists ff.calculate_open_transfers cascade;
drop type if exists ff.open_transfer_per_allocation cascade;
drop function if exists ff.calculate_open_transfers_per_allocation cascade;
drop function if exists ff.calculate_allocations_and_transfers_per_donation cascade;
drop type if exists ff.allocations_and_transfers_per_donation cascade;
drop view if exists ff.separate_charity_parts cascade;
*/

create or replace function ff.calculate_ideal_valuation(opt_id int, extra_cash numeric(20,4), current_invested_amount numeric(20,4)) returns numeric(20,4) as $$
DECLARE
	new_amount numeric(20,4);
BEGIN
	select sum(exchanged_amount)
	        into new_amount
            from ff.option o
            cross join ff.donation d
            where o.option_id = opt_id
            and (o.last_exit is null and d.entered is not null or d.entered > o.last_exit);

	new_amount := coalesce(new_amount,0);
	extra_cash := coalesce(extra_cash,0);

	return (select (current_invested_amount + o.cash_amount + extra_cash - coalesce(o.exit_actual_valuation,0) - new_amount) /* profit */
			* o.reinvestment_fraction
			+ new_amount
			+ coalesce(o.exit_ideal_valuation,0)
			from ff.option o
	        where o.option_id = opt_id);
END;
$$ LANGUAGE plpgsql;

create or replace function ff.calculate_exit(opt_id int, extra_cash numeric(20,4), current_invested_amount numeric(20,4), attime timestamp) returns numeric(20,4) as $$
DECLARE
	current_ideal_valuation numeric(20,4);
	ideal numeric(20,4);
	minimal_exit numeric(20,4);
	
BEGIN
	select ff.calculate_ideal_valuation(opt_id, extra_cash, current_invested_amount) into current_ideal_valuation;
	current_ideal_valuation := coalesce(current_ideal_valuation,0);
	
	select (power(1+o.bad_year_fraction, 
				  (Date(attime) - DATE(coalesce(o.last_exit, (select min(entered) from ff.donation d where d.option_id = opt_id))))/365.0)-1)
		* (current_invested_amount+o.cash_amount)
		into minimal_exit
		from ff.option o
			where o.option_id = opt_id;
	raise info 'minimal_exit = %', minimal_exit;
	select current_invested_amount + o.cash_amount + extra_cash - current_ideal_valuation
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

create or replace function ff.calculate_open_transfers(threshold numeric(20,4)=0.01) returns SETOF ff.open_transfer as $$
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
	having sum(amount)>=threshold;
END; $$ LANGUAGE plpgsql;

do $$
BEGIN
	if not exists (select * from pg_catalog.pg_type t
		join pg_catalog.pg_namespace ns on t.typnamespace = ns.oid
		where t.typname = 'open_transfer_per_allocation' and ns.nspname = 'ff') THEN

		create type ff.open_transfer_per_allocation as (charity_id int, allocation_id int, currency varchar(4), amount numeric(20,4));
	END IF;
END;
$$ LANGUAGE plpgsql;

create or replace function ff.calculate_open_transfers_per_allocation() returns SETOF ff.open_transfer_per_allocation as $$
DECLARE
	open_transfers ff.open_transfer[];
	res ff.open_transfer_per_allocation[];
BEGIN
	open_transfers:=ARRAY(select ff.calculate_open_transfers());
	return query with data as (
		select allocation_id, charity_id, o.currency, amount, sum(amount) over(partition by charity_id order by timestamp, allocation_id desc)-amount total
		from ff.allocation a
		join ff.option o on a.option_id = o.option_id
	)
	select d.charity_id, d.allocation_id, d.currency, 
		(case when d.amount <= open_transfers[i].amount - total then d.amount
		    when total > open_transfers[i].amount then 0
			else open_transfers[i].amount - total end)::numeric(20,4) as amount
	from data d
	join generate_subscripts(open_transfers, 1) i
		on d.charity_id = open_transfers[i].charity_id and d.currency = open_transfers[i].currency
	where total <= open_transfers[i].amount;

END; $$ LANGUAGE plpgsql;

do $$
BEGIN
	if not exists (select * from pg_catalog.pg_type t
		join pg_catalog.pg_namespace ns on t.typnamespace = ns.oid
		where t.typname = 'allocations_and_transfers_per_donation' and ns.nspname = 'ff') THEN

		create type ff.allocations_and_transfers_per_donation as (donation_id int, charity_id int, allocated numeric(20,4), transferred numeric(20,4), ff_allocated numeric(20,4), ff_transferred numeric(20,4), currency varchar(4));
	END IF;
END;
$$ LANGUAGE plpgsql;

create or replace function ff.calculate_allocations_and_transfers_per_donation() returns SETOF ff.allocations_and_transfers_per_donation as $$
DECLARE
	res ff.allocations_and_transfers_per_donation[];
BEGIN
    return query
    (select d.donation_id, d.charity_id,
            coalesce(allocated, 0) :: numeric(20,4) as allocated,
            (coalesce(allocated, 0) - coalesce(not_transferred, 0)) :: numeric(20,4) as transferred,
            coalesce(ff_allocated, 0) :: numeric(20,4) as ff_allocated,
            (coalesce(ff_allocated, 0) - coalesce(ff_not_transferred, 0)) :: numeric(20,4) as ff_transferred,
            sqa.currency
    from ff.donation d
    left join (select af.donation_id, o.currency,
                      sum(a.amount * af.fraction) allocated
                  from ff.allocation a
                  join ff.fraction af on a.fractionset_id = af.fractionset_id
                  join ff.option o on a.option_id = o.option_id
                  group by af.donation_id, o.currency) sqa
        on sqa.donation_id= d.donation_id
    left join (select
                     d.donation_id,
                     sum(ota.amount*f.fraction) as not_transferred
                     from calculate_open_transfers_per_allocation() ota
                     join ff.allocation a on ota.allocation_id = a.allocation_id
                     join ff.fraction f on a.fractionset_id = f.fractionset_id
                     join ff.donation d on d.donation_id = f.donation_id
                     group by d.donation_id) sqota
        on sqota.donation_id = d.donation_id
    left join (select af.donation_id, o.currency,
                      sum(a.amount * af.fraction) ff_allocated
                  from ff.allocation a
                  join ff.charity c on a.charity_id = c.charity_id and c.charity_ext_id='FF'
                  join ff.fraction af on a.fractionset_id = af.fractionset_id
                  join ff.option o on a.option_id = o.option_id
                  group by af.donation_id, o.currency) sqf
        on sqf.donation_id= d.donation_id
    left join (select
                     d.donation_id,
                     sum(ota.amount*f.fraction) as ff_not_transferred
                     from calculate_open_transfers_per_allocation() ota
                     join ff.allocation a on ota.allocation_id = a.allocation_id
                     join ff.charity c on a.charity_id = c.charity_id and c.charity_ext_id='FF'
                     join ff.fraction f on a.fractionset_id = f.fractionset_id
                     join ff.donation d on d.donation_id = f.donation_id
                     group by d.donation_id) sqotaf
        on sqotaf.donation_id = d.donation_id);
end;
    $$ LANGUAGE plpgsql;


create or replace view ff.separate_charity_parts as
 select c.charity_id as main_charity_id
     , coalesce(p.charity_part_id ,c.charity_id) as part_charity_id
     , coalesce(fraction,1) / coalesce(sum(fraction) over (partition by c.charity_id),1) as fraction
 from ff.charity c
left join ff.charity_part p
 join ff.charity pc on p.charity_part_id = pc.charity_id
 on c.charity_id = p.charity_id;


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

select * from ff.calculate_open_transfers(5)

select * from ff.calculate_open_transfers_per_allocation() where charity_id = 193

select * from ff.transfer;
select * from ff.allocation where charity_id=193;

select * from ff.charity

select * from ff.calculate_allocations_and_transfers_per_donation();
*/