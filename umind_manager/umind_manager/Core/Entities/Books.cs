namespace umind_manager.Core.Entities
{
    public class Books
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Sumary { get; set; }
        public string PathPdf { get; set; }
        public string PathCape { get; set; }
        public bool Active { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
    }
}
