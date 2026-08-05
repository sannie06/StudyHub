using System;

namespace StudyHub.Domain.Entities
{
    public class CauHinhHeThong
    {
        public int MaCauHinh { get; set; }
        public string TenCauHinh { get; set; }
        public string GiaTri { get; set; }
        public string MoTa { get; set; }
        public DateTime NgayCapNhat { get; set; } = DateTime.Now;
    }
}
