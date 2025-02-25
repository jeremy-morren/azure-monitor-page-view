
#Postgresql (Pagila)

$link = "https://raw.githubusercontent.com/neondatabase/postgres-sample-dbs/main/pagila.sql"
$tmp = Join-Path ([System.IO.Path]::GetTempPath()) 'pagila.sql'
Invoke-WebRequest -Uri $link -OutFile $tmp

if ($IsWindows) {
    Set-Alias "psql" "${env:ProgramFiles}\PostgreSQL\16\bin\psql.exe"
}
$env:PGPASSWORD = 'postgres'
psql -U postgres -d postgres -c "CREATE DATABASE pagila;"
psql -U postgres -d pagila -f $tmp --quiet

#SQL Server (AdventureWorksLT2017)

$link = 'https://github.com/Microsoft/sql-server-samples/releases/download/adventureworks/AdventureWorksLT2017.bak'

$tmp = Join-Path ([System.IO.Path]::GetTempPath()) 'AdventureWorksLT2017.bak'

Invoke-WebRequest -Uri $link -OutFile $tmp

$folder = Join-Path ([System.Environment]::GetFolderPath("UserProfile")) "mssql"
New-Item -Path $folder -ItemType Directory -ErrorAction SilentlyContinue

$sql = @"
RESTORE DATABASE [AdventureWorks2017]
FROM DISK = N'${tmp}'
WITH
    FILE = 1,
    NOUNLOAD,
    STATS = 5,
    MOVE N'AdventureWorksLT2012_Data' TO N'${folder}\AdventureWorksLT2017.mdf',
    MOVE N'AdventureWorksLT2012_Log' TO N'${folder}\AdventureWorksLT2017_log.ldf';
"@

SqlLocalDB.exe start
$conn = New-Object System.Data.SqlClient.SqlConnection
$conn.ConnectionString = 'Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=True'
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = $sql
$cmd.ExecuteNonQuery()
$conn.Close()