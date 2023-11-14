/*
 drop procedure [CreateNewBranch]
 drop procedure [CreateBranchFrom]
 drop procedure [RemoveBranch]
 drop procedure [AddEvent]
 drop procedure [Rebase]
 drop procedure [FastForward]
 drop view [ConsolidatedEvents]
 drop table [Event]
 drop table [Range]
 drop table [Branch]
 */
if not exists (select 1 from sys.database_principals where name='branching' and Type = 'R') begin
     create role branching;
end
go


 if object_id('Branch') is null begin
    create table [Branch] (
        [Id] int not null identity primary key,
        [Name] varchar(64),
        constraint [u_Branch_Name] unique ([Name])
    )
     create unique index [idx_Branch_Name] on [Branch] ([Name])
end;
go

if object_id('Event') is null begin
    -- Event has no foreign key to Branch, to be able to delete branches without deleting events.
    create table [Event] (
        [Branch] int not null,
        [Sequence] int not null,
        [Content] varchar(4000),
        constraint [pk_Event] primary key ([Sequence], [Branch])
     )
    create index idx_Event_Sequence on [Event]([Sequence])
    create index idx_Event_Branch on [Event]([Branch], [Sequence])
end;
go

if object_id('Range') is null begin
    -- Range has a foreign key to Branch on the Branch field only.
    -- Composition of a branch needs to be deleted when the branch is deleted.
    -- Constitution of a branch should not be deleted, as other branches may depend on it.
    create table [Range] (
        [Branch] int not null,
        [SubBranch] int not null,
        [Min] int not null,
        [Max] int null,
        constraint [fk_Range_Branch] foreign key ([Branch]) references [Branch]([Id])
    )
    create index idx_Range_Branch on [Range]([Branch],[SubBranch])
    create index idx_Range_SubBranch on [Range]([SubBranch])
end;
go

create or alter view [ConsolidatedEvents] as
    select mb.Name as [Branch], e.Sequence, e.Content
        from [Event] as e
        join [Range] as r on e.Sequence >= r.Min and (r.Max is null or e.Sequence < r.Max) and e.Branch = r.SubBranch
        join [Branch] as mb on r.Branch = mb.Id;
go
grant select on [ConsolidatedEvents] to branching
go

create or alter procedure [CreateEmptyBranch](@branchName varchar(64)) as begin
    declare @id int;
    insert into [Branch] ([Name]) values (@branchName);
    select @id = scope_identity();
    insert into [Range] ([Branch], [SubBranch], [Min], [Max])
        values (@id, @id, 0, null);
end;
go
grant execute on [CreateEmptyBranch] to branching
go

create or alter procedure [CreateBranchFrom](@newBranchName varchar(64), @sourceBranchName varchar(64)) as begin
    declare @id int;
    insert into [Branch] ([Name]) values (@newBranchName);
    select @id = scope_identity();
    insert into [Range] ([Branch], [SubBranch], [Min], [Max])
        select @id, r.[SubBranch], r.[Min], r.[Max]
            from [Range] r
            join [Branch] sb
                on r.[Branch] = sb.[Id]
        where sb.[Name] = @sourceBranchName;
    declare @maxSequence int;
    select @maxSequence =  1 + max([Sequence]) from [Event] e join [Range] r on e.[Branch] = r.[SubBranch]
            where r.[Branch] = @id and r.[Max] is null
    update [Range] set [Max] = @maxSequence
        where [Branch] = @id and [Max] is null;
    insert into [Range]([Branch], [SubBranch], [Min], [Max]) values
        (@id, @id, @maxSequence, null);
end;
go
grant execute on [CreateBranchFrom] to branching
go

create or alter procedure [RemoveBranch](@branchName varchar(64)) as begin
    delete r from [Range] r
    join [Branch] b
        on r.[Branch] = b.[Id]
    where b.[Name] = @branchName;

    delete from [Branch] where [Name] = @branchName;
end;
go
grant execute on [RemoveBranch] to branching
go

create or alter procedure [AddEvent](@branchName varchar(64), @content varchar(4000)) as begin
    declare @maxSeq int;
    select @maxSeq = isnull((select max([Sequence]) from [ConsolidatedEvents] where [Branch] = @branchName), -1)

    insert into [Event] ([Branch], [Sequence], [Content])
        select [Id], @maxSeq+1, @content from [Branch] where [Name] = @branchName
end;
go
grant execute on [AddEvent] to branching
go

create or alter procedure [Rebase](@branchName varchar(64), @onBranchName varchar(64)) as begin
    declare @onSeq int;
    select @onSeq = 1 + max([Sequence]) from [ConsolidatedEvents] where [Branch] = @onBranchName;
    declare @start int;
    select @start = isnull(max([Min]), 0) from [Range] r join [Branch] b on r.[Branch] = b.[Id] and r.[SubBranch] = b.[Id] where b.[Name] = @branchName;
    delete r from [Range] r join [Branch] b on r.[Branch] = b.[Id] where b.[Name] = @branchName;
    insert into [Range]([Branch], [SubBranch], [Min], [Max])
        select b.[Id], [SubBranch], [Min], [Max]
            from [Branch] b
            cross join [Branch] ob
                join [Range] r on ob.[Id]=r.[Branch]
            where [Max] is not null and b.[Name] = @branchName and ob.[Name] = @onBranchName
    insert into [Range]([Branch], [SubBranch], [Min], [Max])
        select b.[Id], [SubBranch], [Min], @onSeq
            from [Branch] b
            cross join [Branch] ob
                join [Range] r on ob.[Id]=r.[Branch]
            where [Max] is null and b.[Name] = @branchName and ob.[Name] = @onBranchName;
    insert into [Range]([Branch], [SubBranch], [Min], [Max])
        select [Id], [Id], @onSeq, null from [Branch] where [Name] = @branchName;
    update e set [Sequence] = [Sequence] + @onSeq - @start
             from [Event] e
             join [Branch] b on e.[Branch] = b.[Id]
             where b.[Name] = @branchName;
end;
go
grant execute on [Rebase] to branching
go


create or alter procedure [FastForward](@branchName varchar(64), @toBranchName varchar(64)) as begin
    declare @maxSeq int;
    select @maxSeq = max(Sequence) from [ConsolidatedEvents] ce where ce.[Branch] = @toBranchName;
    delete r from [Range] r join [Branch] b on r.Branch = b.Id where b.[Name] = @branchName
    insert into [Range]([Branch], [SubBranch], [Min], [Max])
        select b.[Id], [SubBranch], [Min], [Max]
            from [Branch] b
            cross join [Branch] ob
                join [Range] r on ob.[Id]=r.[Branch]
            where b.[Name] = @branchName and ob.[Name] = @toBranchName;
    update r set [Max] = @maxSeq + 1
        from [Range] r
            join [Branch] b on r.[Branch] = b.[Id]
        where r.[Max] is null and b.[Name] = @branchName;
    insert into [Range]([Branch], [SubBranch], [Min], [Max])
        select [Id], [Id], @maxSeq+1, null from [Branch] where [Name] = @branchName;
end;
go
grant execute on [FastForward] to branching
go


create or alter procedure [GarbageCollect]() as begin
    delete from Range where Min=Max;
    delete from Range where Branch not in (select Branch from Branch);
    delete e from Event e
    where not exists (select 1 from Range r
        where e.Sequence >= r.Min and (r.Max is null or e.Sequence < r.Max) and e.Branch = r.SubBranch);
end
go
grant execute on [GarbageCollect] to branching
go