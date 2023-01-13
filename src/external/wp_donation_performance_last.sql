create or replace view wp_donation_performance_last as 
select `wp_donation_performance`.`Donation_Id` AS `Donation_Id`
    ,`wp_donation_performance`.`Option_Id` AS `Option_Id`
    ,`wp_donation_performance`.`Charity_Id` AS `Charity_Id`
    ,`wp_donation_performance`.`Currency` AS `Currency`
    ,`wp_donation_performance`.`Exchanged_Amount` AS `Exchanged_Amount`
    ,`wp_donation_performance`.`Has_Entered` AS `Has_Entered`
    ,`wp_donation_performance`.`Worth_Amount` AS `Worth_Amount`
    ,`wp_donation_performance`.`Allocated_Amount` AS `Allocated_Amount`
    ,`wp_donation_performance`.`Transferred_Amount` AS `Transferred_Amount`
    ,`wp_donation_performance`.`Create_DateTime` AS `Create_DateTime` 
    from `wp_donation_performance`
     where (`wp_donation_performance`.`Donation_Id`,`wp_donation_performance`.`Charity_Id`,`wp_donation_performance`.`Create_DateTime`) 
        in 
        (select `wp_donation_performance`.`Donation_Id`
                ,`wp_donation_performance`.`Charity_Id`
                ,max(`wp_donation_performance`.`Create_DateTime`) 
        from `wp_donation_performance` 
        group by `wp_donation_performance`.`Donation_Id`,`wp_donation_performance`.`Charity_Id`);
        