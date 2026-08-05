$connectionString = "Server=localhost;Database=StudyHubDb;Trusted_Connection=True;TrustServerCertificate=True;"

$sqlScript = @"
-- 1. Add columns to LichHoc if not exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LichHoc') AND name = 'TieuDe')
BEGIN
    ALTER TABLE LichHoc ADD TieuDe NVARCHAR(255) NULL;
END

-- 2. Add columns to LichThi if not exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LichThi') AND name = 'TieuDe')
BEGIN
    ALTER TABLE LichThi ADD TieuDe NVARCHAR(255) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LichThi') AND name = 'GiangVien')
BEGIN
    ALTER TABLE LichThi ADD GiangVien NVARCHAR(100) NULL;
END

-- 3. Add columns to SuKien if not exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SuKien') AND name = 'MaMonHoc')
BEGIN
    ALTER TABLE SuKien ADD MaMonHoc INT NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SuKien') AND name = 'GiangVien')
BEGIN
    ALTER TABLE SuKien ADD GiangVien NVARCHAR(100) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SuKien') AND name = 'HinhThucThi')
BEGIN
    ALTER TABLE SuKien ADD HinhThucThi NVARCHAR(100) NULL;
END

-- Add Foreign Key for SuKien.MaMonHoc if not exists
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SuKien_MonHoc_MaMonHoc')
BEGIN
    ALTER TABLE SuKien ADD CONSTRAINT FK_SuKien_MonHoc_MaMonHoc FOREIGN KEY (MaMonHoc) REFERENCES MonHoc(MaMonHoc) ON DELETE SET NULL;
END

-- 4. Clean up existing data in SuKien where MoTa has pipe '|' formatting
UPDATE SuKien
SET 
    GiangVien = CASE 
        WHEN MoTa LIKE N'%Giảng viên:%' THEN 
            LTRIM(RTRIM(SUBSTRING(
                MoTa, 
                CHARINDEX(N'Giảng viên:', MoTa) + 11,
                CASE 
                    WHEN CHARINDEX(N'|', MoTa, CHARINDEX(N'Giảng viên:', MoTa)) > 0 
                    THEN CHARINDEX(N'|', MoTa, CHARINDEX(N'Giảng viên:', MoTa)) - (CHARINDEX(N'Giảng viên:', MoTa) + 11)
                    ELSE LEN(MoTa)
                END
            )))
        ELSE GiangVien 
    END,
    HinhThucThi = CASE 
        WHEN MoTa LIKE N'%Hình thức:%' THEN 
            LTRIM(RTRIM(SUBSTRING(
                MoTa, 
                CHARINDEX(N'Hình thức:', MoTa) + 10,
                CASE 
                    WHEN CHARINDEX(N'|', MoTa, CHARINDEX(N'Hình thức:', MoTa)) > 0 
                    THEN CHARINDEX(N'|', MoTa, CHARINDEX(N'Hình thức:', MoTa)) - (CHARINDEX(N'Hình thức:', MoTa) + 10)
                    ELSE LEN(MoTa)
                END
            )))
        ELSE HinhThucThi 
    END
WHERE MoTa LIKE N'%Giảng viên:%' OR MoTa LIKE N'%Môn học:%';

-- Clean up MoTa column text for rows with piped string
UPDATE SuKien
SET MoTa = N''
WHERE MoTa LIKE N'Môn học:%Giảng viên:%';
"@

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    $command = $connection.CreateCommand()
    $command.CommandText = $sqlScript
    $command.ExecuteNonQuery() | Out-Null
    $connection.Close()
    Write-Host "SUCCESS: Database schema upgraded and existing SuKien rows cleaned up successfully!"
} catch {
    Write-Host "ERROR upgrading database:" $_.Exception.Message
}
