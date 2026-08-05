$connectionString = "Server=localhost;Database=StudyHubDb;Trusted_Connection=True;TrustServerCertificate=True;"

$sql = @"
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('NguoiDung') AND name = 'EmailConfirmationToken')
    ALTER TABLE NguoiDung ADD EmailConfirmationToken NVARCHAR(MAX) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('NguoiDung') AND name = 'EmailOtpCode')
    ALTER TABLE NguoiDung ADD EmailOtpCode NVARCHAR(MAX) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('NguoiDung') AND name = 'IsEmailConfirmed')
    ALTER TABLE NguoiDung ADD IsEmailConfirmed BIT NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('NguoiDung') AND name = 'OtpExpiresAt')
    ALTER TABLE NguoiDung ADD OtpExpiresAt DATETIME2 NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('NguoiDung') AND name = 'PasswordResetOtp')
    ALTER TABLE NguoiDung ADD PasswordResetOtp NVARCHAR(MAX) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('NguoiDung') AND name = 'ResetOtpExpiresAt')
    ALTER TABLE NguoiDung ADD ResetOtpExpiresAt DATETIME2 NULL;
"@

Write-Host "Connecting to SQL Server ($connectionString)..."
try {
    $conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $sql
    $cmd.ExecuteNonQuery()
    $conn.Close()
    Write-Host "[SQL SUCCESS] Columns successfully added to NguoiDung table in StudyHubDb!"
} catch {
    Write-Host "[SQL ERROR] Failed with System.Data.SqlClient: " $_.Exception.Message
    try {
        $conn = New-Object Microsoft.Data.SqlClient.SqlConnection($connectionString)
        $conn.Open()
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $sql
        $cmd.ExecuteNonQuery()
        $conn.Close()
        Write-Host "[SQL SUCCESS] Columns successfully added to NguoiDung table via Microsoft.Data.SqlClient!"
    } catch {
        Write-Host "[SQL ERROR] Failed with Microsoft.Data.SqlClient: " $_.Exception.Message
    }
}
