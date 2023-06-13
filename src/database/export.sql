/* 
drop view if exists ff.web_export cascade;
drop type if exists ff.charity_transfer cascade;
drop function if exists ff.bank_transfers cascade;
drop function if exists ff.select_audit cascade;
drop function if exists ff.new_audit cascade;
drop function if exists ff.audit_for_currency cascade;
drop function if exists report.record_web_export_history cascade;
drop function if exists report.record_web_export_history_when_reportable cascade;
drop view if exists ff.web_export cascade;
*/

create or replace view ff.web_export as
	select d.donation_ext_id donation_id, d.donor_id, o.option_ext_id option_id, c.charity_ext_id charity_id, o.currency, d.exchanged_amount,
	(min(f.fraction) is not null) as has_entered,
	coalesce((o.invested_amount+o.cash_amount) * f.fraction, d.exchanged_amount) as worth,
	atd.allocated,
	atd.transferred,
	atd.ff_allocated,
	atd.ff_transferred
	from ff.donation d
	join ff.option o on d.option_id = o.option_id
	join ff.charity c on d.charity_id = c.charity_id
    left join ff.fraction f on o.fractionset_id = f.fractionset_id and d.donation_id = f.donation_id
	left join ff.calculate_allocations_and_transfers_per_donation() atd
        on d.donation_id = atd.donation_id
    group by d.donation_ext_id, d.donor_id, o.option_ext_id, c.charity_ext_id, o.currency, d.exchanged_amount,
         coalesce((o.invested_amount+o.cash_amount) * f.fraction, d.exchanged_amount),
         atd.allocated, atd.transferred, atd.ff_allocated, atd.ff_transferred
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


create or replace function audit.audit_for_currency(paudit_id int, pcurrency varchar(4)) returns audit.financial as $$
declare
	res audit.financial;
begin
	res.audit_id := paudit_id;
	res.currency := pcurrency;
	select sum(amount)
		, sum(case when cancelled = TRUE then amount else 0 end)
		, sum(case when entered is null then amount else 0 end)
		into res.donation_amount, res.cancelled_donation_amount, res.unentered_donation_amount
		from ff.donation d
		join ff.option o on d.option_id = o.option_id
		where o.currency = pcurrency;
	select sum(invested_amount), sum(cash_amount)
		into res.invested_amount, res.cash_amount
		from ff.option o 
		where o.currency = pcurrency;
	select sum(amount)
		into res.allocated_amount
		from ff.allocation a
	    join ff.option o on a.option_id = o.option_id
		where o.currency = pcurrency;
	select sum(amount)
		into res.transferred_amount
		from ff.transfer
		where currency = pcurrency;
	select ARRAY(select ROW(case when exchanged_currency <> '' then exchanged_currency else currency end, sum(coalesce(exchanged_amount, amount)), sum(amount))::audit.transfers
					 from ff.transfer 
					 where currency = pcurrency
					 group by case when exchanged_currency <> '' then exchanged_currency else currency end)
			 into res.transfers;
	return res;
end; $$ LANGUAGE plpgsql;
create or replace function audit.select_audit() returns table (main audit.main, financials audit.financial[]) as 
$$
	select a as main, ARRAY(select f from audit.financial f where f.audit_id = a.audit_id) as financials
	from audit.main a;
$$ LANGUAGE SQL;

create or replace function audit.new_audit(phashcode varchar(128), ptimestamp timestamp) returns table (main audit.main, financials audit.financial[]) as $$
declare
	aid int;
	main audit.main;
	financial audit.financial;
	financials audit.financial[];
begin
	aid := nextval('audit.main_seq');
	select aid, phashcode, count(*), count(distinct charity_id), count(distinct donor_id) 
		into main.audit_id, main.hashcode, main.num_donations, main.num_charities, main.num_donors
		from ff.donation;
	select count(*), sum(case when processed=true then 1 else 0 end)
		into main.num_events, main.num_processed_events
		from core.event;
		
	insert into audit.main(audit_id, hashcode, timestamp, num_events, num_processed_events, num_donations, num_charities, num_donors)
		select main.audit_id, main.hashcode, ptimestamp, main.num_events, main.num_processed_events, main.num_donations, main.num_charities, main.num_donors;
		
	insert into audit.financial(audit_id, currency, donation_amount, cancelled_donation_amount, unentered_donation_amount,
		invested_amount, cash_amount, allocated_amount, transferred_amount, transfers)
		select f.audit_id, f.currency, f.donation_amount, f.cancelled_donation_amount, f.unentered_donation_amount,
			f.invested_amount, f.cash_amount, f.allocated_amount, f.transferred_amount, f.transfers
			from ff.option o
			join lateral audit.audit_for_currency(aid, o.currency) f on true;
	return query select a.main, a.financials from audit.select_audit() a where (a.main).audit_id = aid;
end; $$ LANGUAGE plpgsql;

/*
	select * from ff.web_export where charity_id=195;
	select * from ff.transfer;
	select * from audit.new_audit();
	select * from audit.main;
	select * from ff.bank_transfers(2);
	select * from audit.select_audit();
	 select *
		from ff.allocation
		
		select * from fraction where donation_id in (506,508) or fractionset_id in (175)
*/

create or replace function report.record_web_export_history(attime timestamp) returns int as $$
    declare
        id int;
    begin
        id := nextval('report.web_export_history_seq');
        delete from report.web_export_history_donation where web_export_history_id in (select web_export_history_id from report.web_export_history where attime - timestamp < '1D');
        delete from report.web_export_history where attime - timestamp < '1D';
        insert into report.web_export_history(web_export_history_id, timestamp) values (id, attime);
        insert into report.web_export_history_donation(web_export_history_id, donation_id, donor_id, option_id, charity_id, currency, exchanged_amount, has_entered, worth, allocated, transferred, ff_allocated, ff_transferred)
        select id, donation_id, donor_id, option_id, charity_id, currency, exchanged_amount, has_entered, worth, allocated, transferred, ff_allocated, ff_transferred
        from ff.web_export;
        return id;
    end;
    $$ LANGUAGE plpgsql;

create or replace function report.record_web_export_history_when_reportable(pEvent_id int, attime timestamp) returns int as $$
    begin
        IF pEvent_id in (select event_id from report.reportable_events) THEN
            return report.record_web_export_history(attime);
        ELSE
            return null;
        END IF;
    end;
$$ LANGUAGE plpgsql;
