using System;

namespace StudyHub.Domain.Common
{
    public abstract class BaseEntity
    {
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? NgayCapNhat { get; set; }
        public bool DaXoa { get; set; } = false;
    }
}
