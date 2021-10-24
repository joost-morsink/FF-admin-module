/* 
drop view if exists ff.web_export cascade;
drop type if exists ff.charity_transfer cascade;
drop function if exists ff.bank_transfers cascade;
drop view if exists ff.audit_main cascade;

*/
drop view if exists ff.web_export cascade;

create view ff.web_export as
	select d.donation_id, d.donor_id, d.option_id, d.charity_id, o.currency, d.exchanged_amount,
	(o.invested_amount+o.cash_amount) * f.fraction as worth,
	sum(a.amount * af.fraction) as allocated
	from ff.donation d
	join ff.option o on d.option_id = o.option_id
	join ff.fraction f on d.donation_id = f.donation_id and o.fractionset_id = f.fractionset_id
	join ff.allocation a on d.charity_id = a.charity_id and d.option_id = a.option_id
	join ff.fraction af on a.fractionset_id = af.fractionset_id and af.donation_id = d.donation_id
	group by d.donation_id, d.donor_id, d.option_id, d.charity_id, o.currency, d.exchanged_amount,
	(o.invested_amount+o.cash_amount) * f.fraction
	;

do $$
BEGIN
	if not exists (select * from pg_catalog.pg_type t
		join pg_catalog.pg_namespace ns on t.typnamespace = ns.oid
		where t.typname = 'charity_transfer' and ns.nspname = 'ff') THEN

		create type ff.charity_transfer as (charity_id int, currency varchar(4), amount numeric(20,4),name varchar(256), bank_name varchar(256), bank_account_no varchar(64), bank_bic varchar(32));
	END IF;
END;
$$ LANGUAGE plpgsql;

create or replace function ff.bank_transfers(threshold numeric(16,4)) returns setof ff.charity_transfer as $$
begin
	return query select c.charity_id, currency, amount, name, bank_name, bank_account_no, bank_bic
	from ff.calculate_open_transfers(threshold) t
	join ff.charity c on c.charity_id = t.charity_id;
end; $$ LANGUAGE plpgsql;

drop view if exists ff.audit_main cascade;

create view ff.audit_main as
	select 'Total number of events' as name, (select count(*) from core.event where processed=TRUE)::numeric as value
	union all select 'Unprocessed events', (select count(*) from core.event where processed=FALSE)
	union all select 'Total number of charities', (select count(*) from ff.charity)
	union all select 'Total number of donations', (select count(*) from ff.donation)
	union all select 'Donation total (' || d.currency || ')', sum(d.amount)
		from ff.donation d 
		group by d.currency
	union all select 'Unentered donations (' || d.currency || ')', sum(d.amount)
		from ff.donation d
		where d.entered is null 
		group by d.currency
	union all select 'Worth (' || currency || ')', sum(invested_amount+cash_amount)
		from ff.option
		group by currency
	union all select 'Allocated (' || currency || ')', sum(amount)
		from ff.allocation a
	    join ff.option o on a.option_id = o.option_id
		group by currency
	union all select 'Transferred (' || currency || ')', sum(amount)
		from ff.transfer
		group by currency
	;
	
	select * from ff.audit_main
	
/*
	select * from ff.web_export;
	select * from ff.transfer;
	
	select * from ff.bank_transfers(2);
	
	 select *
		from ff.allocation
*/
	
	