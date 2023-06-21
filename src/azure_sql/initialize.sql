/*
 drop view [ConsolidatedEvents]
 drop table [Event]
 drop table [Range]
 drop table [Branch]
 */

 if object_id('Branch') is null begin
    create table [Branch] (
        [Id] int not null identity primary key,
        [Name] varchar(64),
        constraint [u_Branch_Name] unique ([Name])
    )
     create unique index [idx_Branch_Name] on [Branch] ([Name])
end go

if object_id('Event') is null begin
    create table [Event] (
        [Branch] int not null,
        [Sequence] int not null,
        [Content] varchar(4000),
        constraint [pk_Event] primary key ([Sequence], [Branch]),
        constraint [fk_Event_Branch] foreign key ([Branch]) references [Branch]([Id]),
     )
    create index idx_Event_Sequence on [Event]([Sequence])
    create index idx_Event_Branch on [Event]([Branch], [Sequence])

end go

if object_id('Range') is null begin
    create table [Range] (
        [Branch] int not null,
        [SubBranch] int not null,
        [Min] int not null,
        [Max] int null,
        constraint [fk_Range_Branch] foreign key ([Branch]) references [Branch]([Id]),
        constraint [fk_Range_SubBranch] foreign key ([SubBranch]) references [Branch]([Id])
    )
    create index idx_Range_Branch on [Range]([Branch],[SubBranch])
    create index idx_Range_SubBranch on [Range]([SubBranch])
end go

create or alter view [ConsolidatedEvents] as
    select mb.Name as [Branch], e.Sequence, e.Content
        from [Event] as e
        join [Range] as r on e.Sequence >= r.Min and (r.Max is null or e.Sequence < r.Max) and e.Branch = r.SubBranch
        join [Branch] as mb on r.Branch = mb.Id
        join [Branch] as sb on r.SubBranch = sb.Id

