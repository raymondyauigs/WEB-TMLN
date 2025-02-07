

INSERT WalkwayBFAExit([BfaRecordId], [ExitNo], [ExitToBuild], [AtExitFor], [ReasonwNoBFA], [DescwBFA], [AlterMeanswBFA], [AlterDetailwBFA], [AlterDist], [Remarks], [ExitwBFA],  [UpdatedAt],[UpdatedBy])
select r.Id,'A',d.ExitAToBuild,d.AtExitForA,d.ReasonAwNoBFA,d.DescAwBFA,d.AlterMeansAwBFA,d.AlterDetailAwBFA,d.AlterDistA,d.RemarksA,d.ExitAwBFA,
coalesce(d.AuditedAt,d.EndorsedAt,d.DraftedAt),coalesce(d.AuditedBy,d.EndorsedBy,d.DraftedBy)
from data d join WalkwayBFARecord r on d.Id = r.ImportKey
where d.ExitCnt >=2
UNION ALL
select r.Id,'B',d.ExitBToBuild,d.AtExitForB,d.ReasonBwNoBFA,d.DescBwBFA,d.AlterMeansBwBFA,d.AlterDetailBwBFA,d.AlterDistB,d.RemarksB,d.ExitBwBFA,
coalesce(d.AuditedAt,d.EndorsedAt,d.DraftedAt),coalesce(d.AuditedBy,d.EndorsedBy,d.DraftedBy)
from data d join WalkwayBFARecord r on d.Id = r.ImportKey
where d.ExitCnt >=2
UNION ALL
select r.Id,'C',d.ExitCToBuild,d.AtExitForC,d.ReasonCwNoBFA,d.DescCwBFA,d.AlterMeansCwBFA,d.AlterDetailCwBFA,d.AlterDistC,d.RemarksC,d.ExitCwBFA,
coalesce(d.AuditedAt,d.EndorsedAt,d.DraftedAt),coalesce(d.AuditedBy,d.EndorsedBy,d.DraftedBy)
from data d join WalkwayBFARecord r on d.Id = r.ImportKey
where d.ExitCnt >=3
UNION ALL
select r.Id,'D',d.ExitDToBuild,d.AtExitForD,d.ReasonDwNoBFA,d.DescDwBFA,d.AlterMeansDwBFA,d.AlterDetailDwBFA,d.AlterDistD,d.RemarksD,d.ExitDwBFA,
coalesce(d.AuditedAt,d.EndorsedAt,d.DraftedAt),coalesce(d.AuditedBy,d.EndorsedBy,d.DraftedBy)
from data d join WalkwayBFARecord r on d.Id = r.ImportKey
where d.ExitCnt >=4
UNION ALL
select r.Id,'E',d.ExitEToBuild,d.AtExitForE,d.ReasonEwNoBFA,d.DescEwBFA,d.AlterMeansEwBFA,d.AlterDetailEwBFA,d.AlterDistE,d.RemarksE,d.ExitEwBFA,
coalesce(d.AuditedAt,d.EndorsedAt,d.DraftedAt),coalesce(d.AuditedBy,d.EndorsedBy,d.DraftedBy)
from data d join WalkwayBFARecord r on d.Id = r.ImportKey
where d.ExitCnt >=5
UNION ALL
select r.Id,'F',d.ExitFToBuild,d.AtExitForF,d.ReasonFwNoBFA,d.DescFwBFA,d.AlterMeansFwBFA,d.AlterDetailFwBFA,d.AlterDistF,d.RemarksF,d.ExitFwBFA,
coalesce(d.AuditedAt,d.EndorsedAt,d.DraftedAt),coalesce(d.AuditedBy,d.EndorsedBy,d.DraftedBy)
from data d join WalkwayBFARecord r on d.Id = r.ImportKey
where d.ExitCnt >=6
UNION ALL
select r.Id,'G',d.ExitGToBuild,d.AtExitForG,d.ReasonGwNoBFA,d.DescGwBFA,d.AlterMeansGwBFA,d.AlterDetailGwBFA,d.AlterDistG,d.RemarksG,d.ExitGwBFA,
coalesce(d.AuditedAt,d.EndorsedAt,d.DraftedAt),coalesce(d.AuditedBy,d.EndorsedBy,d.DraftedBy)
from data d join WalkwayBFARecord r on d.Id = r.ImportKey
where d.ExitCnt >=7
UNION ALL
select r.Id,'H',d.ExitHToBuild,d.AtExitForH,d.ReasonHwNoBFA,d.DescHwBFA,d.AlterMeansHwBFA,d.AlterDetailHwBFA,d.AlterDistH,d.RemarksH,d.ExitHwBFA,
coalesce(d.AuditedAt,d.EndorsedAt,d.DraftedAt),coalesce(d.AuditedBy,d.EndorsedBy,d.DraftedBy)
from data d join WalkwayBFARecord r on d.Id = r.ImportKey
where d.ExitCnt >=8
UNION ALL
select r.Id,'J',d.ExitJToBuild,d.AtExitForJ,d.ReasonJwNoBFA,d.DescJwBFA,d.AlterMeansJwBFA,d.AlterDetailJwBFA,d.AlterDistJ,d.RemarksJ,d.ExitJwBFA,
coalesce(d.AuditedAt,d.EndorsedAt,d.DraftedAt),coalesce(d.AuditedBy,d.EndorsedBy,d.DraftedBy)
from data d join WalkwayBFARecord r on d.Id = r.ImportKey
where d.ExitCnt >=9
UNION ALL
select r.Id,'K',d.ExitKToBuild,d.AtExitForK,d.ReasonKwNoBFA,d.DescKwBFA,d.AlterMeansKwBFA,d.AlterDetailKwBFA,d.AlterDistK,d.RemarksK,d.ExitKwBFA,
coalesce(d.AuditedAt,d.EndorsedAt,d.DraftedAt),coalesce(d.AuditedBy,d.EndorsedBy,d.DraftedBy)
from data d join WalkwayBFARecord r on d.Id = r.ImportKey
where d.ExitCnt >=10

