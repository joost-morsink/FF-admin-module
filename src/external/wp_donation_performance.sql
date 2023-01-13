create table wp_donation_performance (
    Donation_Id	bigint(20) unsigned	,
    Donor_Id	bigint(20) unsigned	,
    Option_Id	bigint(20) unsigned	,
    Charity_Id	bigint(20) unsigned	,
    Currency	varchar(3)	,
    Exchanged_Amount	decimal(16,4) unsigned	,
    Has_Entered	varchar(5)	,
    Worth_Amount	decimal(16,4) unsigned	,
    Allocated_Amount	decimal(16,4) unsigned	,
    Transferred_Amount	decimal(16,4) unsigned	,
    Create_DateTime	datetime
);
