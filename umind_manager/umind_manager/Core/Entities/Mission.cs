namespace umind_manager.Core.Entities
{
    public class Mission
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Level { get; set; }
    }
}
