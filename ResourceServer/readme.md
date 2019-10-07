dotnet restore

dotnet ef migrations add dbdataeventrecord --context DataEventRecordContext

dotnet ef database update --context DataEventRecordContext

