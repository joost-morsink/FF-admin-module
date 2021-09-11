/*
drop schema ff cascade;
update ev.event set processed = FALSE
drop schema ev cascade;
*/
create schema if not exists ev;
create schema if not exists ff;

create sequence if not exists ev.event_seq;

create table if not exists ev.event (
	event_id int primary key not null default nextval('ev.event_seq'),
	type varchar(32) not null,
	timestamp timestamp not null,
	name varchar(256) null,
	option_currency varchar(4) null,
	reinvestment_fraction numeric(10,10) null,
	futurefund_fraction numeric(10,10) null,
	charity_fraction numeric(10,10) null,
	bad_year_fraction numeric(10,10) null,
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
	exchanged_transfer_amount numeric(20,4) null,
	processed boolean not null default FALSE
);

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

create sequence if not exists ff.charity_seq;
create table if not exists ff.charity (
	charity_id int primary key not null default nextval('ff.charity_seq'),
	charity_ext_id varchar(32) not null,
	name varchar(32) not null,
	bank_name varchar(256) null,
	bank_account_no varchar(64) null,
	bank_nic varchar(32) null
);

create sequence if not exists ff.option_seq;
create table if not exists ff.option (
	option_id int primary key not null default nextval('ff.option_seq'),
	option_ext_id varchar(32) not null,
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
	exit_ideal_valudation numeric(20,4) null
);

create sequence if not exists ff.donation_seq;
create table if not exists ff.donation (
	donation_id int primary key not null default nextval('ff.donation_seq'),
	donation_ext_id varchar(32) not null,
	donor_id varchar(32) not null,
	currency varchar(4) not null,
	amount numeric(16,4) not null,
	exchanged_amount numeric(16,4) not null,
	option_id int not null references option(option_id),
	charity_id int not null references charity(charity_id),
	entered timestamp null
);

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


