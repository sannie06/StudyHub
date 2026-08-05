using System;
using Microsoft.Data.SqlClient;

namespace DbFixer
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] connStrings = new[]
            {
                "Server=localhost;Database=StudyHubDb;Trusted_Connection=True;TrustServerCertificate=True;",
                "Server=(local);Database=StudyHubDb;Trusted_Connection=True;TrustServerCertificate=True;",
                "Server=.\\SQLEXPRESS;Database=StudyHubDb;Trusted_Connection=True;TrustServerCertificate=True;"
            };

            SqlConnection conn = null;
            foreach (var cs in connStrings)
            {
                try
                {
                    conn = new SqlConnection(cs);
                    conn.Open();
                    Console.WriteLine($"[Connected] Successfully connected using: {cs}");
                    break;
                }
                catch
                {
                    conn?.Dispose();
                    conn = null;
                }
            }

            if (conn == null)
            {
                Console.WriteLine("[ERROR] Could not connect to SQL Server database StudyHubDb!");
                return;
            }

            using (conn)
            {
                string[] ddlCommands = new[]
                {
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LichHoc') AND name = 'TieuDe') ALTER TABLE LichHoc ADD TieuDe NVARCHAR(255) NULL;",
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LichThi') AND name = 'TieuDe') ALTER TABLE LichThi ADD TieuDe NVARCHAR(255) NULL;",
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LichThi') AND name = 'GiangVien') ALTER TABLE LichThi ADD GiangVien NVARCHAR(100) NULL;",
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SuKien') AND name = 'MaMonHoc') ALTER TABLE SuKien ADD MaMonHoc INT NULL;",
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SuKien') AND name = 'GiangVien') ALTER TABLE SuKien ADD GiangVien NVARCHAR(100) NULL;",
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SuKien') AND name = 'HinhThucThi') ALTER TABLE SuKien ADD HinhThucThi NVARCHAR(100) NULL;",
                    @"UPDATE SuKien
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
                      WHERE MoTa LIKE N'%Giảng viên:%' OR MoTa LIKE N'%Môn học:%';",
                    "UPDATE SuKien SET MoTa = N'' WHERE MoTa LIKE N'Môn học:%Giảng viên:%';"
                };

                foreach (var sql in ddlCommands)
                {
                    try
                    {
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SQL Error] {ex.Message}");
                    }
                }

                Console.WriteLine("SUCCESS: All columns created and data cleaned up in StudyHubDb!");
            }
        }
    }
}
