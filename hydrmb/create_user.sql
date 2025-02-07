if object_id('tempdb..#cteuser') is not null 
drop table #cteuser;

with cte as (
select distinct username,userpost,userlevel = cast (userlevel as int) from 
(
select distinct username=DraftedBy,userpost=DraftedByPost,userlevel=18 from data 
union all
select distinct EndorsedBy,EndorsedByPost,userlevel=9 from data
union all
select distinct AuditedBy,AuditedByPost,userlevel=6 from data
) a
), cteb as (
select 
cnt=count(*) over (partition by username order by cast(userpost as nvarchar(255))), username, userpost,userlevel
from cte 
)
select * into #cteuser from cteb

delete from userdata;
insert userdata(username,userpost,userlevel)
select username,userpost,userlevel from #cteuser where username is not null and userpost is not null and cnt = 1;
insert userdata(username,userpost,userlevel)
select username,case when userlevel = 6 then 'auditor' when userlevel = 9 then 'endorser' else 'drafter' end,userlevel from #cteuser where cnt =1 and userpost is null and username is not null 
and not exists(select * from userdata where username = #cteuser.username);

with cte as
(
select  username,post=coalesce(d.DraftedBypost, d.EndorsedBypost ,d.AuditedBypost), rowid=ROW_NUMBER() over (partition by username order by coalesce(d.AuditedAt,d.endorsedat,d.draftedat) desc)   from #cteuser u join data d on u.username = coalesce(d.DraftedBy, d.EndorsedBy ,d.AuditedBy)
where cnt > 1 

) , ye as 
(
select username,post=max(post),mrwid=max(rowid) from cte 
where post is not null 
group by username
)
update ud
set ud.userpost =case when CHARINDEX(' ',ye.post,1) >1 then stuff(ye.post,1,CHARINDEX(' ',ye.post,1),'') else ye.post end
from ye join userdata ud on ye.username = ud.username
