/*
drop schema ff cascade;
update core.event set processed = FALSE;
drop schema core cascade;
******************************************
truncate table ff.fraction cascade;
truncate table ff.fractionset cascade;
truncate table ff.option cascade;
truncate table ff.donation cascade;
truncate table ff.charity cascade;
truncate table ff.transfer cascade;
truncate table ff.allocation cascade;
truncate table core.event;
truncate table core.event_file;
*/
create schema if not exists core;
create schema if not exists ff;

create sequence if not exists core.event_seq;

create table if not exists core.event (
	event_id int primary key not null default nextval('core.event_seq'),
	type varchar(32) not null,
	timestamp timestamp not null,
	name varchar(256) null,
	option_currency varchar(4) null,
	reinvestment_fraction numeric(10,10) null,
	futurefund_fraction numeric(10,10) null,
	charity_fraction numeric(10,10) null,
	bad_year_fraction numeric(10,10) null,
	donation_id varchar(16) null,
	donor_id varchar(16) null,
	charity_id varchar(16) null,
	option_id varchar(16) null,
	donation_currency varchar(4) null,
	donation_amount numeric(16,4) null,
	exchanged_donation_amount numeric(16,4) null,
	transaction_reference varchar(128) null,
	exchange_reference varchar(128) null,
	invested_amount numeric(20,4) null,
	cash_amount numeric(20,4) null,
	exit_amount numeric(20,4) null,
	transfer_currency varchar(4) null,
	transfer_amount numeric(20,4) null,
	exchanged_transfer_currency varchar(4) null,
	exchanged_transfer_amount numeric(20,4) null,
	processed boolean not null default FALSE
);

create index if not exists event_timestamp on core.event (timestamp);
create unique index if not exists event_donation on core.event(donation_id);

create table if not exists core.event_file(
	path varchar(64) primary key not null
);

do $$
BEGIN
	if not exists (select * from pg_catalog.pg_type t
		join pg_catalog.pg_namespace ns on t.typnamespace = ns.oid
		where t.typname = 'message' and ns.nspname = 'core') THEN

		create type core.message as (status int, key varchar(64), message varchar(256));
	END IF;
END;
$$ LANGUAGE plpgsql;

create sequence if not exists ff.fractionset_seq;
create table if not exists ff.fractionset (
	fractionset_id int primary key not null default nextval('ff.fractionset_seq'),
	created timestamp not null
);

create sequence if not exists ff.fraction_seq;
create table if not exists ff.fraction (
	fraction_id int primary key not null default nextval('ff.fraction_seq'),
	fractionset_id int not null references fractionset(fractionset_id),
	donation_id int not null,
	fraction numeric(21,20) not null
);

create index if not exists fraction_fractionset on ff.fraction (fractionset_id);

create sequence if not exists ff.charity_seq;
create table if not exists ff.charity (
	charity_id int primary key not null default nextval('ff.charity_seq'),
	charity_ext_id varchar(32) not null,
	name varchar(256) not null,
	bank_name varchar(256) null,
	bank_account_no varchar(64) null,
	bank_bic varchar(32) null
);

create unique index if not exists charity_ext on ff.charity(charity_ext_id);

create sequence if not exists ff.option_seq;
create table if not exists ff.option (
	option_id int primary key not null default nextval('ff.option_seq'),
	option_ext_id varchar(32) not null,
	name varchar(64) not null,
	reinvestment_fraction numeric(10,10) not null,
	futurefund_fraction numeric(10,10) not null,
	charity_fraction numeric(10,10) not null,
	bad_year_fraction numeric(10,10) not null,
	currency varchar(4) not null,
	invested_amount numeric(20,4) not null,
	cash_amount numeric(20,4) not null,
	fractionset_id int null references fractionset(fractionset_id),
	last_exit timestamp null,
	exit_actual_valuation numeric(20,4) null,
	exit_ideal_valuation numeric(20,4) null
);

create unique index if not exists option_ext on ff.option (option_ext_id);

create sequence if not exists ff.donation_seq;
create table if not exists ff.donation (
	donation_id int primary key not null default nextval('ff.donation_seq'),
	timestamp timestamp not null,
	donation_ext_id varchar(32) not null,
	donor_id varchar(32) not null,
	currency varchar(4) not null,
	amount numeric(16,4) not null,
	exchanged_amount numeric(16,4) not null,
	option_id int not null references option(option_id),
	charity_id int not null references charity(charity_id),
	entered timestamp null
);

create unique index if not exists donation_ext on ff.donation(donation_ext_id);
create index if not exists donation_option on ff.donation(option_id, entered);
create index if not exists donation_charity on ff.donation(charity_id);

create sequence if not exists ff.allocation_seq;
create table if not exists ff.allocation (
	allocation_id int primary key not null default nextval('ff.allocation_seq'),
	timestamp timestamp not null,
	option_id int not null references option(option_id),
	charity_id int not null references charity(charity_id),
	fractionset_id int not null references fractionset(fractionset_id),
	amount numeric(20,4) not null,
	transferred boolean not null default FALSE
);

create index if not exists allocation_option on ff.allocation(option_id);
create index if not exists allocation_charity on ff.allocation(charity_id);

create sequence if not exists ff.transfer_seq;
create table if not exists ff.transfer (
	transfer_id int primary key not null default nextval('ff.transfer_seq'),
	timestamp timestamp not null,
	charity_id int not null references charity(charity_id),
	currency varchar(4) not null,
	amount numeric(20,4) not null,
	exchanged_currency varchar(4) null,
	exchanged_amount numeric(20,4) null
);

create index if not exists transfer_charity on ff.transfer(charity_id);

