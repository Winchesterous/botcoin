declare @name as varchar(300), @type varchar(300), @sql as varchar(1000)

declare obj_list cursor for 
	select
		x.ObjectName,
		x.ObjectType
	from
	(			
		select '[' + s.name + '].[' + o.name + ']' ObjectName,
			   case
				 when o.type in ('P', 'RF', 'PC') then 'procedure'
				 when o.type in ('TF', 'FN', 'IF', 'FS', 'FT') then 'function'
				 when o.type in ('V') then 'view'
				 else o.type
			   end
			   as ObjectType
		from sys.all_objects o
			 inner join sys.schemas s on (o.schema_id = s.schema_id)
		where o.type in ('P', 'RF', 'PC', 'TF', 'FN', 'IF', 'FS', 'FT', 'V') and 
			  cast( case
					 when o.is_ms_shipped = 1 then 1
					 when (select major_id
							from sys.extended_properties 
							where major_id = o.object_id and 
								  minor_id = 0 and 
								  class = 1 and 
								  name = N'microsoft_database_tools_support'
						   ) is not null then 1
					 else 0
				   end          
				 as bit) = 0
		and
			s.name = 'dbo' or s.name = 'test'
	)x		

open obj_list
fetch next from obj_list into @name, @type

while (@@fetch_status = 0)
begin
	set @sql = 'drop ' + @type + ' ' + @name
	print @sql
	exec (@sql)
	
	fetch next from obj_list into @name, @type
end

close obj_list
deallocate obj_list

--IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'test')
--	DROP SCHEMA [test]

GO