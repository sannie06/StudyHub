namespace StudyHub.Domain.Entities
{
    public class TepDinhKemTinNhan
    {
        public int MaTep { get; set; }
        public int MaTinNhan { get; set; }
        public virtual TinNhan TinNhan { get; set; }
        
        public int MaFile { get; set; }
        public virtual FileTaiLen FileTaiLen { get; set; }
    }
}
